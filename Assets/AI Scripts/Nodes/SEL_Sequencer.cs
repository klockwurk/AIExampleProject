/*******************************************************************************/
/*!
\file   BT_Sequencer.cs
\author Khan Sweetman
\par    All content © 2015 DigiPen (USA) Corporation, all rights reserved.
\par    Golden Bullet Games
\brief
  File does does things. COOL things.
  
*/
/*******************************************************************************/

using UnityEngine;
using System.Collections;

public class SEL_Sequencer : BTNode
{
  /////////////////////////////////////// Public Interface ///////////////////////////////////////
  public override void EnterBehavior()
  {
    CurrIndex = 0;
    SetStatus(BT_Status.Running);
    Children[CurrIndex].EnterBehavior();
  }

  public override void EnterAtIndex(int index)
  {
    CurrIndex = index;
    SetStatus(BT_Status.Running);
  }

  public override void ExitBehavior()
  {
    // Call ExitBehavior on active children
    Children[CurrIndex].ExitBehavior();
    SetStatus(BT_Status.Fail);
  }

  /////////////////////////////////////// Per frame Functions ///////////////////////////////////////
  override public BT_Status Update ()
  {
    // Update current child
    BT_Status status = Children[CurrIndex].Update();
    if (status == BT_Status.Success)
    {
      ++CurrIndex;

      // Complete when all children have succeeded
      if (CurrIndex == Children.Count)
        return SetStatus(BT_Status.Success);

      Children[CurrIndex].EnterBehavior();
    }
    else if(status == BT_Status.Fail)
    {
      return SetStatus(BT_Status.Fail);
    }

    return CurrStatus;
  }
}
