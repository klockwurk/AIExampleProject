/*******************************************************************************/
/*!
\file   AILogicHelper.cs
\author Khan Sweetman
\par    All content © 2017 DigiPen (USA) Corporation, all rights reserved.
\par    The Bakery
\brief
  Other AI logic helpers inherit from here
  
*/
/*******************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AILogicHelper : MonoBehaviour
{
  // ------------------------------------------------- Variables -------------------------------------------------- //
  public SpatialPartitionComponent OwnerPartition;
  protected Animator Anim;
  protected BlackboardComponent Blackboard;
  public ZombieTypes Type;

  // ------------------------------------------------- Inherited -------------------------------------------------- //
  public virtual void RegisterWithSpatialPartition(SpatialPartitionComponent partition)
  {
    OwnerPartition = partition;
    OwnerPartition.RegisterAI(gameObject);
  }

  public virtual void RegisterWithHordeManager()
  {
  }

  public virtual void Despawn()
  {
    Destroy(gameObject);
  }

  // ------------------------------------------------- Life Cycle -------------------------------------------------- //
  public void OnDestroy()
  {
    if (OwnerPartition != null)
    {
      OwnerPartition.UnregisterAI(gameObject);
    }
    HordeManager.UnregisterZombie(this);
  }

  public static void DestroyAILogic(GameObject ai)
  {
    // Bodies don't need networking
    Destroy(ai.GetComponent<NetworkAnimator>());
    Destroy(ai.GetComponent<NetworkTransform>());

    // Only the networked ai have logic on them
    if (!Network.isClient)
    {
      MonoBehaviour mono = ai.GetComponent<AggroFX>();
      if (mono != null) { Destroy(mono); }

      // Core things that all AI  have
      Destroy(ai.GetComponent<Steering>());
      Destroy(ai.GetComponent<OrientTowardsTarget>());
      Destroy(ai.GetComponent<Aggro>());
      Destroy(ai.GetComponent<Sensing>());
      Destroy(ai.GetComponent<Sensable>());
      Destroy(ai.GetComponent<BTAgent>());
      Destroy(ai.GetComponent<AILogicHelper>());
      Destroy(ai.GetComponent<BlackboardComponent>());
      Destroy(ai.GetComponent<UnityEngine.AI.NavMeshAgent>());
      Destroy(ai.GetComponent<Animator>());

      mono = ai.GetComponent<AIMelee>();
      if (mono != null) { Destroy(mono); }
      mono = ai.GetComponent<AIShoot>();
      if (mono != null) { Destroy(mono); }
    }
  }
}
