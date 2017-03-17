/*******************************************************************************/
/*!
\file   StreamSpawner.cs
\author Khan Sweetman
\par    All content © 2016-2017 DigiPen (USA) Corporation, all rights reserved.
\par    The Bakery

\brief
  Networked spawner code.
  
*/
/*******************************************************************************/
//#define DEBUG_HORDE

using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

[System.Serializable]
public class HordeConfig
{
  public int MaxZombies;
  public float TimeBetweenSpawns;
  public float SpawnTimeVariance;
  public float MaxTime;
  public float StartCount;
}

[System.Serializable]
public struct PrefabPair
{
  public static readonly PrefabPair Default = new PrefabPair();

  public GameObject ServerVersion;
  public GameObject ClientVersion;
  public NetworkHash128 serverAssetId
  {
    get
    {
      return ServerVersion.GetComponent<NetworkIdentity>().assetId;
    }
  }
  public NetworkHash128 clientAssetId
  {
    get
    {
      return ClientVersion.GetComponent<NetworkIdentity>().assetId;
    }
  }
}

public class StreamSpawner : MonoBehaviour
{
  // ------------------------------------------------- Variables -------------------------------------------------- //
  // Behavior Adjusters
  public PrefabPair EnemyToSpawn;
  public HordeConfig DefaultConfig;
  private HordeConfig currConfig;

  // Internal data
  private SpatialPartitionComponent OwnerPartition;

  // ------------------------------------------------- Life Cycle -------------------------------------------------- //
  void Start()
  {
    currConfig = DefaultConfig;
    GetComponent<Collider>().enabled = false;
    // Registration set-up
    HordeManager.RegisterSpawner(this);
  }

#if DEBUG_HORDE
  void OnDrawGizmos()
  {
    if (Application.isPlaying)
    {
      Color color = triggered ? Color.red : Color.white;
      Util.DrawCircle(transform.position + Vector3.up, color, 3);
    }
  }
#endif

  public void RegisterWithPartition(SpatialPartitionComponent partition)
  {
    OwnerPartition = partition;
  }

  // ------------------------------------------------- Spawning -------------------------------------------------- //
  public void QueueAISpawn(ZombieTypes type = ZombieTypes.Normal)
  {
    HordeManager.QueueAISpawn(this, type);
  }

  public void SpawnAINow(ZombieTypes type = ZombieTypes.Normal)
  {
    SpawnAINow(EnemyToSpawn, type);
  }

  public void SpawnAINow(PrefabPair prefab, ZombieTypes type = ZombieTypes.Normal)
  {
    if (HordeManager.Zombies.Count < HordeManager.ZOMBIE_LIMIT)
    {
      // Calculate random position
      Bounds bounds = GetComponent<Collider>().bounds;
      float x = Random.Range(bounds.min.x, bounds.max.x);
      float y = 2; Random.Range(bounds.min.y, bounds.max.y);
      float z = Random.Range(bounds.min.z, bounds.max.z);
      Vector3 spawnPos = new Vector3(x, y, z);

      // Calculate random rotation
      Quaternion spawnRot = Quaternion.LookRotation(Random.insideUnitCircle, Vector3.up);

      // Should only run on server
      GameObject enemyToSpawn = HordeManager.Worker.GBZPrefab.ServerVersion;
      GameObject enemy = Instantiate(enemyToSpawn, spawnPos, spawnRot);
      enemy.GetComponent<AILogicHelper>().Type = type;
      enemy.GetComponent<AILogicHelper>().RegisterWithSpatialPartition(OwnerPartition);
      //NetworkServer.Spawn(enemy);
    }
  }

  // ------------------------------------------------- Hordes -------------------------------------------------- //
  private void TriggerHorde()
  {
    StartCoroutine(TriggerHordeOverTime());
  }

  private IEnumerator TriggerHordeOverTime()
  {
    // Spawn starting zombies
    for (int i = 0; i < currConfig.StartCount; ++i)
    {
      QueueAISpawn();
    }

    // Spawn delayed zombies
    for (int i = 0; i < currConfig.MaxZombies - currConfig.MaxZombies; ++i)
    {
      float randTime = Random.Range(currConfig.TimeBetweenSpawns - currConfig.SpawnTimeVariance, currConfig.TimeBetweenSpawns + currConfig.SpawnTimeVariance);
      yield return new WaitForSeconds(randTime);
      QueueAISpawn();
    }
  }
}
