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
public class LEF_Brake : BTNode
{
  // ------------------------------------------------- Variables -------------------------------------------------- //
  Steering _Steering;

  // ------------------------------------------------- Life Cycle -------------------------------------------------- //
  // First parameter should be owner
  // Second parameter should be root
  public override void Initialize(object[] objs)
  {
    base.Initialize(objs);

    _Steering = Owner.GetComponent<Steering>();
  }

  public override void EnterBehavior()
  {
    SetStatus(BT_Status.Success);

    // Make sure Owner looks at target
    _Steering.Brake();
  }

  public override void ExitBehavior()
  {
    SetStatus(BT_Status.Fail);
  }

  public override BT_Status Update()
  {
    return SetStatus(BT_Status.Success);
  }
}
