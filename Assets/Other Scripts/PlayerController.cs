/*******************************************************************************/
/*!
\file   PlayerController.cs
\author Khan Sweetman
\par    All content © 2016-2017 DigiPen (USA) Corporation, all rights reserved.
\par    The Bakery
 
\brief
  ...
 
*/
/*******************************************************************************/
#define DEBUG_MOVE_TO
#define DEBUG_LAUNCH

using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
  // ------------------------------------------------- Variables -------------------------------------------------- //
  private Camera Cam;
  private NavMeshAgent NavAgent;
  private AudioSource AudioComponent;

  public float BaseSpeed;

  private Vector3 MoveToLocation;
  private bool ShouldRaycast;
  private bool AtDestination;
  private int GroundMask;

  private bool GodModeData;
  public bool GodMode
  {
    get { return GodModeData; }
    set
    {
      if (value)
      {
        BaseSpeed *= 3.0f;
      }
      else
      {
        BaseSpeed /= 3.0f;
      }
    }
  }

  private Vector3 FootPosition { get { return new Vector3(transform.position.x, transform.position.y, transform.position.z); } }

  // ------------------------------------------------- Life Cycle -------------------------------------------------- //
  void Start()
  {
    Cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    NavAgent = gameObject.GetComponent<NavMeshAgent>();
    AudioComponent = GetComponent<AudioSource>();

    GroundMask = LayerMask.GetMask("Ground");
  }

  void Update()
  {
    if (Input.GetMouseButtonDown(0))
    {
      ShouldRaycast = true;
    }

    // Check for destination ending
    if (!AtDestination)
    {
      ArrivalUpdate();
    }
  }

  void FixedUpdate()
  {
    // Movement
    if (ShouldRaycast)
    {
      MoveClick();
      ShouldRaycast = false;
    }
  }

  // ------------------------------------------------- Movement -------------------------------------------------- //
  private Vector3 MouseGroundPosition()
  {
    // Figure out in world space where we clicked
    Vector3 mouseScreen = Input.mousePosition;
    mouseScreen.z = 100000.0f;
    Vector3 mouseWorld = Cam.ScreenToWorldPoint(mouseScreen);

    // raycast to that point to find the spot on the ground
    Vector3 hitLocation = Vector3.zero;
    RaycastHit hitInfo;
    if (Physics.Raycast(Cam.transform.position, mouseWorld, out hitInfo, 100.0f, GroundMask))
    {
#if DEBUG_MOVE_TO
        Util.DrawSphere(hitInfo.point, Color.red, 1.0f, 1.0f);
        Debug.DrawLine(Cam.transform.position + Vector3.up * 0.01f, hitInfo.point, Color.red, 1.0f);
        Debug.DrawLine(mouseWorld, hitInfo.point, Color.red, 1.0f);
        Debug.DrawLine(Cam.transform.position + Vector3.up * 0.01f, mouseWorld, Color.red, 1.0f);
#endif
      return hitInfo.point;
    }
    return Vector3.zero;
  }

  private void MoveClick()
  {
    // tell q-chan to move to location
    SetMoveToLocation(MouseGroundPosition());
  }

  private void SetMoveToLocation(Vector3 vec)
  {
    // tell q-chan to move to location
    MoveToLocation = vec;
    NavAgent.SetDestination(MoveToLocation);
    AtDestination = false;
  }

  private void ArrivalUpdate()
  {
    // Arriving?
    if (Util.DistSqr(NavAgent.destination, FootPosition) < 0.5f)
    {
      AtDestination = true;
    }
  }
}
