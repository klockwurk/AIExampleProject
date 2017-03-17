/*******************************************************************************/
/*!
\file   CommandPrompt.cs
\author Khan Sweetman
\par    All content © 2015 DigiPen (USA) Corporation, all rights reserved.
\par    Golden Bullet Games

\brief
  File does does things. COOL things.
  
  COMMANDS:
    - kill (tag name)
      - Kill all objects by tag_name

  TODO:
    - Make into a singleton class
    - Autocomplete by tabbing
    - Make it look like a command line
      - Make a chat mode that looks like a chat mode
    - Halt player input when command line is open
    - Have a "chat log" history or whatever
    - COMMANDS
      - Help
      - Open/close Debug Window
      - GodMode
      - Wipe
      - Boss
      - Win
      - Lose
    - Change strings into a HashSet of CommandDatas
      - name of command  
      - List<string> validInputNames?
      - description
      - function pointer
      - Use for help command
    
  BUGS:
    - Submits input when pressing escape
    - KillByName() only kills one target, not all
  
*/
/*******************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.SceneManagement;

public class Command
{
  public string Name;
  public string Description;
  public CommandDel Function;

  public override string ToString()
  {
    return Name;
  }

  public delegate void CommandDel(string[] input);
}

public class CommandPrompt : MonoBehaviour
{
  // ------------------------------------------------- Variables -------------------------------------------------- //
  public static GameObject CmdPrompt;
  public static InputField TextField;
  public static Text OutputText;
  public static bool PromptOpen = false;
  public static TrieTree<Command> AllCommands;
  public static AIStats AIStatsConsole;

  private bool KillAuraActive;
  private IEnumerator KillAuraCoroutine;
  private static CommandPrompt Instance;

  // ------------------------------------------------ Life Cycle -------------------------------------------------- //
  public void OnEnable()
  {
    InitializeCommands();
    if (Instance == null)
    {
      Instance = this;
    }
    else
    {
      Debug.LogError("Two or more CommandPrompts are present in the scene. There can be only one.");
    }
    KillAuraCoroutine = UpdateKillAura();
  }

  private void InitializeCommands()
  {
    AllCommands = new TrieTree<Command>();

    {
      Command command = new Command();
      command.Name = "aistats";
      command.Description = "brings up a stats window about the ai.";
      command.Function = new Command.CommandDel(AIStatsFunction);
      AllCommands.Insert(command.Name, command);
    }

    {
      Command command = new Command();
      command.Name = "help";
      command.Description = "Describes how to use the command prompt.";
      command.Function = new Command.CommandDel(Help);
      AllCommands.Insert(command.Name, command);
    }

    {
      Command command = new Command();
      command.Name = "killaura";
      command.Description = "Starts/stops killing everything around you.";
      command.Function = new Command.CommandDel(ToggleKillAura);
      AllCommands.Insert(command.Name, command);
    }

    {
      Command command = new Command();
      command.Name = "loadlevel";
      command.Description = "Loads a level. Input needs to be a valid level name.";
      command.Function = new Command.CommandDel(LoadLevel);
      AllCommands.Insert(command.Name, command);
    }

    {
      Command command = new Command();
      command.Name = "partitions";
      command.Description = "Toggles drawing of spatial partitions.";
      command.Function = new Command.CommandDel(ToggleDrawPartitions);
      AllCommands.Insert(command.Name, command);
    }

    {
      Command command = new Command();
      command.Name = "reset";
      command.Description = "Resets the level";
      command.Function = new Command.CommandDel(Reset);
      AllCommands.Insert(command.Name, command);
    }

    {
      Command command = new Command();
      command.Name = "spawn";
      command.Description = "Spawns zombies from all nearby & valid spawners.";
      command.Function = new Command.CommandDel(SpawnZombies);
      AllCommands.Insert(command.Name, command);
    }

    {
      Command command = new Command();
      command.Name = "starthorde";
      command.Description = "Starts a horde.";
      command.Function = new Command.CommandDel(StartHorde);
      AllCommands.Insert(command.Name, command);
    }

    {
      Command command = new Command();
      command.Name = "wipe";
      command.Description = "Kills all zombies";
      command.Function = new Command.CommandDel(Wipe);
      AllCommands.Insert(command.Name, command);
    }
  }

  public void Update()
  {
    // Open/close command prompt
    if (Input.GetKeyDown(KeyCode.BackQuote))
    {
      if (CommandPrompt.PromptOpen == false)
        CommandPrompt.OpenPrompt();
      else
        CommandPrompt.ClosePrompt();
      CommandPrompt.PromptOpen = !CommandPrompt.PromptOpen;
    }
  }

  public static void OpenPrompt()
  {
    // Create, activate prompt
    CmdPrompt = (GameObject)Instantiate(Resources.Load("CommandCanvas"));
    TextField = CmdPrompt.GetComponentInChildren<InputField>();
    TextField.ActivateInputField();
    OutputText = CmdPrompt.transform.Find("OutputBackground").GetChild(0).GetComponent<Text>();
    OutputText.text = "";

    // Listen to text submission
    var se = new InputField.SubmitEvent();
    se.AddListener(CmdTextInput);
    TextField.onEndEdit = se;

    // Select prompt
    GUI.FocusControl("CommandInput");
  }

  public static void CmdTextInput(string arg)
  {
    if (arg != "`")
    {
      // Separate input into different variables
      // 0th word is arg
      string[] words = arg.Split(' ');

      // Call the appropriate function with input
      string command = words[0].ToLower();
      Command node = AllCommands.Get(command);
      if (node != null)
      {
        node.Function(words);
      }
      else
      {
        Debug.LogWarning("Invalid command prompt input: \"" + arg + "\". Use 'help' command for more info");
      }

      // Clear input
      TextField.text = "";

      // Keep text focus in prompt
      TextField.ActivateInputField();
      GUI.FocusControl("CmdInput");
    }
  }

  public static void ClosePrompt()
  {
    // Destroy command prompt
    Destroy(CmdPrompt);
  }

  // ------------------------------------------------- Commands -------------------------------------------------- //
  public static void AIStatsFunction(string[] input = null)
  {
    if (AIStatsConsole == null)
    {
      GameObject obj = (GameObject)Instantiate(Resources.Load("AIStatsConsole"));
      AIStatsConsole = obj.GetComponent<AIStats>();
    }
    else
    {
      Destroy(AIStatsConsole.gameObject);
    }
  }

  public static void ToggleDrawPartitions(string[] input = null)
  {
    SpatialPartition.DrawPartitions = !SpatialPartition.DrawPartitions;
  }

  public static void Help(string[] input = null)
  {
    // instructions on use
    List<TrieTree<Command>.TrieNode> nodes = AllCommands.GetAllFullNodes();

    // print all commands
    StringBuilder builder = new StringBuilder();
    foreach (TrieTree<Command>.TrieNode node in nodes)
    {
      builder.AppendLine(node.Value.Name + " - " + node.Value.Description);
    }
    OutputText.text = builder.ToString();
  }

  public static void Reset(string[] input = null)
  {
    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
  }

  public static void Wipe(string[] parameters = null)
  {
    // Dispatch damage event to all objects with tag
    string tag = parameters.Length >= 2 ? parameters[1] : null;
    if (tag == "ai" || tag == "AI" || tag == null)
    {
      tag = "GBZombie";
    }
    else if (tag == "player" || tag == "players")
    {
      tag = "Player";
    }
    GameObject[] targets = GameObject.FindGameObjectsWithTag(tag);
    GameObject player = Util.FindClosestWithTag("Player", Vector3.zero).gameObject;
    foreach (GameObject target in targets)
    {
      target.EventSend("Damage", new DamageInfo(9999.0f, player));
    }
  }

  public static void LoadLevel(string[] parameters)
  {
    SceneManager.LoadScene(parameters[1]);
  }

  public static void StartHorde(string[] input = null)
  {
    HordeManager.InstigateHorde(10.0f);
  }

  public static void ToggleKillAura(string[] input = null)
  {
    if (Instance.KillAuraActive)
    {
      Instance.StopCoroutine(Instance.KillAuraCoroutine);
    }
    else
    {
      Instance.KillAuraCoroutine = UpdateKillAura();
      Instance.StartCoroutine(Instance.KillAuraCoroutine);
    }
    Instance.KillAuraActive = !Instance.KillAuraActive;
  }

  public static void SpawnZombies(string[] input = null)
  {
    int multiplier = 1;
    if (input.Length >= 2)
    {
      int.TryParse(input[1], out multiplier);
    }
    foreach (List<SpatialPartitionComponent> partitions in SpatialPartition.OccupiedPartitions.Values)
    {
      foreach (SpatialPartitionComponent partition in partitions)
      {
        foreach (StreamSpawner spawner in partition.OwnedSpawners)
        {
          for (int i = 0; i < multiplier; ++i)
          {
            spawner.QueueAISpawn();
          }
        }
      }
    }
  }

  // ------------------------------------------------- Kill Aura Helpers -------------------------------------------------- //
  public static IEnumerator UpdateKillAura()
  {
    while (true)
    {
      // Find player
      Sensable player = Sensable.FindClosestWithFactionTo(Sensable.FactionEnum.Baker, Vector3.zero);

      // Kill the things around them
      Collider[] objects = Physics.OverlapSphere(player.transform.position, 10.0f);
      for (int i = 0; i < objects.Length; ++i)
      {
        if (Util.IsRelatedTo(objects[i], player.transform))
        {
          continue;
        }
        try
        {
          objects[i].gameObject.EventSend<DamageInfo>("Damage", new DamageInfo(100.0f, null));

        }
        catch
        {
          // lol, no
        }
      }
      yield return new WaitForSeconds(0.2f);
    }
  }
}
