/*******************************************************************************/
/*!
\file   LEF_MoveTo.cs
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

public class LEF_MoveTo : BTNode
{
  // ------------------------------------------------- Variables -------------------------------------------------- //
  private AIMovementController MovementController;

  public string TargetBBKey;
  public float MoveSpeed;

  // ------------------------------------------------- Life Cycle -------------------------------------------------- //
  public override void Initialize(object[] objs)
  {
    base.Initialize(objs);
    MovementController = Owner.GetComponent<AIMovementController>();
    MovementController.ArrivalEvent += OnMovementArrival;
  }

  public override void EnterBehavior()
  {
    SetStatus(BT_Status.Running);
    MovementController.MoveSpeed = MoveSpeed;
    MovementController.SetDestination(Blackboard.GetEntryAsVector3(TargetBBKey));
  }

  public override void ExitBehavior()
  {
    SetStatus(BT_Status.Fail);
    MovementController.AbortPathing();
  }

  public override BT_Status Update()
  {
    return CurrStatus;
  }

  private void OnMovementArrival(Vector3 destination)
  {
    SetStatus(BT_Status.Success);
  }
}
