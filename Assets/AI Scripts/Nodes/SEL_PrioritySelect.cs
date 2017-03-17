/*******************************************************************************/
/*!
\file   SEL_PrioritySelect.cs
\author Khan Sweetman
\par    All content © 2015 DigiPen (USA) Corporation, all rights reserved.
\par    Golden Bullet Games
\brief
  - Selects highest Utility child
  - Completes when selected child completes
  
*/
/*******************************************************************************/

using UnityEngine;
using System.Collections;

public class SEL_PrioritySelect : BTNode
{
  // ------------------------------------------------- Life Cycle -------------------------------------------------- //
  public override void EnterBehavior()
  {
    // Initializiton
    SelectHighestPriorityChild();
    SetStatus(BT_Status.Running);
    Children[CurrIndex].SetStatus(BT_Status.Entering);
  }

  public override void ExitBehavior()
  {
    // Call ExitBehavior on active children
    Children[CurrIndex].ExitBehavior();
    SetStatus(BT_Status.Fail);
  }

  override public BT_Status Update ()
  {
    SetStatus(Children[CurrIndex].Update());
    return CurrStatus;
  }

  // ------------------------------------------------- Helper Functions -------------------------------------------------- //
  void SelectHighestPriorityChild()
  {
    // Choose highest Utility child
    CurrIndex = 0;
    float highestUtility = 0.0f;
    for (int i = 0; i < Children.Count; ++i)
    {
      float utility = Children[i].Utility();
      if (utility > highestUtility)
      {
        CurrIndex = i;
        highestUtility = utility;
      }
    }
  }
}
