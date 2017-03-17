/*******************************************************************************/
/*!
\file   SpatialPartition.cs
\author Khan Sweetman
\par    All content © 2016-2017 DigiPen (USA) Corporation, all rights reserved.
\par    The Bakery
 
\brief
  ...
 
*/
/*******************************************************************************/
//#define DEBUG_LOG_PARTITIONS
#define DEBUG_DRAW_PARTITIONS
//#define DEBUG_REPOPULATION

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// ------------------------------------------------- Spatial Partition -------------------------------------------------- //
public static class SpatialPartition
{
  // ------------------------------------------------- Variables -------------------------------------------------- //
  public static bool DrawPartitions = true;

  public static HashSet<SpatialPartitionComponent> AllPartitions = new HashSet<SpatialPartitionComponent>();
  public static Dictionary<Transform, List<SpatialPartitionComponent>> OccupiedPartitions = new Dictionary<Transform, List<SpatialPartitionComponent>>();
  public static HashSet<SpatialPartitionComponent> RelevantPartitions
  {
    get
    {
      // Get directly occupied spaces and neighbors
      HashSet<SpatialPartitionComponent> partitions = new HashSet<SpatialPartitionComponent>();
      foreach (List<SpatialPartitionComponent> list in OccupiedPartitions.Values)
      {
        foreach (SpatialPartitionComponent partition in list)
        {
          partitions.Add(partition);
          foreach (SpatialPartitionComponent neighbor in partition.Neighbors)
          {
            if (!neighbor.GatedOff)
            {
              partitions.Add(neighbor);
            }
          }
        }
      }
      return partitions;
    }
  }

  public static HashSet<SpatialPartitionComponent> RelevantButUnoccupiedPartitions
  {
    get
    {
      // Collect neighbors
      HashSet<SpatialPartitionComponent> partitions = new HashSet<SpatialPartitionComponent>();
      foreach (List<SpatialPartitionComponent> list in OccupiedPartitions.Values)
      {
        foreach (SpatialPartitionComponent partition in list)
        {
          foreach (SpatialPartitionComponent neighbor in partition.Neighbors)
          {
            if (!neighbor.GatedOff)
            {
              partitions.Add(neighbor);
            }
          }
        }
      }

      // Remove directly occupied spaces
      foreach (List<SpatialPartitionComponent> list in OccupiedPartitions.Values)
      {
        foreach (SpatialPartitionComponent partition in list)
        {
          partitions.Remove(partition);
        }
      }
      return partitions;
    }
  }
  private static List<SpatialPartitionComponent> PartitionsWaitingToBeRemoved = new List<SpatialPartitionComponent>();

  internal static SpatialPartitionComponent[] LastFewRooms = new SpatialPartitionComponent[2];

  // ------------------------------------------------- Registration -------------------------------------------------- //
  public static void RegisterPartition(SpatialPartitionComponent partition)
  {
    AllPartitions.Add(partition);
  }

  public static void RegisterPlayer(Transform player)
  {
    if (!OccupiedPartitions.ContainsKey(player))
    {
      OccupiedPartitions.Add(player, new List<SpatialPartitionComponent>());
    }
  }

  public static void RegisterAI(AILogicHelper ai)
  {

  }

  // ------------------------------------------------- Player Position Tracking -------------------------------------------------- //
  public static void ReportTrigger(Collider col, SpatialPartitionComponent partition)
  {
    Transform player = col.transform;
    // edge case: triggering the room we're already in (expected behavior, results from gaps between triggers)
    if (LastFewRooms[0] == partition)
    {
      return;
    }

    // Make new lists as necessary
    if (!OccupiedPartitions.ContainsKey(player))
    {
      OccupiedPartitions.Add(player, new List<SpatialPartitionComponent>());
    }
    OccupiedPartitions[player].Add(partition);

    // check PositionsWaitingToBeRemoved
    for (int i = 0; i < OccupiedPartitions[player].Count; ++i)
    {
      if (PartitionsWaitingToBeRemoved.Contains(OccupiedPartitions[player][i]))
      {
        OccupiedPartitions[player].Remove(OccupiedPartitions[player][i]);
      }
    }

    // Track which few rooms we were in
    for (int i = LastFewRooms.Length - 1; i > 0; --i)
    {
      LastFewRooms[i] = LastFewRooms[i - 1];
    }
    LastFewRooms[0] = partition;

    // Repopulate
    RepopulateRooms();
#if DEBUG_LOG_PARTITIONS
    Debug.Log(col.gameObject.name + " triggered: " + partition.gameObject.name);
#endif
  }

  public static void ReportUntrigger(Collider col, SpatialPartitionComponent partition)
  {
    if (OccupiedPartitions[col.transform].Count > 1)
    {
      // Track unoccupied partitions
      OccupiedPartitions[col.gameObject.transform].Remove(partition);
      // Repopulate
      RepopulateRooms();
    }
    else
    {
      // if this partition is the only thing containing this player, wait until the player gets to another
      // partition before marking partition as unoccupied. This way, we always the player is always
      // counted as being in at least one partition.
      PartitionsWaitingToBeRemoved.Add(partition);
    }
  }

  private static void RepopulateRooms()
  {
    // depopulate every roomm that...
    // isn't relevant
    foreach (SpatialPartitionComponent partition in AllPartitions)
    {
      if (!RelevantPartitions.Contains(partition))
      {
        partition.Depopulate();
      }
    }

    // calculate direction player is moving in (what partition they came from, where they entered)
    // populate every room that...
    // is relevant
    // isn't directly occupied by the player
    // isn't already populated
    // isn't the one we just came from
    // isn't gated off
    foreach (SpatialPartitionComponent partition in RelevantButUnoccupiedPartitions)
    {
      if (!partition.GatedOff)
      {
        bool isValid = true;
        for (int i = 0; i < LastFewRooms.Length; ++i)
        {
          if (LastFewRooms[i] == partition)
          {
            isValid = false;
            break;
          }
        }
        if (!isValid)
        {
          continue;
        }
      }
      // Passed all checks, populate room
      partition.Populate();
    }

    // scale population on...
    // player progress (positive)
    // zombies killed (negative)
    // ...
  }
}

// ------------------------------------------------- Spatial Partition Component -------------------------------------------------- //
public class SpatialPartitionComponent : MonoBehaviour
{
  // ------------------------------------------------- Variables -------------------------------------------------- //
  public SpatialPartitionComponent[] Neighbors;
  [System.NonSerialized]
  public List<StreamSpawner> OwnedSpawners = new List<StreamSpawner>();
  public bool GatedOff; // used to hard deactivate if gated off or whatever
  public bool IsOccupied
  {
    get
    {
      foreach (List<SpatialPartitionComponent> list in SpatialPartition.OccupiedPartitions.Values)
      {
        if (list.Contains(this))
        {
          return true;
        }
      }
      return false;
    }
  }

  // AI Tracking
  public int TargetWalkers;
  private int WalkersKilled;
  public HashSet<GameObject> ActiveAI { get; private set; }

  // ------------------------------------------------- AI Spawning/Tracking -------------------------------------------------- //
  public void Ungate()
  {
    GatedOff = false;
  }

  public void Populate()
  {
    //int aiToSpawn = TargetWalkers - WalkersKilled - ActiveAI.Count;
    int aiToSpawn = TargetWalkers - ActiveAI.Count;
    for (int i = 0; i < aiToSpawn; ++i)
    {
      int randIndex = Random.Range(0, OwnedSpawners.Count);
      HordeManager.QueueAISpawn(OwnedSpawners[randIndex]);
    }
#if DEBUG_REPOPULATION
    Debug.Log("Populating(" + aiToSpawn + "): " + gameObject.name);
#endif
  }

  public void Depopulate()
  {
#if DEBUG_REPOPULATION
    Debug.Log("Depopulating:(" + ActiveAI.Count + ")" + gameObject.name);
#endif

    foreach (GameObject ai in ActiveAI)
    {
      Destroy(ai);
    }
    ActiveAI.Clear();
  }

  public void RegisterAI(GameObject ai)
  {
    ActiveAI.Add(ai);
  }

  public void UnregisterAI(GameObject ai)
  {
    ActiveAI.Remove(ai);
  }

  public void ReportAIDeath(GameObject ai)
  {
    ActiveAI.Remove(ai);

    // Only count as passive zombie death if horde isn't active
    if (!HordeManager.HordeActive)
    {
      ++WalkersKilled;
    }
  }

  // ------------------------------------------------- Life Cycle -------------------------------------------------- //
  public void Awake()
  {
    ActiveAI = new HashSet<GameObject>();
  }

  public void Start()
  {
    SpatialPartition.RegisterPartition(this);
    foreach (StreamSpawner spawner in GetComponentsInChildren<StreamSpawner>())
    {
      OwnedSpawners.Add(spawner);
      spawner.RegisterWithPartition(this);
    }
#if DEBUG_LOG_PARTITIONS
    if (Neighbors.Length == 0)
    {
      Debug.LogWarning("WARNING: Spatial partition did not have any neighbors: " + gameObject.name);
    }
    if (OwnedSpawners.Length == 0)
    {
      Debug.LogWarning("WARNING: Spatial partition did not have any spawners: " + gameObject.name);
    }
#endif
  }

  public void OnTriggerEnter(Collider col)
  {
    if (col.CompareTag("Player"))
    {
      SpatialPartition.ReportTrigger(col, this);
    }
    else if (col.CompareTag("Zombie"))
    {
      AILogicHelper logic = col.gameObject.GetComponent<AILogicHelper>();
      logic.OwnerPartition.ActiveAI.Remove(col.gameObject);
      logic.OwnerPartition = this;
      ActiveAI.Add(col.gameObject);
    }
  }

  public void OnTriggerExit(Collider col)
  {
    if (col.CompareTag("Player"))
    {
      SpatialPartition.ReportUntrigger(col, this);
    }
  }

  // ------------------------------------------------- UI -------------------------------------------------- //
  public void OnDrawGizmos()
  {
    if (Application.isPlaying && SpatialPartition.DrawPartitions)
    {
      Color color = Color.white;
      if (SpatialPartition.RelevantPartitions.Contains(this))
      {
        color = Color.yellow;
        if (IsOccupied)
        {
          color = Color.red;
        }
      }
      Color ogColor = Gizmos.color;
      Gizmos.color = color;
      Gizmos.DrawWireSphere(gameObject.transform.position, 5f);
      Gizmos.color = ogColor;

#if UNITY_EDITOR
      // Draw # occupying zombies
      Camera cam = Camera.main;
      Vector3 towardsCam = cam.transform.position - transform.position;
      if (Vector3.Dot(cam.transform.forward, towardsCam) < 0)
      {
        Vector3[] verts = 
        {
          transform.position - cam.transform.right + Vector3.up,
          transform.position + cam.transform.right + Vector3.up,
          transform.position + cam.transform.right - Vector3.up,
          transform.position - cam.transform.right - Vector3.up
        };
        UnityEditor.Handles.DrawSolidRectangleWithOutline(verts, Color.gray, color);
        GUIStyle style = new GUIStyle(UnityEditor.EditorStyles.label);
        style.normal.textColor = color;
        style.alignment = TextAnchor.UpperCenter;
        style.fontSize = 16;
        UnityEditor.Handles.Label(transform.position, "Active AI: " + ActiveAI.Count, style);
      }

      // Draw previously occupied rooms
      for(int i = 0; i < SpatialPartition.LastFewRooms.Length; ++i)
      {
        if (SpatialPartition.LastFewRooms[i] != null)
        {
          Gizmos.DrawIcon(SpatialPartition.LastFewRooms[i].transform.position + Vector3.up * 2.0f, "Previously Occupied Icon.png", true);
        }
      }
#endif
    }
    Gizmos.DrawIcon(transform.position, "Spatial Partition Icon.png", false);
  }

  void OnDrawGizmosSelected()
  {
    // Draw neighbor connections when selected
    Gizmos.color = Color.yellow;
    if (Neighbors != null)
    {
      foreach (SpatialPartitionComponent neighbor in Neighbors)
      {
        if (neighbor != null)
        {
          Gizmos.DrawLine(transform.position, neighbor.transform.position);
        }
      }
    }
  }
}
