/*******************************************************************************/
/*!
\file   DEC_InvertFinish.cs
\author Khan Sweetman
\par    All content © 2016-2017 DigiPen (USA) Corporation, all rights reserved.
\par    The Bakery
 
\brief
  ...
 
*/
/*******************************************************************************/
using UnityEngine;
using System.Collections;

public class DEC_InvertFinish : BTNode
{
  // ------------------------------------------------- Header -------------------------------------------------- //
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
  override public BT_Status Update()
  {
    BT_Status status = Children[0].Update();
    if (status == BT_Status.Fail)
    {
      status = BT_Status.Success;
      CurrStatus = status;
    }
    if (status == BT_Status.Success)
    {
      status = BT_Status.Fail;
      CurrStatus = status;
    }
    return CurrStatus;
  }
}
