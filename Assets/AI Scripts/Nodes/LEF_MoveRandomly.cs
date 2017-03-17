/*******************************************************************************/
/*!
\file   LEF_MoveRandomly.cs
\author Khan Sweetman
\par    All content © 2015 DigiPen (USA) Corporation, all rights reserved.
\par    Golden Bullet Games
\brief
  File does does things. COOL things.
  
*/
/*******************************************************************************/

//#define DEBUG_MOVE_RANDOMLY

using UnityEngine;
using System.Collections;

[System.Serializable]
public class LEF_MoveRandomly : BTNode
{
  // ------------------------------------------------- Variables-------------------------------------------------- //
  public bool FailIfDirectionless;
  public float TimeSpentMoving = 1.0f;
  public float TimeSpentMovingRand = 0.25f;
  private float Timer;
  private Steering SteeringComponent;
  private Vector3 MoveDirection;
  private int HitMask;

  // ------------------------------------------------- Life Cycle -------------------------------------------------- //
  // First parameter should be owner
  // Second parameter should be root
  public override void Initialize(object[] objs)
  {
    base.Initialize(objs);
    SteeringComponent = Owner.GetComponent<Steering>();
    HitMask = LayerMask.GetMask("Furniture", "Default");
#if DEBUG_MOVE_RANDOMLY
    if (SteeringComponent == null)
    {
      Debug.LogError("AI ERROR: SteeringComponent null in LEF_MoveRandomly in " + name + " on " + Owner.name);
    }
#endif
  }

  public override void EnterBehavior()
  {
    // Entrance behavior
    base.EnterBehavior();
    SetStatus(BT_Status.Running);
    Timer = TimeSpentMoving + Random.Range(-TimeSpentMovingRand, TimeSpentMovingRand);

    // Find a valid direction
    bool validDirection = true;
    int attempts = 0;
    do
    {
      attempts++;
      MoveDirection = new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f));
      MoveDirection.Normalize();
      Vector3 startPoint = Owner.transform.position + Vector3.up;
      float magnitude = 2.0f; // arbitrary approximation. Steering speed is probably not accurate. Check later.
      validDirection = !Physics.Raycast(startPoint, MoveDirection, magnitude, HitMask);
#if DEBUG_MOVE_RANDOMLY
      Color debugColor = validDirection ? Color.green : Color.red;
      Debug.DrawRay(Owner.transform.position, MoveDirection * magnitude, debugColor, 2.0f);
#endif
    } while (!validDirection && attempts < 5);
    
    // Fail?
    if (!validDirection && FailIfDirectionless)
    {
      SetStatus(BT_Status.Fail);
    }
  }

  public override BT_Status Update()
  {
    Timer -= Time.fixedDeltaTime;
    if (Timer <= 0.0f)
    {
      CurrStatus = BT_Status.Success;
    }
    SteeringComponent.AddMovementInput(MoveDirection);
    return CurrStatus;
  }
}
