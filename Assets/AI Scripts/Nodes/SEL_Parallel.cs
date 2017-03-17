/*******************************************************************************/
/*!
\file   BT_Parallel.cs
\author Khan Sweetman
\par    All content © 2015 DigiPen (USA) Corporation, all rights reserved.
\par    Golden Bullet Games
\brief
  File does does things. COOL things.
  
*/
/*******************************************************************************/

using UnityEngine;
using System.Collections;

public class SEL_Parallel : BTNode
{
  public enum FinishMethods
  {
    FirstChildToFinish,
    WaitForAllChildren
  }

  // ------------------------------------------------- Variables -------------------------------------------------- //
  public FinishMethods FinishMethod;
  private int NumChildrenFinished;

  /////////////////////////////////////// Public Interface ///////////////////////////////////////
  public override void EnterBehavior()
  {
    // Initialization
    SetStatus(BT_Status.Running);
    NumChildrenFinished = 0;
    foreach (BTNode node in Children)
      node.SetStatus(BT_Status.Entering);
  }

  public override void ExitBehavior()
  {
    // Call ExitBehavior on active children
    foreach(var child in Children)
      if (child.CurrStatus == BT_Status.Running)
        child.ExitBehavior();
    SetStatus(BT_Status.Fail);
  }

  /////////////////////////////////////// Per frame Functions ///////////////////////////////////////
  override public BT_Status Update ()
  {
    // Update children
    // Success when all children have succeeded
    // Fail when first child fails
    bool complete = true;
    foreach(BTNode child in Children)
    {
      if (child.CurrStatus == BT_Status.Fail || child.CurrStatus == BT_Status.Success)
      {
        if (FinishMethod == FinishMethods.FirstChildToFinish)
          return SetStatus(child.CurrStatus);
        else
        {
          ++NumChildrenFinished;
          if (NumChildrenFinished == Children.Count)
            SetStatus(child.CurrStatus);
        }
      }
      // Continue to update children that are still running
      else
      {
        BT_Status status = child.Update();
        if (status == BT_Status.Running)
          complete = false;
        // Fail if a child fails
        else if (status == BT_Status.Fail)
          return SetStatus(BT_Status.Fail);
      }
    }

    if (complete)
      return SetStatus(BT_Status.Success);

    return CurrStatus;
  }
}
