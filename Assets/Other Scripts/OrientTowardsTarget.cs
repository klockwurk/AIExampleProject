/*******************************************************************************/
/*!
\file   OrientTowardsTarget.cs
\author Khan Sweetman
\par    All content © 2015 DigiPen (USA) Corporation, all rights reserved.
\par    Golden Bullet Games
\brief
  File does does things. COOL things.
  
*/
/*******************************************************************************/  

using UnityEngine;
using System.Collections;

public enum TargetTypes
{
  Position,
  GameObject,
  Velocity,
  Unoriented // Basically off
}

public class OrientTowardsTarget : MonoBehaviour
{
  private Rigidbody Body;
  [SerializeField] private TargetTypes TargetType = TargetTypes.Position;
  [SerializeField] private GameObject ObjTarget;
  [SerializeField] private Vector3 PosTarget;
  
  [SerializeField] public float TurnSpeed = 90.0f;
  [SerializeField] public float AngleMaximum = 0.0f;
  [SerializeField] public Vector3 Offset = Vector3.zero;
  [SerializeField] public bool XLocked = false;
  [SerializeField] public bool YLocked = false;
  [SerializeField] public bool ZLocked = false;

  public delegate void LookDelegate();
  LookDelegate LookFunction;

  public delegate void LockDelegate(ref Vector3 towards);
  public LockDelegate AxisLockFunction;
  public LockDelegate AngleLockFunction; // Angle relative to parent's forward

  void Start()
  {
    Body = GetComponent<Rigidbody>();
    SetTargetType(TargetType);
    SetLocks(XLocked, YLocked, ZLocked);
  }

  void Update()
  {
    LookFunction();
  }

  ////////////////////////////////////////////// Getter/Setters /////////////////////////////////////
  public void SetObjTarget(GameObject obj)    { ObjTarget = obj; TargetType = TargetTypes.GameObject; LookFunction = ObjLookFunction; }
  public void SetPosTarget(Vector3 pos)       { PosTarget = pos; TargetType = TargetTypes.Position; LookFunction = PosLookFunction; }
  public void SetTargetType(TargetTypes type)
  {
    TargetType = type;
         if (TargetType == TargetTypes.Velocity)   { LookFunction = VelLookFunction; }
    else if (TargetType == TargetTypes.Position)   { LookFunction = PosLookFunction; }
    else if (TargetType == TargetTypes.GameObject) { LookFunction = ObjLookFunction; }
    else                                           { LookFunction = NoLookFunction; }
  }

  public void SetLocks(bool x, bool y, bool z)
  {
    // Set members
    XLocked = x;
    YLocked = y;
    ZLocked = z;

    // Determine look function based on locked axes
    AxisLockFunction = NoLocks;
    if (XLocked)
    {
      AxisLockFunction = XLockedFunc;
      if (YLocked)
        AxisLockFunction = XYLockedFunc;
      else if (ZLocked)
        AxisLockFunction = XZLockedFunc;
    }
    else if (YLocked)
    {
      AxisLockFunction = YLockedFunc;
      if (ZLocked)
        AxisLockFunction = YZLockedFunc;
    }
    else if (ZLocked)
    {
      AxisLockFunction = ZLockedFunc;
    }
  }

  ////////////////////////////////////////////// Look Functions /////////////////////////////////////
  void ObjLookFunction()
  {
    if (ObjTarget)
    {
      Vector3 towards = ObjTarget.transform.position + Offset - transform.position;
      AxisLockFunction(ref towards);
      if (AngleMaximum != 0.0f)
        AngleLock(ref towards);
      transform.forward = Vector3.RotateTowards(transform.forward, towards, TurnSpeed * Mathf.PI / 180.0f * Time.deltaTime, 0.0f);
    }
  }

  void PosLookFunction()
  {
    Vector3 towards = (PosTarget - transform.position);
    AxisLockFunction(ref towards);
    if (AngleMaximum != 0.0f)
      AngleLock(ref towards);
    transform.forward = Vector3.RotateTowards(transform.forward, towards, TurnSpeed * Mathf.PI / 180.0f * Time.deltaTime, 0.0f);
  }

  void VelLookFunction()
  {
    Vector3 vel = Body.velocity.normalized;

    // Guard against 0 length vectors
    if (vel.sqrMagnitude > 0.01)
    {
      AxisLockFunction(ref vel);
      transform.forward = Vector3.RotateTowards(transform.forward, vel, TurnSpeed * Mathf.PI / 180.0f * Time.deltaTime, 0.0f);
    }
  }

  void NoLookFunction() { }

  ////////////////////////////////////////////// Lock Functions /////////////////////////////////////
  void AngleLock(ref Vector3 towards)
  {
    Vector3 edgeLock = Vector3.RotateTowards(transform.parent.forward, towards, AngleMaximum, 0.0f);
    if (Vector3.Dot(towards, transform.forward) <= Vector3.Dot(edgeLock, transform.forward))
      towards = edgeLock;
  }

  void NoLocks(ref Vector3 towards)
  {
  }

  void XLockedFunc(ref Vector3 towards)
  {
    towards.x = transform.forward.x;
  }

  void YLockedFunc(ref Vector3 towards)
  {
    towards.y = transform.forward.y;
  }

  void ZLockedFunc(ref Vector3 towards)
  {
    towards.z = transform.forward.z;
  }

  void XYLockedFunc(ref Vector3 towards)
  {
    towards.x = transform.forward.x;
    towards.y = transform.forward.y;
  }

  void XZLockedFunc(ref Vector3 towards)
  {
    towards.y = transform.forward.y;
    towards.z = transform.forward.z;
  }

  void YZLockedFunc(ref Vector3 towards)
  {
    towards.y = transform.forward.y;
    towards.z = transform.forward.z;
  }
}
