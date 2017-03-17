/*******************************************************************************/
/*!
\file   LEF_Jump.cs
\author Khan Sweetman
\par    All content © 2016-2017 DigiPen (USA) Corporation, all rights reserved.
\par    The Bakery
 
\brief
  ...
 
*/
/*******************************************************************************/
//#define DEBUG_JUMP

using UnityEngine;
using System.Collections;

public class LEF_Jump : BTNode
{
  // ------------------------------------------------- Variables-------------------------------------------------- //
  public float JumpVelocity = 4.0f;
  private Steering SteeringComponent;

  // ------------------------------------------------- Life Cycle -------------------------------------------------- //
  // First parameter should be owner
  // Second parameter should be root
  public override void Initialize(object[] objs)
  {
    base.Initialize(objs);
    SteeringComponent = Owner.GetComponent<Steering>();
#if DEBUG_JUMP
    if (SteeringComponent == null)
    {
      Debug.LogError("AI ERROR: SteeringComponent null in LEF_MoveRandomly in " + name + " on " + Owner.name);
    }
#endif
  }

  public override void EnterBehavior()
  {
    SteeringComponent.Jump(JumpVelocity);
    CurrStatus = BT_Status.Success;
  }

  public override BT_Status Update()
  {
    return CurrStatus;
  }
}