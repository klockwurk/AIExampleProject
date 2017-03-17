/*******************************************************************************/
/*!
\file   DEC_AlwaysSucceed.cs
\author Khan Sweetman
\par    All content © 2015 DigiPen (USA) Corporation, all rights reserved.
\par    Golden Bullet Games
\brief
  File does does things. COOL things.
  
*/
/*******************************************************************************/

using UnityEngine;
using System.Collections;

public class DEC_AlwaysSucceed : BTNode
{
  public bool OnlySucceedOnFinish = true;

  /////////////////////////////////////// Public Interface ///////////////////////////////////////
  public override void EnterBehavior()
  {
    Children[0].SetStatus(BT_Status.Entering);
    SetStatus(BT_Status.Running);
  }

  public override void ExitBehavior()
  {
    Children[0].ExitBehavior();
  }

  // ------------------------------------------------- Life Cycle -------------------------------------------------- //
  override public BT_Status Update ()
  {
    BT_Status status = Children[0].Update();
    if (OnlySucceedOnFinish)
    {
      if (status == BT_Status.Success || status == BT_Status.Fail)
        SetStatus(BT_Status.Success);
    }
    else
    {
      SetStatus(BT_Status.Success);
    }

    return CurrStatus;
  }
}
