/*******************************************************************************/
/*!
\file   BT_Selector.cs
\author Khan Sweetman
\par    All content © 2015 DigiPen (USA) Corporation, all rights reserved.
\par    Golden Bullet Games
\brief
  ...
  
*/
/*******************************************************************************/

using UnityEngine;
using System.Collections;

public class SEL_Selector : BTNode
{
  // ------------------------------------------------- Life Cycle -------------------------------------------------- //
  public override void EnterBehavior()
  {
#if UNITY_EDITOR
    if (HasBreakpoint)
    {
      UnityEditor.EditorApplication.isPaused = true;
    }
#endif
    CurrIndex = 0;
    SetStatus(BT_Status.Running);
    Children[CurrIndex].SetStatus(BT_Status.Entering);
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

  override public BT_Status Update ()
  {
    // Try children until one succeeds
    BT_Status status = Children[CurrIndex].Update();
    if (status == BT_Status.Fail)
    {
      ++CurrIndex;
      SetStatus(BT_Status.Running);

      // Fail when all children have failed
      if (CurrIndex == Children.Count)
        return SetStatus(BT_Status.Fail);

      Children[CurrIndex].SetStatus(BT_Status.Entering);
    }
    // Exit when the first child succeeds
    else if(status == BT_Status.Success)
    {
      return SetStatus(BT_Status.Success);
    }

    return CurrStatus;
  }
}
