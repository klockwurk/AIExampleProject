/*******************************************************************************/
/*!
\file   Steering.cs
\author Khan Sweetman
\par    All content © 2015 DigiPen (USA) Corporation, all rights reserved.
\par    Golden Bullet Games
\brief
  File does does things. COOL things.
  
*/
/*******************************************************************************/

//#define MOVEMENT_SWEPT
//#define DEBUG_SWEPT
//#define DEBUG_STEERING
//#define DEBUG_DESIRED_VEL
//#define DEBUG_PATHING
//#define DEBUG_OFF_MESH_LINK
//#define DEBUG_GROUND
//#define DEBUG_INPUT
//#define DEBUG_JUMP
//#define DEBUG_WANDER
//#define DEBUG_OBSTACLE

using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public enum LookEnum
{
  TowardsTarget, // Towards GameObject
  TowardsFace,   // Towards sensable offset of GameObject
  AlongPath,     // Looks towards next waypoint
  DesiredVel,    // Orients towards velocity
  ActualVel,     // Orients towards actual velocity
  Unoriented     // Doesn't try looking anywhere. Useful when doing other things with OrientTowards
}

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(OrientTowardsTarget))]
[RequireComponent(typeof(CapsuleCollider))]
public class Steering : AIMovementController
{
  // ------------------------------------------------- Variables -------------------------------------------------- //
  public const float TRY_OBSTACLE_TIME = 3.5f;
  static readonly Vector3 VEC_ZERO = Vector3.zero;
  static readonly Vector3 VEC_DOWN = Vector3.down;
  static readonly int GROUNDING_MASK = 1048833; // LayerMask.GetMask("Default", "Furniture", "Stairs");
  static readonly int TRAVERSABLE_OBSTACLES_MASK = 256; // LayerMask.GetMask("Furniture");
  
  [HideInInspector]
  public Animator _Animator;
  [HideInInspector]
  public Rigidbody RigBody;
  [HideInInspector]
  public OrientTowardsTarget OrientTowards;
  [HideInInspector]
  public Collider _Collider;
  [HideInInspector]
  public BTAgent _BTAgent;
  [HideInInspector]
  public UnityEngine.AI.NavMeshAgent NavAgent;
  [HideInInspector]
  public CapsuleCollider Collider;

  // Pathing
  private Vector3 DesiredVel;
  private int CornerIndex;
  private bool Pathing = false;

  // Other Parameters
  public LookEnum LookMode
  {
    get { return LookModeData; }
    set
    {
      switch (value)
      {
        case LookEnum.ActualVel:
          OrientTowards.SetTargetType(TargetTypes.Velocity);
          break;
        default:
          OrientTowards.SetTargetType(TargetTypes.Unoriented);
          break;
      }
      LookModeData = value;
    }
  }
  [SerializeField]
  private LookEnum LookModeData = LookEnum.Unoriented;
  public float MaxSpeed = 15.0f;
  public float NearDist = 1.0f; // distance when we declare ourselves at our destination
  // Avoidance parameters
  public float AvoidanceDistance = 6.0f;
  public float AvoidanceSpread = 1.1f;
  public float AvoidanceInstaVel = 1.0f;
  public int NumAvoidanceFeelers = 1;
  // Wander parameters
  public float WanderDistance = 1.0f;
  public float WanderRadius = 1.0f;
  public float WanderJitter = Mathf.PI;
  private float WanderX = Mathf.PI / 2.0f;
  // Weights
  public float AvoidanceWeight
  {
    get { return AvoidanceWeightData; }
    set
    {
      AvoidanceWeightData = value;
      bool contains = ActiveSteeringDelegates.Contains(ObstacleAvoidance);
      if (value != 0.0f && !contains) { ActiveSteeringDelegates.Add(ObstacleAvoidance); }
      else if (value == 0.0f && contains) { ActiveSteeringDelegates.Remove(ObstacleAvoidance); }
    }
  }
  public float PathingWeight
  {
    get { return PathingWeightData; }
    set
    {
      PathingWeightData = value;
      bool contains = ActiveSteeringDelegates.Contains(MoveAlongPath);
      if (value != 0.0f && !contains) { ActiveSteeringDelegates.Add(MoveAlongPath); }
      else if (value == 0.0f && contains) { ActiveSteeringDelegates.Remove(MoveAlongPath); }
    }
  }
  public float SeparationWeight
  {
    get { return SeparationWeightData; }
    set
    {
      SeparationWeightData = value;
      bool contains = ActiveSteeringDelegates.Contains(Separation);
      if (value != 0.0f && !contains) { ActiveSteeringDelegates.Add(Separation); }
      else if (value == 0.0f && contains) { ActiveSteeringDelegates.Remove(Separation); }
    }
  }
  public float BrakingWeight
  {
    get { return BrakingWeightData; }
    set
    {
      BrakingWeightData = value;
      bool contains = ActiveSteeringDelegates.Contains(ObstacleAvoidance);
      if (value != 0.0f || AvoidanceWeight != 0 && !contains) { ActiveSteeringDelegates.Add(Separation); }
      else if (value == 0.0f && AvoidanceWeight == 0 && contains) { ActiveSteeringDelegates.Remove(Separation); BrakingWeightData = 0.0f; }
    }
  }
  public float WanderWeight
  {
    get { return WanderWeightData; }
    set
    {
      WanderWeightData = value;
      bool contains = ActiveSteeringDelegates.Contains(Wander);
      if (value != 0.0f && !contains) { ActiveSteeringDelegates.Add(Wander); }
      else if (value == 0.0f && contains) { ActiveSteeringDelegates.Remove(Wander); }
    }
  }
  public float RunAtWeight
  {
    get { return RunAtWeightData; }
    set
    {
      RunAtWeightData = value;
      bool contains = ActiveSteeringDelegates.Contains(RunAtTarget);
      if (value != 0.0f && !contains) { ActiveSteeringDelegates.Add(RunAtTarget); }
      else if (value == 0.0f && contains) { ActiveSteeringDelegates.Remove(RunAtTarget); }
    }
  }
  public float AvoidanceWeightData = 1.0f;
  public float PathingWeightData = 1.0f;
  public float SeparationWeightData = 1.0f;
  public float BrakingWeightData = 1.0f;
  public float WanderWeightData = 1.0f;
  public float RunAtWeightData = 1f;
  // Braking
  private float BrakeTime = 0.5f;
  private float BrakeTimer = 0.0f;
  private float PassiveBrakingForce = 45.0f; // force applied to stop the character from moving when no input is supplied this frame
  // Input
  private Vector3 InputVectorGivenThisFrame;
  public float AirControl;
  // Grounding
  public bool OnGround { get; private set; }
  private bool PreviouslyOnGround;
  private RaycastHit GroundHitInfo;
  private int GroundingMask;
  // Jumping
  private int TicksSinceLastJumpCheck;
  private bool IsJumping;
  private Vector3 JumpInput;
  private float CantGroundTimer; // time where agent cannot become grounded after jumping
  private Vector3 FakeVelocity;
  private Vector3 TargetLandingPoint;
  // Obstacle traversal
  private Collision CurrObstacle;
  private Vector3 CurrObstacleDes;
  private float ObstacleTimer;
  private int TraversableObstaclesMask;
  // NavMesh cost adjustment
  private int CurrNavMeshIndex;

  IEnumerator SetDestinationCoroutine;

  public delegate Vector3 SteeringDelegate();
  public List<SteeringDelegate> ActiveSteeringDelegates = new List<SteeringDelegate>();

  // ------------------------------------------------- Public Interface -------------------------------------------------- //
  public override void AddMovementInput(Vector3 dir)
  {
    // add force
    InputVectorGivenThisFrame += dir;

    // debug draw desired velocity
#if DEBUG_STEERING
    if (DebugLevel >= 1)
      Debug.DrawLine(transform.position + new Vector3(0.0f, 0.1f, 0.0f), transform.position + dir * 5.0f, Color.black);
#endif
  }

  public override void Jump(float speed)
  {
    Jump(new Vector3(RigBody.velocity.x, speed, RigBody.velocity.z));
  }

  public override void Jump(Vector3 vec)
  {
    JumpInput = vec;
  }

  public override void SetDestination(Vector3 position)
  {
    StopCoroutine(SetDestinationCoroutine);
    SetDestinationCoroutine = SetDestinationOnLanding(position);
    StartCoroutine(SetDestinationCoroutine);
  }

  private IEnumerator SetDestinationOnLanding(Vector3 position)
  {
    while (!OnGround)
    {
      yield return null;
    }

    // Set data
    Destination = position;
    Pathing = true;
    //Set path
    NavMeshPath path = new NavMeshPath();
    bool warpedCorrectly = NavAgent.Warp(transform.position);
    if (warpedCorrectly)
    {
      NavAgent.CalculatePath(Destination, path);
      NavAgent.SetPath(path);
      NavAgent.Stop();
      NavAgent.autoRepath = false;
      NavAgent.updatePosition = false;
      NavAgent.updateRotation = false;
      CornerIndex = 0;
      // Handle looking
      if (LookMode == LookEnum.AlongPath)
      {
        OrientTowards.SetPosTarget(new Vector3(NavAgent.steeringTarget.x, NavAgent.height * 0.5f, NavAgent.steeringTarget.z));
      }

      // Stop braking coroutine
      StopCoroutine(BrakingCoroutine());
    }
    else
    {
      StartCoroutine(SetDestinationOnLanding(position));
    }
  }

  public override void Brake()
  {
    Pathing = false;
    StartCoroutine(BrakingCoroutine());
  }

  public override void AbortPathing()
  {
    Brake();
  }

  public override void CancelBraking()
  {
    StopCoroutine(BrakingCoroutine());
  }

  // TODO:
  // - Some way to respond to getting shoved and such
  public IEnumerator BrakingCoroutine()
  {
    float startSpeed = Vector3.Magnitude(RigBody.velocity);
    BrakeTimer = 0.0f;
    while (BrakeTimer <= BrakeTime)
    {
      RigBody.velocity = (startSpeed + (0.0f - startSpeed) * (BrakeTimer / BrakeTime)) * RigBody.velocity.normalized;
      BrakeTimer += Time.deltaTime;
      yield return null;
    }
    RigBody.velocity = Vector3.zero;
  }

  // ------------------------------------------------- Life Cycle -------------------------------------------------- //
  public void Awake()
  {
    // Grab components
    _Animator = GetComponent<Animator>();
    RigBody = GetComponent<Rigidbody>();
    OrientTowards = GetComponent<OrientTowardsTarget>();
    _Collider = GetComponent<Collider>();
    _BTAgent = GetComponent<BTAgent>();
    NavAgent = GetComponent<NavMeshAgent>();
    NavAgent.autoRepath = false;
    NavAgent.updatePosition = false;
    NavAgent.updateRotation = false;
    Collider = GetComponent<CapsuleCollider>();

    // Set up flocking functions
    if (AvoidanceWeight != 0)
    {
      ActiveSteeringDelegates.Add(ObstacleAvoidance);
    }
    if (PathingWeight != 0)
    {
      ActiveSteeringDelegates.Add(MoveAlongPath);
    }
    if (RunAtWeight != 0)
    {
      ActiveSteeringDelegates.Add(RunAtTarget);
    }
    if (SeparationWeight != 0)
    {
      ActiveSteeringDelegates.Add(Separation);
    }
    if (WanderWeight != 0)
    {
      ActiveSteeringDelegates.Add(Wander);
    }

    // Layer mask shit
    GroundingMask = GROUNDING_MASK;
    TraversableObstaclesMask = TRAVERSABLE_OBSTACLES_MASK;

    // Some good-ass coroutines
    SetDestinationCoroutine = SetDestinationOnLanding(VEC_ZERO);
  }

  public void OnDestroy()
  {
    Neighbors.Clear();
  }

  public void OnCollisionEnter(Collision col)
  {
    if (CurrObstacle == null)
    {
      // Check if we hit a walkable object
      int layer = 1 << col.gameObject.layer;
      layer = layer & TraversableObstaclesMask;
      if (layer != 0)
      {
        // Check if it's a thing we should walk over
        // (in front, collision over foot height)
        Vector3 towards = col.transform.position - transform.position;
        float dot = Vector3.Dot(transform.forward, towards);
        float minHeight = transform.position.y + Collider.radius;
        if (dot > 0.0f && col.contacts[0].point.y > minHeight)
        {
          // Set as current obstacle
          CurrObstacle = col;
          Vector3 point = CurrObstacle.contacts[0].point;
          CurrObstacleDes = new Vector3(point.x, CurrObstacle.collider.bounds.max.y, point.z);
          ObstacleTimer = 0.0f;

          // switch to kinematic so WE CAN CLIP THROUGH THE FUCKING OBSTACLE
          RigBody.isKinematic = true;

#if DEBUG_OBSTACLE
          if (CurrObstacle != null)
          {
            Util.DrawCircle(CurrObstacleDes, Color.green, 0.5f, 2.0f);
          }
          else
          {
            Vector3 max = new Vector3(col.contacts[0].point.x,
              CurrObstacle.collider.bounds.max.y, col.contacts[0].point.z);
            Util.DrawCircle(max, Color.red, 0.5f, 2.0f);
          }
#endif
        }
      }
#if DEBUG_OBSTACLE
      Util.DrawSphere(col.contacts[0].point, Color.white, 0.5f, 2.0f);
#endif
    }
  }

  // ------------------------------------------------- Movement Updates -------------------------------------------------- //
  public void FixedUpdate()
  {
#if DEBUG_OFF_MESH_LINK
    OffMeshLinkUpdate();
#endif
    GroundingUpdate();
    JumpUpdate();
    if (CurrObstacle != null)
    {
      ObstacleTraversalUpdate();
    }
    else
    {
      if (OnGround)
      {
        if (Pathing)
        {
          PathingUpdate();
        }
        if (InputVectorGivenThisFrame != VEC_ZERO) { GroundInputMovementUpdate(); }
        //else { PassiveBrakingUpdate(); } // InputVec is sometimes zero for reasons I can't discern :(
        UpdateNeighbors();
        CapVelocity();
      }
      else
      {
        if (!IsJumping && PreviouslyOnGround)
        {
          StickToGroundUpdate();
        }
        AirInputMovementUpdate();
      }
    }
    IsJumping = false;
  }

  private void JumpUpdate()
  {
    if (OnGround && JumpInput != VEC_ZERO)
    {
      RigBody.velocity = JumpInput;
      IsJumping = true;
      OnGround = false;
      CantGroundTimer = 0.2f;
    }
    CantGroundTimer -= Time.deltaTime;
    JumpInput = VEC_ZERO;
  }

  private void OffMeshLinkUpdate()
  {
#if DEBUG_OFF_MESH_LINK
    Color color = Color.yellow;
    if (NavAgent.isOnOffMeshLink)
    {
      color = Color.yellow;
      Debug.DrawLine(transform.position, NavAgent.currentOffMeshLinkData.startPos, color, Time.fixedDeltaTime);
      Debug.DrawLine(NavAgent.currentOffMeshLinkData.startPos, NavAgent.currentOffMeshLinkData.endPos, color, Time.fixedDeltaTime);
      Util.DrawCircle(NavAgent.currentOffMeshLinkData.startPos, color, 0.5f, Time.fixedDeltaTime);
      Util.DrawCircle(NavAgent.currentOffMeshLinkData.endPos, color, 0.5f, Time.fixedDeltaTime);
    }
    else
    {
      color = Color.blue;
      if (NavAgent.currentOffMeshLinkData.valid)
      {
        Util.DrawCircle(NavAgent.currentOffMeshLinkData.startPos, color, 0.25f, Time.fixedDeltaTime);
        Util.DrawCircle(NavAgent.currentOffMeshLinkData.endPos + Vector3.up * 0.25f, color, 0.25f, Time.fixedDeltaTime);
        Debug.DrawLine(NavAgent.currentOffMeshLinkData.startPos, NavAgent.currentOffMeshLinkData.endPos);
      }
      if (NavAgent.nextOffMeshLinkData.valid)
      {
        Util.DrawSphere(NavAgent.nextOffMeshLinkData.startPos, color, 0.25f, Time.fixedDeltaTime);
      }
    }
#endif
  }

  private void PathingUpdate()
  {
    // Pre-emptive final pathing check
    if (CheckFinalTarget())
    {
      return;
    }

    // Movement vectors
    Vector3 avoidance = Vector3.zero;
    if (AvoidanceWeight != 0.0f || AvoidanceInstaVel != 0.0f)
    {
      avoidance = ObstacleAvoidance();
    }
    DesiredVel = avoidance;
    for (int i = 0; i < ActiveSteeringDelegates.Count; ++i)
    {
      DesiredVel += ActiveSteeringDelegates[i]();
    }
    DesiredVel.y = 0.0f;
    AddMovementInput(DesiredVel);
    RigBody.AddForce(avoidance * AvoidanceInstaVel * Time.fixedDeltaTime, ForceMode.Impulse);
    RigBody.AddForce(BrakingForce * BrakingWeight * Time.fixedDeltaTime, ForceMode.Acceleration);

    //Orientation
    if (LookMode == LookEnum.DesiredVel)
    {
      OrientTowards.SetPosTarget(transform.position + DesiredVel);
    }

    // Start moving towards next pathing point?
    // don't do shit if we don't have a valid path
    if (NavAgent.hasPath && CornerIndex < NavAgent.path.corners.Length)
    {
      bool readyToUpdate = Vector3.SqrMagnitude(transform.position -
        NavAgent.path.corners[CornerIndex]) < 1.5f * 1.5f;

      if (readyToUpdate)
      {
        // edge-case: entering an off-mesh link
        if (NavAgent.nextOffMeshLinkData.valid && Util.DistSqr(NavAgent.nextOffMeshLinkData.startPos,
          NavAgent.path.corners[CornerIndex]) < 0.1f)
        {
          EnterOffMeshLink();
        }
        else
        {
          //Check for ending
          ++CornerIndex;
          if (CornerIndex >= NavAgent.path.corners.Length)
          {
            FinishPathing();
            // Update next steering target
            if (LookMode == LookEnum.AlongPath)
            {
              OrientTowards.SetPosTarget(NavAgent.destination);
            }
          }
          else
          {
            // Update next steering target
            if (LookMode == LookEnum.AlongPath)
            {
              OrientTowards.SetPosTarget(NavAgent.path.corners[CornerIndex]);
            }
          }
        }
      }
    }

#if DEBUG_DESIRED_VEL
    Debug.DrawRay(transform.position + Vector3.up * 0.5f, DesiredVel * 2f, Color.white);
#endif
  }

  private void GroundingUpdate()
  {
    PreviouslyOnGround = OnGround;
    if (CantGroundTimer < 0.0f)
    {
      float dist = Collider.height * 0.5f + 0.2f;
      Vector3 position = Collider.bounds.center;
      OnGround = Physics.SphereCast(position, Collider.radius * 0.5f, VEC_DOWN, out GroundHitInfo,
        dist, GroundingMask);

#if DEBUG_GROUND
    Debug.DrawRay(position, Vector3.down * dist, OnGround ? Color.red : Color.green, Time.fixedDeltaTime, false);
#endif
    }
  }

  private void StickToGroundUpdate()
  {
    Vector3 projected = Vector3.ProjectOnPlane(RigBody.velocity, GroundHitInfo.normal);
    RigBody.velocity = new Vector3(projected.x, -GroundHitInfo.normal.y, projected.z);
#if DEBUG_GROUND
    Debug.DrawLine(transform.position, transform.position + RigBody.velocity, Color.blue, 0.5f);
#endif
  }

  private void PassiveBrakingUpdate()
  {
    if (RigBody.velocity.sqrMagnitude < 0.5f)
    {
      RigBody.velocity = VEC_ZERO;
    }
    else
    {
      RigBody.AddForce(new Vector3(-RigBody.velocity.x, 0.0f, -RigBody.velocity.z) * PassiveBrakingForce, ForceMode.Acceleration);
    }
  }

  private void GroundInputMovementUpdate()
  {
#if MOVEMENT_SWEPT
    // Adjust input to move along ground
    Vector3 groundNormal = GroundHitInfo.normal;
    Vector3 moveDir = InputVectorGivenThisFrame.normalized * Time.fixedDeltaTime;
    Vector3.OrthoNormalize(ref groundNormal, ref moveDir);

    // Sweep along input
    RaycastHit hitInfo;
    Vector3 point1 = transform.position;
    point1.y += Collider.radius;
    Vector3 point2 = transform.position;
    point2.y += Collider.height - Collider.radius;

    // Adjust position based on sweep
    if (Physics.CapsuleCast(point1, point2, Collider.radius, moveDir, out hitInfo,
      MoveSpeed, WalkableLayersMask, QueryTriggerInteraction.Ignore))
    {
      transform.position = hitInfo.point;
    }
    else
    {
      transform.position = transform.position + moveDir;
    }
#if DEBUG_SWEPT
    Debug.DrawLine(point1, point2);
#endif

#else
    // Calculate direction to move in
    Vector3 groundNormal = GroundHitInfo.normal;
    Vector3 moveDir = InputVectorGivenThisFrame;
    Vector3.OrthoNormalize(ref groundNormal, ref moveDir);
    RigBody.AddForce(moveDir * MoveSpeed, ForceMode.Acceleration);
    
#endif
    // Reset input
    InputVectorGivenThisFrame = Vector3.zero;

#if DEBUG_GROUND
    Debug.DrawRay(GroundHitInfo.point, GroundHitInfo.normal, Color.white, Time.fixedDeltaTime);
#endif
#if DEBUG_INPUT
    Debug.DrawRay(transform.position, moveDir, Color.white, Time.fixedDeltaTime);
#endif
  }

  private void AirInputMovementUpdate()
  {
    RigBody.AddForce(InputVectorGivenThisFrame.normalized * MoveSpeed * AirControl, ForceMode.Acceleration);
    InputVectorGivenThisFrame = Vector3.zero;

    // Handle looking
    if (LookMode == LookEnum.DesiredVel || LookMode == LookEnum.AlongPath)
    {
      OrientTowards.SetPosTarget(TargetLandingPoint);
    }
  }

  private void ObstacleTraversalUpdate()
  {
    // give up after trying to run up it for a while
    ObstacleTimer += Time.fixedDeltaTime;
    if (ObstacleTimer > TRY_OBSTACLE_TIME)
    {
      CurrObstacle = null;
    }
    else
    {
      // move towards top of obstacle extent
      Vector3 dir = (CurrObstacleDes - transform.position).normalized;
      if (transform.position.y < CurrObstacleDes.y)
      {
        transform.position += dir * MaxSpeed * 0.6f * Time.fixedDeltaTime; // 0.6 is magic number
      }
      // reached top of extent?
      // we out
      else
      {
        transform.position = CurrObstacleDes;
        CurrObstacle = null;
        RigBody.isKinematic = false;
      }

      // Handle looking
      if (LookMode == LookEnum.DesiredVel || LookMode == LookEnum.AlongPath)
      {
        OrientTowards.SetPosTarget(CurrObstacleDes);
        Debug.DrawLine(transform.position, CurrObstacleDes, Color.white);
      }
      
#if DEBUG_OBSTACLE
      Debug.DrawLine(transform.position, transform.position + dir, Color.green);
      Util.DrawCircle(CurrObstacleDes, Color.green, 0.5f);
#endif
    }
  }

  // ------------------------------------------------- Steering -------------------------------------------------- //
  public Vector3 Wander()
  {
    WanderX += Random.Range(-WanderJitter, WanderJitter) * Time.deltaTime;
    Vector3 target = new Vector3(Mathf.Cos(WanderX) * WanderRadius, 1.0f, Mathf.Sin(WanderX) * WanderRadius);
    target += transform.forward * WanderDistance;

#if DEBUG_WANDER
    Vector3 pos = transform.position;
    Debug.DrawLine(pos, pos + target - new Vector3(0.0f, 1.0f, 0.0f));
    Util.DrawCircle(pos + transform.forward * WanderDistance, WanderRadius);
#endif

    return target.normalized * WanderWeight;
  }

  // Returns steering force
  private Vector3 BrakingForce = Vector3.zero;
  public Vector3 ObstacleAvoidance()
  {
    Vector3 steeringForce = Vector3.zero;
    BrakingForce = Vector3.zero;

    for (int i = 0; i < NumAvoidanceFeelers; ++i)
    {
      // Raycast in a random direction in front of us
      Vector3 direction = Vector3.RotateTowards(transform.forward, transform.right,
        Random.Range(-AvoidanceSpread * Mathf.PI / 360.0f, AvoidanceSpread * Mathf.PI / 360.0f), 0.0f);
      RaycastHit hitInfo = new RaycastHit();

      // Check direction of collision, if one occured
      if (Physics.Raycast(transform.position, direction, out hitInfo, AvoidanceDistance))
      {
        if (transform.InverseTransformPoint(hitInfo.point).x > 0.0f)
          steeringForce = -transform.right;
        else
          steeringForce = transform.right;

        BrakingForce = -transform.forward * (1.0f / Vector3.SqrMagnitude(hitInfo.point - transform.position));
        BrakingForce.Normalize();
      }

#if DEBUG_AVOIDANCE
      Debug.DrawLine(transform.position, transform.position + direction * AvoidanceDistance, Color.green);
      Debug.DrawLine(transform.position, transform.position + steeringForce, Color.green);
#endif
    }

#if DEBUG_STEERING
    // Debug Draw steering force
    if (DebugLevel >= 1)
      Debug.DrawLine(transform.position, transform.position + steeringForce * AvoidanceDistance, Color.cyan);
#endif
    return steeringForce.normalized * AvoidanceWeight;
  }

  public Vector3 MoveAlongPath()
  {
    // Skips logic if we don't have a valid path
    if (Pathing && NavAgent.hasPath)
    {
      //Get vector towards steering target
      Vector3 towards;
      Vector3[] corners = NavAgent.path.corners;
      if (CornerIndex >= corners.Length)
      {
#if DEBUG_PATHING
        Debug.Log("CornderIndex(" + CornerIndex + ") out of range(" + NavAgent.path.corners.Length + "):" + gameObject);
        Util.DrawFullPath(NavAgent.path, Color.red, 5.0f);
#endif
        towards = NavAgent.destination - transform.position;
      }
      else
      {
        towards = corners[CornerIndex] - transform.position;
#if DEBUG_PATHING
        Color color = NavAgent.path.status == NavMeshPathStatus.PathComplete ? Color.green : Color.red;
        //Debug.DrawLine(transform.position, transform.position + towards, color, Time.fixedDeltaTime);
        //Debug.DrawLine(transform.position + Vector3.up, NavAgent.path.corners[CornerIndex], color, Time.fixedDeltaTime);
        Util.DrawFullPath(NavAgent.path, color, Time.fixedDeltaTime);
#endif
      }

      return towards.normalized * PathingWeight;
    }
    else
    {
      return (Destination - transform.position).normalized * PathingWeight;
    }
  }

  public Vector3 RunAtTarget()
  {
    return (Destination - transform.position).normalized * RunAtWeight;
  }

  public Vector3 Separation()
  {
    Vector3 sep = Vector3.zero;
    for (int i = 0; i < Neighbors.Count; ++i)
    {
      if (Neighbors[i] == null)
        continue;

      sep += (transform.position - Neighbors[i].transform.position);
    }

    return sep.normalized * SeparationWeight;
  }

  // ------------------------------------------------- Helpers -------------------------------------------------- //
  private List<GameObject> Neighbors = new List<GameObject>();
  private int FramesSinceLastNeighborUpdate = 0;
  public float NeighborhoodRange = 5.0f;
  private void UpdateNeighbors()
  {
    // Only update neighbors every once in a while
    if (--FramesSinceLastNeighborUpdate <= 0)
    {
      FramesSinceLastNeighborUpdate = Random.Range(180, 600);
      // Get updated list of neighbors
      Neighbors.Clear();
      int count = HordeManager.Zombies.Count;
      foreach(GBZombieLogicHelper zombie in HordeManager.Zombies)
      {
        if (Util.DistSqr(zombie.transform.position, transform.position) < NeighborhoodRange * NeighborhoodRange)
        {
          Neighbors.Add(zombie.gameObject);
        }
      }
    }
  }

  private bool CheckFinalTarget()
  {
    // We occasionally check if we're next to the final target, which
    // is often the player.
    if (Vector3.SqrMagnitude(Destination - transform.position) < NearDist)
    {
      FinishPathing();
      return true;
    }
    return false;
  }

  private void FinishPathing()
  {
    Pathing = false;
    OnArrival(Destination);
    Brake();
  }

  private void CapVelocity()
  {
    // cap velocity
    Vector2 groundVel = new Vector2(RigBody.velocity.x, RigBody.velocity.z);
    if (Vector2.SqrMagnitude(groundVel) > MaxSpeed * MaxSpeed)
    {
      groundVel = groundVel.normalized * MaxSpeed;
      RigBody.velocity = new Vector3(groundVel.x, RigBody.velocity.y, groundVel.y);
    }
  }

  private void EnterOffMeshLink()
  {
    if (OnGround && NavAgent.isOnNavMesh)
    {
#if DEBUG_OFF_MESH_LINK
      Debug.Log("Entering off mesh link");
#endif
      // try to update OffMeshLinkData
      OffMeshLinkData data = NavAgent.currentOffMeshLinkData;
      if (!NavAgent.currentOffMeshLinkData.valid)
      {
        if (NavAgent.nextOffMeshLinkData.valid)
        {
          data = NavAgent.nextOffMeshLinkData;
        }
        else
        {
          return;
        }
      }

      float peakHeight = Mathf.Max(data.endPos.y, data.startPos.y) + NavAgent.height * Random.Range(1.5f, 2.5f);
      TargetLandingPoint = data.endPos;
#if !DEBUG_OFF_MESH_LINK
      Jump(Util.CalculateJumpVel(transform.position, TargetLandingPoint, peakHeight, 0, false));
#else
      Jump(Util.CalculateJumpVel(transform.position, endPos, peakHeight, 0, true));
      Util.DrawColumn(endPos, Color.yellow, 1.0f, 3, 3.0f);
#endif
    }
  }
}
