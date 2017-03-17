/*******************************************************************************/
/*!
\file   Aggro.cs
\author Khan Sweetman
\par    All content © 2015 DigiPen (USA) Corporation, all rights reserved.
\par    Golden Bullet Games
\brief
  ...
  
*/
/*******************************************************************************/

//#define DEBUG_AGGRO

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Sensing))]
public class Aggro : MonoBehaviour
{
  // ------------------------------------------------- Variables -------------------------------------------------- //
  private Sensing SensingComponent;
  private Sensable SensableComponent;
  private BlackboardComponent Blackboard;

  [System.NonSerialized] public Sensable Target;

  public string BBTargetKey;
  public float SenseMultiplier = 2.0f;
  public float AggroTime = 1.0f;
  public bool ForwardsAggroToTeammates = true;
  public float AggroForwardingRange = 10.0f;
  [System.NonSerialized] public float AggroTimer = 0.0f;
  [System.NonSerialized] public bool CanSeeTarget = false;
  [System.NonSerialized] public bool Aggroed = false;

  // ------------------------------------------------- Interface -------------------------------------------------- //
  public void SetTarget(Sensable obj)
  {
    // Set sensing to aggressive
    if (Aggroed == false)
    {
      SensingComponent.ViewConeLength *= SenseMultiplier;
      SensingComponent.ViewConeAngle *= 2.0f;
    }

    // Update aggro status
    Target = obj;
    AggroTimer = AggroTime;
    Aggroed = true;
    CanSeeTarget = true;
    if (ForwardsAggroToTeammates)
    {
      NotifyTeammates(Target);
    }

    // update blackboard
    Blackboard[BBTargetKey] = obj.transform;

#if DEBUG_AGGRO
      foreach (var renderer in gameObject.GetComponentsInChildren<Renderer>())
        renderer.material.color = Color.red;
#endif
  }

  // ------------------------------------------------- Event Stuff -------------------------------------------------- //
  void OnObjectDetectedResponse(Sensable obj)
  {
    // Only consider fightable objects for aggro
    if (!Sensable.IsEnemyTo(SensableComponent, obj))
      return;

    // Refresh timer if it's the old target
    if (obj == Target)
    {
      SetTarget(obj);
    }

    // Take a target if we don't have one
    if (Target == null)
    {
      gameObject.EventSend<GameObject>("AggroTriggered", obj.gameObject);
      SetTarget(obj);
    }
    // Reprioritize if something more interesting comes by
    else if (obj.GetComponent<Sensable>().Priority > Target.GetComponent<Sensable>().Priority)
    {
      SetTarget(obj);
    }
  }

  void OnObjectLostResponse(Sensable obj)
  {
    if (obj == null || Target == null)
      return;

    if (obj == Target)
    {
      //LastKnownLocation = Target.transform.position;
      CanSeeTarget = false;
#if DEBUG_AGGRO
        foreach (var renderer in gameObject.GetComponentsInChildren<Renderer>())
          renderer.material.color = Color.magenta;
#endif
    }
  }

  // ------------------------------------------------- Life Cycle -------------------------------------------------- //
  void Start()
  {
    // Grab components
    SensingComponent = GetComponent<Sensing>();
    SensableComponent = GetComponent<Sensable>();
    Blackboard = GetComponent<BlackboardComponent>();

    // Set up event connections
    SensingComponent.DetectedObject += OnObjectDetectedResponse;
    SensingComponent.LostObject += OnObjectLostResponse;
  }

  void Update ()
  {
    // constantly recheck aggro
    if (HordeManager.HordeActive)
    {
      Aggroed = true;
    }

    // Aggroed?
    if (Aggroed)
    {
      // Update time
      if (CanSeeTarget == false)
      {
        AggroTimer -= Time.deltaTime;
      }
      else
      {
        AggroTimer = AggroTime;
      }

      // Turn off aggro?
      if (AggroTimer < 0.0f)
      {
        LoseAggro();
      }
    }

#if DEBUG_AGGRO
    foreach(Renderer r in GetComponentsInChildren<Renderer>())
    {
      r.material.color = Aggroed ? Color.red : Color.white;
    }
#endif
  }

  // ------------------------------------------------- Helpers -------------------------------------------------- //
  void LoseAggro()
  {
    // Set sensing to passive
    SensingComponent.ViewConeLength /= SenseMultiplier;
    SensingComponent.ViewConeAngle /= 2.0f;

    // notify
    gameObject.EventSend<GameObject>("AggroLost", Target.gameObject);

    Target = null;
    Aggroed = false;

#if DEBUG_AGGRO
    foreach (var renderer in gameObject.GetComponentsInChildren<Renderer>())
      renderer.material.color = Color.grey;
#endif
  }

  private void NotifyTeammates(Sensable obj)
  {
    Sensing.ReportGroupSenseEvent(obj, AggroForwardingRange, transform.position, SensableComponent.Faction);
#if DEBUG_AGGRO
      Util.DrawCircle(transform.position, Color.red, AggroForwardingRange, 1.0f);
#endif
  }
}
