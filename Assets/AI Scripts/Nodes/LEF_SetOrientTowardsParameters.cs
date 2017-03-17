/*******************************************************************************/
/*!
\file   LEF_SetOrientTowardsParameters.cs
\author Khan Sweetman
\par    All content © 2015 DigiPen (USA) Corporation, all rights reserved.
\par    Golden Bullet Games
\brief
  Sets parameters of OrientTowardsTarget. Useful for conveying where an AI
  is facing.

*/
/*******************************************************************************/  

using UnityEngine;
using System.Collections;

[System.Serializable]
[RequireComponent(typeof(OrientTowardsTarget))]
public class LEF_SetOrientTowardsParameters : BTNode
{
  // ------------------------------------------------- Variables -------------------------------------------------- //
  private OrientTowardsTarget _OrientTowards;

  public GameObject Target;
  public string TargetBlackboardKey;
  public TargetTypes OrientType = TargetTypes.Unoriented;
  public float TurnSpeed = 90.0f;
  public bool XLocked = false;
  public bool YLocked = false;
  public bool ZLocked = false;

  // First parameter should be owner
  // Second parameter should be root
  public override void Initialize(object[] objs)
  {
    base.Initialize(objs);

    // Grab components
    _OrientTowards = Owner.GetComponent<OrientTowardsTarget>();
  }

  // ------------------------------------------------- Life Cycle -------------------------------------------------- //
  public override void EnterBehavior()
  {
    // Steering overrides OrientTowards sometimes, so we need to let it know that we're doing things
    Steering steering = Owner.GetComponent<Steering>();
    if (steering != null)
    {
      steering.LookMode = LookEnum.Unoriented;
    }

    // Set non-orientType parameters
    _OrientTowards.SetLocks(XLocked, YLocked, ZLocked);
    _OrientTowards.SetTargetType(OrientType);

    // If we don't want to change orientation type
    if (OrientType == TargetTypes.Unoriented)
    {
      _OrientTowards.SetTargetType(TargetTypes.Unoriented);
    }
    // OrientType specific behavior
    else if (OrientType == TargetTypes.GameObject)
    {
      // choose target through blackboard
      Target = Blackboard.GetEntryAsGameObject(TargetBlackboardKey);
      _OrientTowards.SetObjTarget(Target);
    }
    SetStatus(BT_Status.Success);
  }

  public override void ExitBehavior()
  {
    base.ExitBehavior();
    Target = null;
  }

  public override BT_Status Update()
  {
    // Null check
    return SetStatus(BT_Status.Success);
  }
}
