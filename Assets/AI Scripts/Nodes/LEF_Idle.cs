/*******************************************************************************/
/*!
\file   BT_Idle.cs
\author Khan Sweetman
\par    All content © 2015 DigiPen (USA) Corporation, all rights reserved.
\par    Golden Bullet Games
\brief
  File does does things. COOL things.
  
*/
/*******************************************************************************/  

using UnityEngine;
using System.Collections;

[System.Serializable]
public class LEF_Idle : BTNode
{
  float IdleTimer;
  public float IdleTime = 3.0f;
  public float IdleTimeRandom = 1.0f;

  // First parameter should be owner
  // Second parameter should be root
  public override void Initialize(object[] objs)
  {
    base.Initialize(objs);
  }

  /////////////////////////////////// Public Interface ///////////////////////////////////
  public override void EnterBehavior()
  {
    // Entrance behavior
    base.EnterBehavior();
    IdleTimer = Random.Range(-IdleTimeRandom, IdleTimeRandom) + IdleTime;
  }

  /////////////////////////////////// Per frame Functions ///////////////////////////////////
  public override BT_Status Update()
  {
    // Count down
    IdleTimer -= Time.deltaTime;
    if (IdleTimer <= 0.0f)
      return SetStatus(BT_Status.Success);

    return CurrStatus;
  }
}
