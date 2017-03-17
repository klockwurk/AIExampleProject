/*******************************************************************************/
/*!
\file   LEF_Shoot.cs
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

public class LEF_Shoot : BTNode
{
  // ------------------------------------------------- Variables -------------------------------------------------- //
  private AIShoot ShootLogic;
  public string BBTargetKey = "Target";
  public string BBProjectileKey = "SpitPrefab";
  public float Spread = 60.0f;
  public int NumShots = 1;
  public float DestroyAfterTime = 5.0f; // set to a negative number to prevent destruction after time
  public bool UseArc = false;
  public float PeakHeightOffset = 5.0f;
  public float PeakRandom = 3.0f;

  // ------------------------------------------------- Life Cycle -------------------------------------------------- //
  public override void Initialize(object[] objs)
  {
    base.Initialize(objs);
    ShootLogic = Owner.GetComponent<AIShoot>();
  }

  public override void EnterBehavior()
  {
    if (BBProjectileKey != "")
    {
      ShootLogic.ProjectilePrefab = (GameObject)Blackboard[BBProjectileKey];
    }
    Shoot();
    SetStatus(BT_Status.Success);
  }

  private void Shoot()
  {
    // get targets
    Transform target = (Transform)Blackboard[BBTargetKey];

    // shoot
    if (UseArc)
    {
      float peakHeight = ShootLogic.Muzzle.position.y + PeakHeightOffset;
      ShootLogic.CmdArcShoot(target.position, NumShots, DestroyAfterTime, Spread, peakHeight, PeakRandom);
    }
    else
    {
      ShootLogic.CmdShoot(target.position, NumShots, DestroyAfterTime, Spread);
    }
  }
}
