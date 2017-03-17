/*******************************************************************************/
/*!
\file   LEF_SetSteeringParameters.cs
\author Khan Sweetman
\par    All content © 2016-2017 DigiPen (USA) Corporation, all rights reserved.
\par    The Bakery
 
\brief
  ...
 
*/
/*******************************************************************************/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LEF_SetSteeringParameters : BTNode
{
  // ---------------------------------------- Variables ---------------------------------------- //
  // Required components
  protected Steering _Steering;
  protected OrientTowardsTarget _OrientTowards;

  // --------------------- Parameters --------------------- //
  // Misc parameters
  public LookEnum LookMode = LookEnum.Unoriented;
  public float TurnSpeed = 360.0f;
  public float MoveSpeed = 25.0f;
  public float MaxSpeed = 6.0f;
  // ------------------------------------------------- Life Cycle -------------------------------------------------- //
  // First parameter should be owner
  // Second parameter should be root
  public override void Initialize(object[] objs)
  {
    base.Initialize(objs);
    _Steering = Owner.GetComponent<Steering>();
    _OrientTowards = Owner.GetComponent<OrientTowardsTarget>();
  }

  public override void EnterBehavior()
  {
    // Initialize target
    SetSteeringParameters();
    SetStatus(BT_Status.Success);
  }

  public override void ExitBehavior()
  {
    SetStatus(BT_Status.Fail);
  }

  public override BT_Status Update()
  {
    return SetStatus(BT_Status.Success);
  }

  protected virtual void SetSteeringParameters()
  {
    // Other parameters
    _OrientTowards.TurnSpeed = TurnSpeed;
    _Steering.MoveSpeed = MoveSpeed;
    _Steering.MaxSpeed = MaxSpeed;
  }
}
