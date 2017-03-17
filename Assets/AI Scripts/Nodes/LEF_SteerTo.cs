/*******************************************************************************/
/*!
\file   LEF_SetSteeringParameters.cs
\author Khan Sweetman
\par    All content © 2015 DigiPen (USA) Corporation, all rights reserved.
\par    Golden Bullet Games
\brief
  TODO:
  - Need function for avoiding players
  
*/
/*******************************************************************************/

using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.AI;

[System.Serializable]
[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Steering))]
[RequireComponent(typeof(OrientTowardsTarget))]
public class LEF_SteerTo : BTNode
{
  // ---------------------------------------- Variables ---------------------------------------- //
  // Required components
  protected NavMeshAgent _NavMeshAgent;
  protected Rigidbody _RigidBody;
  protected Steering _Steering;
  protected OrientTowardsTarget _OrientTowards;

  // --------------------- Parameters --------------------- //
  // Misc parameters
  public LookEnum LookMode = LookEnum.AlongPath;
  public string TargetBBKey; // used if looking at target
  public float TurnSpeed = 360.0f;
  public float MoveSpeed = 25.0f;
  public float MaxSpeed = 6.0f;
  public float NearDist = 1.0f;
  // Avoidance parameters
  public float AvoidanceDistance = 8.0f;
  public float AvoidanceSpread = 5.0f;
  public float AvoidanceInstaVel = 0.1f;
  public int NumAvoidanceFeelers = 1;
  // Wander Parameters
  public float WanderDistance = 1.0f;
  public float WanderRadius = 1.0f;
  public float WanderJitter = 20.0f;
  // Weights
  public float AvoidanceWeight = 0.1f;
  public float PathingWeight = 1.0f;
  public float SeparationWeight = 0.0f;
  public float BrakingWeight = 0.3f;
  public float WanderWeight = 0.1f;
  public float RunAtWeight = 0.0f;

  // ------------------------------------------------- Event Stuff -------------------------------------------------- //
  protected virtual void OnArrival(Vector3 pos)
  {
    // Ignore event if we're not active
    if (CurrStatus != BT_Status.Running)
      return;

    SetStatus(BT_Status.Success);
  }

  // ------------------------------------------------- Life Cycle -------------------------------------------------- //
  // First parameter should be owner
  // Second parameter should be root
  public override void Initialize(object[] objs)
  {
    base.Initialize(objs);
    _RigidBody = Owner.GetComponent<Rigidbody>();
    _NavMeshAgent = Owner.GetComponent<UnityEngine.AI.NavMeshAgent>();
    _Steering = Owner.GetComponent<Steering>();
    _OrientTowards = Owner.GetComponent<OrientTowardsTarget>();

    // Set up event stuff
    _Steering.ArrivalEvent += OnArrival;
  }

  public override void EnterBehavior()
  {
    // Initialize target
    //BTree.Agent.StopCoroutine(_Steering.BrakingCoroutine());
    _Steering.CancelBraking();
    SetSteeringParameters();
    SetTarget();
    SetStatus(BT_Status.Running);
  }

  public override void ExitBehavior()
  {
    _Steering.Brake();
    SetStatus(BT_Status.Fail);
  }

  int Ticks = 0;
  public override BT_Status Update()
  {
    // Only update every once in a while
    if (Ticks > 0)
    {
      --Ticks;
      return CurrStatus;
    }
    Ticks = Random.Range(8, 16);

    // Update target position
    SetTarget();
    return CurrStatus;
  }

  // ------------------------------------------------- Helper Functions -------------------------------------------------- //
  protected void SetTarget()
  {
    object targ = Blackboard[TargetBBKey];
    if (targ.GetType() == typeof(Vector3))
    {
      Vector3 pos = Vector3.zero;
      pos = (Vector3)targ;
      _Steering.SetDestination(pos);
      _OrientTowards.SetPosTarget(pos);
    }
    else // assuming it's basically a GameObject
    {
      GameObject target = Blackboard.GetEntryAsGameObject(TargetBBKey);
      _Steering.SetDestination(target.transform.position);
      _OrientTowards.SetObjTarget(target);

      if (LookMode == LookEnum.TowardsFace)
      {
        Sensable sensable = target.GetComponent<Sensable>();
        if (sensable)
        {
          _OrientTowards.Offset = sensable.HeadOffset;
        }
      }
    }
  }

  protected virtual void SetSteeringParameters()
  {
    // Other parameters
    _Steering.LookMode = LookMode;
    _OrientTowards.TurnSpeed = TurnSpeed;
    _Steering.MoveSpeed = MoveSpeed;
    _Steering.MaxSpeed = MaxSpeed;
    _Steering.NearDist = NearDist;
    // Wander parameters
    _Steering.WanderDistance = WanderDistance;
    _Steering.WanderRadius = WanderRadius;
    _Steering.WanderJitter = WanderJitter;
    // Avoidance parameters
    _Steering.AvoidanceDistance = AvoidanceDistance;
    _Steering.AvoidanceSpread = AvoidanceSpread;
    _Steering.AvoidanceInstaVel = AvoidanceInstaVel;
    _Steering.NumAvoidanceFeelers = NumAvoidanceFeelers;
    // Weights
    _Steering.AvoidanceWeight = AvoidanceWeight;
    _Steering.PathingWeight = PathingWeight;
    _Steering.SeparationWeight = SeparationWeight;
    _Steering.BrakingWeight = BrakingWeight;
    _Steering.WanderWeight = WanderWeight;
    _Steering.RunAtWeight = RunAtWeight;
  }
}
