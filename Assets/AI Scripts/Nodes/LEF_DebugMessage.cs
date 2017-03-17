/*******************************************************************************/
/*!
\file   LEF_DebugMessage.cs
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

public class LEF_DebugMessage : BTNode
{
  // ------------------------------------------------- Variables -------------------------------------------------- //
  public string Message;
  public string BBKeyToPrint;

  // ------------------------------------------------- Life Cycle -------------------------------------------------- //
  public override void EnterBehavior()
  {
    string str = Message;
    if (BBKeyToPrint != "")
    {
      str = "BBKey (" + BBKeyToPrint + "): " + Blackboard[BBKeyToPrint];
    }
    Debug.Log(str);
    SetStatus(BT_Status.Success);
  }

  public override BT_Status Update()
  {
    SetStatus(BT_Status.Success);
    return CurrStatus;
  }
}
