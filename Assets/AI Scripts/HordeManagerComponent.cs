/*******************************************************************************/
/*!
\file   HordeManager.cs
\author Khan Sweetman
\par    All content © 2016-2017 DigiPen (USA) Corporation, all rights reserved.
\par    The Bakery
 
\brief
  ...
 
*/
/*******************************************************************************/
//#define DEBUG_HORDE
//#define DEBUG_STALKERS

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// ---------------------------------------------- Horde Manager ----------------------------------------------- //
public static class HordeManager
{
  public struct SpawnQueueData
  {
    public StreamSpawner Spawner;
    public ZombieTypes Type;
    public PrefabPair Pair;

    public SpawnQueueData(StreamSpawner spawner, ZombieTypes type = ZombieTypes.Normal)
    {
      Spawner = spawner;
      Type = type;
      Pair = PrefabPair.Default;
    }

    public SpawnQueueData(StreamSpawner spawner, ZombieTypes type, PrefabPair pair)
    {
      Spawner = spawner;
      Type = type;
      Pair = pair;
    }
  }

  // ------------------------------------------------- Variables -------------------------------------------------- //
  public const int ZOMBIE_LIMIT = 100;

  public static HordeManagerComponent Worker;
  public static HashSet<AILogicHelper> Zombies = new HashSet<AILogicHelper>();
  public static List<StreamSpawner> ZombieSpawners = new List<StreamSpawner>();
  public static bool HordeActive { get { return HordeTimer > 0.0f || HordeTimer == -1f; } }
  public static float HordeTimer;
  private static IEnumerator SustainHordeCoroutine;
  public static int TotalZombiesSlain { get; private set; }
  internal static Queue<SpawnQueueData> SpawnQueue = new Queue<SpawnQueueData>();

  // ------------------------------------------------- Interface -------------------------------------------------- //
  public static void InstigateHorde(float time = 10.0f)
  {
    // Make it so that zombies are aggroed while HordeMode is active
    // Sick each zombie on a random player
    foreach (GBZombieLogicHelper zombie in Zombies)
    {
      zombie.GetComponent<Aggro>().SetTarget(Sensable.RegisteredObjects[Sensable.FactionEnum.Baker].RandomElement());
    }

    // Sustain horde
    if (SustainHordeCoroutine != null)
    {
      Worker.StopCoroutine(SustainHordeCoroutine);
    }
    SustainHordeCoroutine = SustainHorde(time);
    Worker.StartCoroutine(SustainHordeCoroutine);

#if DEBUG_HORDE
    Debug.Log("Horde started");
#endif
  }

  private static IEnumerator SustainHorde(float time)
  {
    HordeTimer = time;
    if (HordeTimer != -1f)
    {
      while (HordeTimer > 0.0f)
      {
        HordeTimer -= time == -1f ? 0.0f : Time.deltaTime;
        yield return null;
      }
    }

#if DEBUG_HORDE
    if (HordeTimer != -1f)
      Debug.Log("Horde ended");
#endif
  }

  public static void AmpUpStalkerLevel(int numFuzes)
  {
    // fuck it, just increase everything, no shits given
    // do it fo' reals next semester
    switch (numFuzes)
    {
      case 1:
        Worker.StalkerSpawnTime -= 1.0f;
        break;
      case 2:
        Worker.StalkerSpawnTime -= 1.0f;
        break;
      case 3:
        Worker.StalkerSpawnTime -= 1.0f;
        break;
    }
  }

  // Request to spawn a zombie
  public static void QueueAISpawn(StreamSpawner spawner, ZombieTypes type = ZombieTypes.Normal)
  {
    SpawnQueue.Enqueue(new SpawnQueueData(spawner, type));
  }

  // ------------------------------------------------- AI Tracking -------------------------------------------------- //
  public static void RegisterZombie(GBZombieLogicHelper zombie)
  {
    Zombies.Add(zombie);
  }

  public static void UnregisterZombie(AILogicHelper zombie)
  {
    Zombies.Remove(zombie);
  }

  public static void RegisterSpawner(StreamSpawner spawner)
  {
    ZombieSpawners.Add(spawner);
  }

  public static void UnregisterSpawner(StreamSpawner spawner)
  {
    ZombieSpawners.Remove(spawner);
  }

  public static void ReportAIDeath(AILogicHelper ai)
  {
    ++TotalZombiesSlain;
    UnregisterZombie(ai);
  }
}

// ------------------------------------------------- Horde Manager Component -------------------------------------------------- //
// HordeManagerComponent is here to be stuck on some kind of singleton in the level
// -Lets HordeManager have access to a MonoBehaviour to spawn shit in the level
// -Has configurable parameters for designers
public class HordeManagerComponent : MonoBehaviour
{
  // ------------------------------------------------- Variables -------------------------------------------------- //
  public PrefabPair GBZPrefab;
  public PrefabPair CroissantPrefab;
  public PrefabPair CakePrefab;
  public int NumStalkers;
  public int NumStalkersRandom;
  public float StalkerSpawnTime;
  public float StalkerSpawnTimeRandom;
  private bool StalkersActive = false;
  private float StalkerTimer;
  public static HordeManagerComponent Instance { get; private set; }
  private HordeManagerComponent InstanceData;

  // ------------------------------------------------- Interface -------------------------------------------------- //
  public void EnableStalkers()
  {
    StalkersActive = true;
  }

  // ------------------------------------------------- Life Cycle -------------------------------------------------- //
  public void OnEnable()
  {
    // Register prefabs
    RegisterPrefabs();
    if (HordeManager.Worker == null)
    {
      HordeManager.Worker = this;
      HordeManagerComponent.Instance = this;
    }
    else
    {
      Debug.LogError("Multiple HordeManagerComponents in scene. There should be only one.");
    }

    // Only server should run logic after this
    if (!Network.isClient)
    {
      StalkerTimer = StalkerSpawnTime + Random.Range(-StalkerSpawnTimeRandom, StalkerSpawnTimeRandom);
    }
    else
    {
      enabled = false;
    }
  }

  public void RegisterPrefabs()
  {
    //UnityEngine.Networking.ClientScene.RegisterSpawnHandler(GBZPrefab.serverAssetId, SpawnGBZ, UnspawnEnemy);
    //UnityEngine.Networking.ClientScene.RegisterSpawnHandler(CroissantPrefab.serverAssetId, SpawnCroissant, UnspawnEnemy);
    //UnityEngine.Networking.ClientScene.RegisterSpawnHandler(CakePrefab.serverAssetId, SpawnCake, UnspawnEnemy);
  }

  public void Update()
  {
    if (StalkersActive)
    {
      StalkerUpdate();
    }
    SpawnUpdate();

#if DEBUG_HORDE
    if (HordeManager.HordeActive)
    {
      Util.DrawCircle(Sensable.GetPlayer1().position + Vector3.up, Color.red, 2.0f);
    }
#endif
  }

  // ----------------------------------------- Custom Network Spawning ------------------------------------------ //
  public GameObject SpawnGBZ(Vector3 position, UnityEngine.Networking.NetworkHash128 assetId)
  {
    return Instantiate(GBZPrefab.ClientVersion, position, Quaternion.identity);
  }

  public GameObject SpawnCroissant(Vector3 position, UnityEngine.Networking.NetworkHash128 assetId)
  {
    return Instantiate(CroissantPrefab.ClientVersion, position, Quaternion.identity);
  }

  public GameObject SpawnCake(Vector3 position, UnityEngine.Networking.NetworkHash128 assetId)
  {
    return Instantiate(CakePrefab.ClientVersion, position, Quaternion.identity);
  }

  public void UnspawnEnemy(GameObject spawned)
  {
    Destroy(spawned);
  }

  // ------------------------------------------------- Updates -------------------------------------------------- //
  public void StalkerUpdate()
  {
    if (StalkerTimer > 0)
    {
      StalkerTimer -= Time.deltaTime;
      return;
    }
    StalkerTimer = StalkerSpawnTime + Random.Range(-StalkerSpawnTimeRandom, StalkerSpawnTimeRandom);

    // Find spawner that's close to players but not directly visible
    if (SpatialPartition.RelevantButUnoccupiedPartitions.Count > 0)
    {
      SpatialPartitionComponent targetPartition = SpatialPartition.RelevantButUnoccupiedPartitions.RandomElement();
      StreamSpawner spawner = targetPartition.OwnedSpawners.RandomElement();
      int numToSpawn = NumStalkers + Random.Range(-NumStalkersRandom, NumStalkersRandom);
      for (int i = 0; i < numToSpawn; ++i)
      {
        spawner.QueueAISpawn(ZombieTypes.Stalker);
      }
#if DEBUG_STALKERS
      Debug.Log("Spawned stalkers.");
      for(int i = 0; i < 3; ++i)
      {
        Util.DrawCircle(spawner.transform.position + Vector3.up * i, Color.red, 5.0f, 5.0f);
      }
#endif
    }
#if DEBUG_STALKERS
    else
    {
      Debug.Log("Did not spawn stalkers. No positions available.");
    }
#endif
  }

  public void SpawnUpdate()
  {
    // Act on one spawn request each frame
    if (HordeManager.SpawnQueue.Count != 0)
    {
      HordeManager.SpawnQueueData data = HordeManager.SpawnQueue.Dequeue();
      data.Spawner.SpawnAINow(data.Pair, data.Type);
    }
  }
}
