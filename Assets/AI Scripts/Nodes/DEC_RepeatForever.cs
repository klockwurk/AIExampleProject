/*******************************************************************************/
/*!
\file   DEC_RepeatForever.cs
\author Khan Sweetman
\par    All content © 2015 DigiPen (USA) Corporation, all rights reserved.
\par    Golden Bullet Games
\brief
  File does does things. COOL things.
  
*/
/*******************************************************************************/
using UnityEngine;
using System.Collections;

public class DEC_RepeatForever : BTNode
{
  // ----------------------------------- Public Interface ----------------------------------- //
  public override void EnterBehavior()
  {
    Children[0].SetStatus(BT_Status.Entering);
    SetStatus(BT_Status.Running);
  }

  public override void ExitBehavior()
  {
    SetStatus(BT_Status.Fail);
    Children[0].ExitBehavior();
  }

  // ----------------------------------- Per frame Functions ----------------------------------- //
  override public BT_Status Update()
  {
    BT_Status status = Children[0].Update();
    if (status == BT_Status.Fail || status == BT_Status.Success)
      Children[0].SetStatus(BT_Status.Entering);
    return SetStatus(BT_Status.Running);
  }
}
