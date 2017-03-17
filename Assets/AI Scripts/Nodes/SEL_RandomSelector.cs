/*******************************************************************************/
/*!
\file   BT_RandomSelector.cs
\author Khan Sweetman
\par    All content © 2015 DigiPen (USA) Corporation, all rights reserved.
\par    Golden Bullet Games
\brief
  Weighted random selection between left and right nodes.
  TODO:
    -Accommodate any number of children
      -Serialize arrays
      -Zero style "weighted array of shit that has a nice interface to pick 
       stuff randomly from"
  
*/
/*******************************************************************************/

using UnityEngine;
using System.Collections;

public class SEL_RandomSelector : BTNode
{
  // ------------------------------------------------- Variables -------------------------------------------------- //
  public float LeftWeight = 0.5f;
  public float RightWeight = 0.5f;

  // ------------------------------------------------- Life Cycle -------------------------------------------------- //
  public override void EnterBehavior()
  {
    // Pick random child
    float totalWeight = LeftWeight + RightWeight;
    float picked = Random.Range(0, totalWeight);
    if (picked < LeftWeight)
    {
      CurrIndex = 0;
    }
    else
    {
      CurrIndex = 1;
    }

    // Initialization
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
    // Complete when randomly selected child has finished
    CurrStatus = Children[CurrIndex].Update();
    return CurrStatus;
  }
}
