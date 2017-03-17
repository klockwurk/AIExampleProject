/*******************************************************************************/
/*!
\file   DEC_DistanceCheck.cs
\author Khan Sweetman
\par    All content © 2016-2017 DigiPen (USA) Corporation, all rights reserved.
\par    The Bakery

\brief
  ...
 
*/
/*******************************************************************************/
//#define DEBUG_CHECK

using UnityEngine;
using System.Collections;

public class DEC_DistanceCheck : BTNode
{
  // ------------------------------------------------- Variables -------------------------------------------------- //
  public enum CheckTypes
  {
    SucceedWhenCloserThanDistance,
    SucceedWhenFartherThanDistance
  }

  public string TargetBlackboardEntry;
  public float Distance = 5f;
  public CheckTypes CheckType;
  public bool Check3D;
  public bool InvalidateIfTooMuchHeightDifference;
  public bool FailOnHeightDifference;

  // ------------------------------------------------- Life Cycle -------------------------------------------------- //
  public override void Initialize(object[] objs)
  {
    base.Initialize(objs);
  }

  public override void EnterBehavior()
  {
    base.EnterBehavior();
    SetStatus(BT_Status.Running);
  }

  public override BT_Status Update()
  {
    // Distance calculation
    float sqrMagnitude = 0.0f;
    Vector3 targetPos = BTree.Blackboard.GetEntryAsVector3(TargetBlackboardEntry);

#if DEBUG_CHECK
    Vector3 dir = (targetPos - Owner.transform.position);
    Debug.DrawRay(Owner.transform.position, dir, Color.white, Time.fixedDeltaTime);
    Debug.DrawRay(Owner.transform.position + Vector3.up * 0.3f, dir.normalized * Distance, Color.black, Time.fixedDeltaTime);
#endif

    // invalidate due to height difference?
    if (InvalidateIfTooMuchHeightDifference)
    {
      if (Mathf.Abs(targetPos.y - Owner.transform.position.y) > 5.0f)
      {
        if (FailOnHeightDifference)
        {
          return SetStatus(BT_Status.Fail);
        }
        return Children[0].Update();
      }
    }
    // actual calculation
    if (Check3D)
    {
      sqrMagnitude = Util.DistSqr(Owner.transform.position, targetPos);
    }
    else
    {
      float px = Owner.transform.position.x;
      float pz = Owner.transform.position.z;
      float tx = targetPos.x;
      float tz = targetPos.z;
      sqrMagnitude = (px - tx) * (px - tx) + (pz - tz) * (pz - tz);
    }

    if (sqrMagnitude > Distance * Distance)
    {
      if (CheckType == CheckTypes.SucceedWhenCloserThanDistance)
      {
        return SetStatus(BT_Status.Fail);
      }
    }
    else
    {
      if (CheckType == CheckTypes.SucceedWhenFartherThanDistance)
      {
        return SetStatus(BT_Status.Fail);
      }
    }

    return Children[0].Update();
  }
}
