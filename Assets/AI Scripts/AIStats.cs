/*******************************************************************************/
/*!
\file   AIStats.cs
\author Khan Sweetman
\par    All content © 2017 DigiPen (USA) Corporation, all rights reserved.
\par    The Bakery

\brief
  ...
  
*/
/*******************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class AIStats : MonoBehaviour
{
  // ------------------------------------------------- Variables -------------------------------------------------- //
  public Text PartitionsText;
  public Text ZombiesSlainText;
  public Text HordeText;
  StringBuilder Builder = new StringBuilder();

  // ------------------------------------------------- Life Cycle -------------------------------------------------- //
  void OnGUI()
  {
    PartitionsUI();
    ZombiesSlainUI();
    HordeUI();
  }

  // ------------------------------------------------- Helpers -------------------------------------------------- //
  private void PartitionsUI()
  {
    Builder.Remove(0, Builder.Length);
    Builder.AppendLine("Partitions:");
    foreach(SpatialPartitionComponent partition in SpatialPartition.AllPartitions)
    {
      Builder.AppendLine(partition.gameObject.name + " zombies: " + partition.ActiveAI.Count);
    }


    string str = "Current: ";
    foreach(List<SpatialPartitionComponent> thing in SpatialPartition.OccupiedPartitions.Values)
    {
      foreach(var other in thing)
      {
        str += other.name;
      }
    }

    Builder.AppendLine(str);
    Builder.AppendLine("Total: " + HordeManager.Zombies.Count);
    PartitionsText.text = Builder.ToString();
  }

  private void ZombiesSlainUI()
  {
    Builder.Remove(0, Builder.Length);
    Builder.AppendLine("Zombies Slain: " + HordeManager.TotalZombiesSlain);
    ZombiesSlainText.text = Builder.ToString();
  }

  private void HordeUI()
  {
    Builder.Remove(0, Builder.Length);
    Color color = Color.white;
    if (HordeManager.HordeActive)
    {
      color = Color.red;
      Builder.AppendLine("Horde is ACTIVE! Time left: " + HordeManager.HordeTimer);
    }
    else
    {
      Builder.AppendLine("Horde is inactive");
    }
    HordeText.text = Builder.ToString();
  }
}
