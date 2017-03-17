/*******************************************************************************/
/*!
\file   LEF_Melee.cs
\author Khan Sweetman
\par    All content © 2015 DigiPen (USA) Corporation, all rights reserved.
\par    Golden Bullet Games
\brief
  File does does things. COOL things.
  
*/
/*******************************************************************************/

using UnityEngine;
using System.Collections;

[System.Serializable]
public class LEF_Melee : BTNode
{
  private AIMelee _Melee;
  private GameObject Target;

  // ------------------------------------------------- Life Cycle -------------------------------------------------- //
  // First parameter should be owner
  // Second parameter should be root
  public override void Initialize(object[] objs)
  {
    base.Initialize(objs);

    // Grab references
    _Melee = Owner.GetComponent<AIMelee>();
    _Melee.MeleeFinished += MeleeFinished;
  }

  public override void EnterBehavior()
  {
    // Swings blindly forward
    // Use other nodes to look at a target or whatever
    _Melee.Swing();
    SetStatus(BT_Status.Running);
  }

  public override void ExitBehavior()
  {
    _Melee.MeleeFinish();
    SetStatus(BT_Status.Fail);
  }

  public override BT_Status Update()
  {
    return CurrStatus;
  }

  // ------------------------------------------------- Event Stuff -------------------------------------------------- //
  // Succeed when we finish our animation
  void MeleeFinished()
  {
    SetStatus(BT_Status.Success);
  }
}
