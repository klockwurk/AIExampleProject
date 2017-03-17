/*******************************************************************************/
/*!
\file   Sensable.cs
\author Khan Sweetman
\par    All content © 2015 DigiPen (USA) Corporation, all rights reserved.
\par    Golden Bullet Games
\brief
  File does does things. COOL things.
  
*/
/*******************************************************************************/
//#define SENSE_DEBUG

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Sensable : MonoBehaviour
{
  // ------------------------------------------------- Statics -------------------------------------------------- //
  public static Dictionary<FactionEnum, HashSet<Sensable>> RegisteredObjects = new Dictionary<FactionEnum, HashSet<Sensable>>();

  // ------------------------------------------------- Sub-Classes -------------------------------------------------- //
  public enum FactionEnum
  {
    Evil,
    Baker
  }

  // ------------------------------------------------- Variables -------------------------------------------------- //
  public Vector3 HeadOffset = new Vector3(0.0f, 0.3f, 0.0f);
  public float Priority = 0.5f;
  public FactionEnum Faction;

  private static List<Transform> Players = new List<Transform>();

  // ------------------------------------------------- Life Cycle -------------------------------------------------- //
  void Awake()
  {
    Register();
#if SENSE_DEBUG
    GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    obj.transform.SetParent(gameObject.transform);
    obj.transform.localPosition = HeadOffset;
#endif
  }

  public void OnDestroy()
  {
    if (RegisteredObjects.ContainsKey(Faction))
    {
      RegisteredObjects[Faction].Remove(this);
    }
  }

  // ------------------------------------------------- Interface -------------------------------------------------- //
  public static bool IsEnemyTo(Sensable lhs, Sensable rhs)
  {
    return lhs.Faction != rhs.Faction;
  }

  public static bool IsEnemyTo(FactionEnum lhs, FactionEnum rhs)
  {
    return lhs != rhs;
  }

  public static List<FactionEnum> FindEnemyFactionsTo(Sensable sensable)
  {
    int length = System.Enum.GetValues(typeof(FactionEnum)).Length;
    List<FactionEnum> enemyFactions = new List<FactionEnum>();
    for (int i = 0; i < length; ++i)
    {
      if (IsEnemyTo((FactionEnum)i, sensable.Faction))
      {
        enemyFactions.Add((FactionEnum)i);
      }
    }
    return enemyFactions.Count != 0 ? enemyFactions : null;
  }

  public static List<FactionEnum> FindFriendlyFactionsTo(Sensable sensable)
  {
    int length = System.Enum.GetValues(typeof(FactionEnum)).Length;
    List<FactionEnum> friendlyFactions = new List<FactionEnum>();
    for (int i = 0; i < length; ++i)
    {
      if (!IsEnemyTo((FactionEnum)i, sensable.Faction))
      {
        friendlyFactions.Add((FactionEnum)i);
      }
    }
    return friendlyFactions.Count != 0 ? friendlyFactions : null;
  }

  public static Sensable FindClosestWithFactionTo(FactionEnum faction, Vector3 location)
  {
    if (RegisteredObjects.Count != 0 && RegisteredObjects.ContainsKey(faction) && RegisteredObjects[faction].Count != 0)
    {
      Sensable closest = RegisteredObjects[faction].GetEnumerator().Current;
      float closestDist = float.MaxValue;
      foreach (Sensable obj in RegisteredObjects[faction])
      {
        if (obj.Faction == faction)
        {
          float currDist = Util.DistSqr(obj.transform.position, location);
          if (currDist < closestDist)
          {
            closest = obj;
            closestDist = currDist;
          }
        }
      }
      return closest;
    }
    return null;
  }

  public static Transform GetPlayer1()
  {
    if (Players.Count > 0)
    {
      return Players[0].transform;
    }
    Players.Add(FindClosestWithFactionTo(FactionEnum.Baker, Vector3.zero).transform);
    return Players[0];
  }

  // ------------------------------------------------- Helpers -------------------------------------------------- //
  private void Register()
  {
    // Register
    if (!RegisteredObjects.ContainsKey(Faction))
    {
      RegisteredObjects.Add(Faction, new HashSet<Sensable>());
    }
    RegisteredObjects[Faction].Add(this);

    // Register w/spatial partitioning
    if (Faction == FactionEnum.Baker)
    {
      SpatialPartition.RegisterPlayer(transform);
    }
  }
}
