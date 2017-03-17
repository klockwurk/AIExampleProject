/*******************************************************************************/
/*!
\file   BT_RepeatUntilFail.cs
\author Khan Sweetman
\par    All content © 2015 DigiPen (USA) Corporation, all rights reserved.
\par    Golden Bullet Games
\brief
  File does does things. COOL things.
  
*/
/*******************************************************************************/

using UnityEngine;
using System.Collections;

public class DEC_RepeatUntilFail : BTNode
{
  /////////////////////////////////////// Public Interface ///////////////////////////////////////
  public override void EnterBehavior()
  {
    Children[0].SetStatus(BT_Status.Entering);
    SetStatus(BT_Status.Running);
  }

  public override void ExitBehavior()
  {
    // Call ExitBehavior on active children
    foreach(var child in Children)
    {
      if (child.CurrStatus == BT_Status.Running)
        child.ExitBehavior();
    }
  }

  /////////////////////////////////////// Per frame Functions ///////////////////////////////////////
  override public BT_Status Update ()
  {
    BT_Status status = Children[0].Update();
    if(status == BT_Status.Success)
      Children[0].SetStatus(BT_Status.Entering);
    else if(status == BT_Status.Fail)
      SetStatus(BT_Status.Fail);

    return CurrStatus;
  }
}
