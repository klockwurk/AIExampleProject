/*******************************************************************************/
/*!
\file   LEF_MoveToNavMesh.cs
\author Khan Sweetman
\par    All content © 2016-2017 DigiPen (USA) Corporation, all rights reserved.
\par    The Bakery
 
\brief
  ...
 
*/
/*******************************************************************************/

#define DEBUG_RETURN

using UnityEngine;
using System.Collections;

public class LEF_MoveToNavMesh : BTNode
{
  // ------------------------------------------------- Variables -------------------------------------------------- //
  private const float RUN_TIME = 2.0f;
  private const float RUN_RANDOM = 0.5f;

  private UnityEngine.AI.NavMeshAgent NavAgent;
  private Vector3 ClosestPointOnMesh;
  private Steering SteeringComponent;

  private float RunTimer;

  // ------------------------------------------------- Life Cycle -------------------------------------------------- //
  public override void Initialize(object[] objs)
  {
    base.Initialize(objs);
    NavAgent = Owner.GetComponent<UnityEngine.AI.NavMeshAgent>();
    SteeringComponent = Owner.GetComponent<Steering>();
  }

  public override void EnterBehavior()
  {
    base.EnterBehavior();
    UpdateClosestPoint();
    RunTimer = Random.Range(RUN_TIME - RUN_RANDOM, RUN_TIME + RUN_RANDOM);
  }

  public override BT_Status Update()
  {
    if (Util.DistSqr(Owner.transform.position, ClosestPointOnMesh) < NavAgent.height / 2.0f + 0.2f)
    {
      return SetStatus(BT_Status.Success);
    }

    // Try to steer to the closest point
    SteeringComponent.AddMovementInput(ClosestPointOnMesh - Owner.transform.position);
    RunTimer -= Time.fixedDeltaTime;

    // Jump towards point every once in a while
    if (SteeringComponent.OnGround && RunTimer < 0.0f)
    {
      RunTimer = Random.Range(RUN_TIME - RUN_RANDOM, RUN_TIME + RUN_RANDOM);
      UpdateClosestPoint();
      float peak = Mathf.Max(Owner.transform.position.y, ClosestPointOnMesh.y) + NavAgent.height * Random.Range(0.5f, 3.0f);
#if DEBUG_RETURN
      Debug.DrawLine(Owner.transform.position, ClosestPointOnMesh, Color.yellow);
      Util.DrawCircle(ClosestPointOnMesh, Color.yellow, 0.75f, Time.fixedDeltaTime);
      SteeringComponent.Jump(Util.CalculateJumpVel(Owner.transform.position, ClosestPointOnMesh, peak, 0.0f, true));
#else
    SteeringComponent.Jump(Util.CalculateJumpVel(Owner.transform.position, ClosestPointOnMesh, peak));
#endif
    }

    return CurrStatus;
  }

  // ------------------------------------------------- Helpers -------------------------------------------------- //
  private void UpdateClosestPoint()
  {
    UnityEngine.AI.NavMeshHit hit;
    bool foundSpot = UnityEngine.AI.NavMesh.SamplePosition(Owner.transform.position, out hit, 8.0f, UnityEngine.AI.NavMesh.AllAreas);
    if (foundSpot)
    {
      ClosestPointOnMesh = hit.position;
    }
#if DEBUG_RETURN
    else
    {
      Debug.Log("Could not find a point to return to");
    }
#endif
  }
}
