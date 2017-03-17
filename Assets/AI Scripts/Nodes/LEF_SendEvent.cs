/*******************************************************************************/
/*!
\file   LEF_SendEvent.cs
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

public class LEF_SendEvent : BTNode
{
  // ------------------------------------------------- Variables -------------------------------------------------- //
  public string EventName;
  public string TargetName; // "" defaults to Owner

  private GameObject Target;

  // ------------------------------------------------- Life Cycle -------------------------------------------------- //
  public override void Initialize(object[] objs)
  {
    base.Initialize(objs);
    Target = Owner;
    if (!System.String.IsNullOrEmpty(TargetName))
    {
      Target = GameObject.Find(TargetName);
    }
  }

  public override void EnterBehavior()
  {
    // Send event
    Target.EventSend(EventName);
    Debug.Log("Sending event: " + EventName);

    // Update status
    SetStatus(BT_Status.Success);
  }

  public override BT_Status Update()
  {
    SetStatus(BT_Status.Success);
    return CurrStatus;
  }
}

