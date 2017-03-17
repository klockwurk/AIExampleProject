/*******************************************************************************/
/*!
\file   DEC_RepeatForTime.cs
\author Khan Sweetman
\par    All content © 2015 DigiPen (USA) Corporation, all rights reserved.
\par    Golden Bullet Games
\brief
  - Runs child until timer runs out, then prematurely exits
  
*/
/*******************************************************************************/

using UnityEngine;
using System.Collections;

[System.Serializable]
public class DEC_RepeatForTime : BTNode
{
  public float RepeatTime = 3.0f;
  public float TimeRandom = 1.0f;

  [System.NonSerialized] public float Timer = 0.0f;

  /////////////////////////////////////// Public Interface ///////////////////////////////////////
  public override void EnterBehavior()
  {
    Timer = RepeatTime + Random.Range(-TimeRandom, TimeRandom);
    Children[0].SetStatus(BT_Status.Entering);
    SetStatus(BT_Status.Running);
  }

  public override void ExitBehavior()
  {
    // Call ExitBehavior on active children
    SetStatus(BT_Status.Fail);
    foreach(var child in Children)
    {
      if (child.CurrStatus == BT_Status.Running)
        child.ExitBehavior();
    }
  }
  
  /////////////////////////////////////// Per frame Functions ///////////////////////////////////////
  override public BT_Status Update ()
  {
    // Update while time is left
    Timer -= Time.deltaTime;
    if(Timer <= 0.0f)
    {
      Children[0].ExitBehavior();
      return SetStatus(BT_Status.Success);
    }

    BT_Status status = Children[0].Update();
    if (status == BT_Status.Success)
      Children[0].SetStatus(BT_Status.Entering);
    else if (status == BT_Status.Fail)
      SetStatus(BT_Status.Fail);

    return CurrStatus;
  }
}
