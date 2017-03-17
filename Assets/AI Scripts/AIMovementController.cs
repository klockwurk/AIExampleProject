/*******************************************************************************/
/*!
\file   AIMovementController.cs
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

// ------------------------------------------------- Events -------------------------------------------------- //
public delegate void ArrivalDelegate(Vector3 pos);

public abstract class AIMovementController : MonoBehaviour
{
  // ------------------------------------------------- Variables -------------------------------------------------- //
  protected Vector3 Destination;
  public float MoveSpeed;
  public event ArrivalDelegate ArrivalEvent;

  // ------------------------------------------------- Public Interface -------------------------------------------------- //
  public abstract void AddMovementInput(Vector3 dir);

  public abstract void Jump(float speed);

  public abstract void Jump(Vector3 vec);

  public abstract void SetDestination(Vector3 position);

  public abstract void Brake();

  public abstract void CancelBraking();

  public abstract void AbortPathing();

  // Used to call ArrivalEvent by children
  protected void OnArrival(Vector3 pos)
  {
    if (ArrivalEvent != null)
      ArrivalEvent(pos);
  }
}
