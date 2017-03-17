/*******************************************************************************/
/*!
\file   DEC_Repeat.cs
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

public class DEC_Repeat : BTNode
{
  // ------------------------------------------------- Variables -------------------------------------------------- //
  public int Repetitions = 3;
  private int CurrRepetitions;

  // ------------------------------------------------- Life Cycle -------------------------------------------------- //
  public override void EnterBehavior()
  {
    CurrStatus = BT_Status.Running;
    CurrRepetitions = 0;
    Children[0].EnterBehavior();
  }

  public override void ExitBehavior()
  {
    CurrStatus = BT_Status.Fail;
    Children[0].ExitBehavior();
  }

  public override BT_Status Update()
  {
    BT_Status status = Children[0].Update();
    if (status == BT_Status.Success)
    {
      ++CurrRepetitions;
      if (CurrRepetitions == Repetitions)
      {
        CurrStatus = BT_Status.Success;
      }
      else
      {
        Children[0].EnterBehavior();
      }
    }
    else if (CurrStatus == BT_Status.Fail)
    {
      CurrStatus = BT_Status.Fail;
    }

    return CurrStatus;
  }
}
