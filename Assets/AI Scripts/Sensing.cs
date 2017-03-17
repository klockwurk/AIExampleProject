/*******************************************************************************/
/*!
\file   Sensing.cs
\author Khan Sweetman
\par    All content © 2015 DigiPen (USA) Corporation, all rights reserved.
\par    Golden Bullet Games
\brief
  File does does things. COOL things.
  
*/
/*******************************************************************************/

//#define DEBUG_HEARING
//#define DEBUG_SENSING
#define DEBUG_SENSING_EDGES
//#define DEBUG_LOG

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Sensable))]
public class Sensing : MonoBehaviour
{
  // ------------------------------------------------- Statics -------------------------------------------------- //
  public static HashSet<Sensing> RegisteredSensers = new HashSet<Sensing>();

  // ------------------------------------------------- Variables -------------------------------------------------- //
  public float NearSenseRadius = 2.0f;
  public float ViewConeLength = 7.0f;
  public float ViewConeAngle = 90.0f;
  public bool DetectsEnemies = false;
  public bool DetectsFriendlies = false;

  private Sensable _Sensable;
  private List<Sensable.FactionEnum> FactionsToSearchThrough = new List<Sensable.FactionEnum>();

  [System.NonSerialized] public HashSet<Sensable> DetectedObjects = new HashSet<Sensable>();
  [System.NonSerialized] public HashSet<Sensable> ObjectsDetectedThisFrame = new HashSet<Sensable>();

  private int HitMask;
  private int TicksSinceUpdate;

  // Event stuff
  public delegate void DetectionEvent(Sensable obj);
  public event DetectionEvent DetectedObject;
  public event DetectionEvent LostObject;

  // ------------------------------------------------- Life Cycle -------------------------------------------------- //
  void Start()
  {
    // Register
    RegisteredSensers.Add(this);

    gameObject.EventSubscribe<DamageInfo>("Damage", OnDamage);
    gameObject.EventSubscribe<GameObject>("Death", OnDeath);
    _Sensable = gameObject.GetComponent<Sensable>();
    if (DetectsEnemies)    { FactionsToSearchThrough.AddRange(Sensable.FindEnemyFactionsTo(_Sensable)); }
    if (DetectsFriendlies) { FactionsToSearchThrough.AddRange(Sensable.FindFriendlyFactionsTo(_Sensable)); }
    HitMask = LayerMask.GetMask("Default", "Player");
  }

  void Update()
  {
#if DEBUG_SENSING_EDGES
    // Debug sight
    float time = Time.deltaTime;
    Vector3 pos = transform.position + Vector3.RotateTowards(transform.forward, transform.right, ViewConeAngle * Mathf.PI / 360.0f, 0.0f) * ViewConeLength;
    Debug.DrawLine(transform.position, pos, Color.cyan, time);
    Vector3 center = transform.position + transform.forward * ViewConeLength;
    Debug.DrawLine(pos, center, Color.cyan, time);
    pos = transform.position + Vector3.RotateTowards(transform.forward, -transform.right, ViewConeAngle * Mathf.PI / 360.0f, 0.0f) * ViewConeLength;
    Debug.DrawLine(transform.position, pos, Color.cyan, time);
    Debug.DrawLine(pos, center, Color.cyan, time);

    // Debug near-sense
    Vector3 selfPos = transform.position;
    Debug.DrawLine(selfPos + transform.forward * NearSenseRadius, selfPos + transform.right * NearSenseRadius, Color.cyan, time);
    Debug.DrawLine(selfPos + transform.right * NearSenseRadius, selfPos - transform.forward * NearSenseRadius, Color.cyan, time);
    Debug.DrawLine(selfPos - transform.forward * NearSenseRadius, selfPos + -transform.right * NearSenseRadius, Color.cyan, time);
    Debug.DrawLine(selfPos + -transform.right * NearSenseRadius, selfPos + transform.forward * NearSenseRadius, Color.cyan, time);
#endif

    // Only update every once in a while
    if (TicksSinceUpdate > 0)
    {
      --TicksSinceUpdate;
      return;
    }
    TicksSinceUpdate = Random.Range(8, 12);

    SightUpdate();
    SenseUpdate();
    ResolveDetections();

    // Reset list
    ObjectsDetectedThisFrame.Clear();
  }

  public void OnDestroy()
  {
    gameObject.EventUnsubscribe<DamageInfo>("Damage", OnDamage);
    gameObject.EventUnsubscribe<GameObject>("Death", OnDeath);
    RegisteredSensers.Remove(this);
  }

  // ------------------------------------------------- Member Sensory Functions -------------------------------------------------- //
  void OnDamage(DamageInfo info)
  {
    // Assume we got damaged by the closest baker
    ObjectsDetectedThisFrame.Add(Sensable.FindClosestWithFactionTo(Sensable.FactionEnum.Baker, transform.position));
  }

  void OnDeath(GameObject instigator)
  {
    // Assume we got damaged by the closest baker
    ObjectsDetectedThisFrame.Add(Sensable.FindClosestWithFactionTo(Sensable.FactionEnum.Baker, transform.position));
  }

  void SightUpdate()
  {
    // Iterate through factions we want to detect
    for (int i = 0; i < FactionsToSearchThrough.Count; ++i)
    {
      foreach (Sensable target in Sensable.RegisteredObjects[FactionsToSearchThrough[i]])
      {
        // If within view cone
        Vector3 towardsCol = (target.gameObject.transform.position + target.HeadOffset
                            - transform.position - _Sensable.HeadOffset).normalized;
        if (Mathf.Acos(Vector3.Dot(towardsCol, transform.forward)) < (ViewConeAngle * Mathf.PI / 360.0f))
        {
          // If not behind something
          RaycastHit hitInfo;

          Physics.Raycast(transform.position + _Sensable.HeadOffset, towardsCol, out hitInfo, ViewConeLength, HitMask);
          if (Util.IsRelatedTo(hitInfo.collider, target.gameObject.transform))
          {
            // Passed all checks, we can see the object
            ObjectsDetectedThisFrame.Add(target);
            // Debug specific object sight
#if DEBUG_SENSING
            if (!DetectedObjects.Contains(target))
            {
              Debug.DrawLine(transform.position + _Sensable.HeadOffset, target.gameObject.transform.position + target.HeadOffset, Color.cyan, 1.0f);
              Debug.Log(gameObject + " sight sensed: " + target.gameObject + " from relative: " + hitInfo.collider.gameObject);
            }
#endif
          }
        }
      }
    }
  }

  void SenseUpdate()
  {
    // Search through factions we care about
    for (int i = 0; i < FactionsToSearchThrough.Count; ++i)
    {
      // Sense objects close to us
      foreach (Sensable sensable in Sensable.RegisteredObjects[FactionsToSearchThrough[i]])
      {
        float distSqr = Util.DistSqr(sensable.transform.position, transform.position);
        if (sensable.gameObject != gameObject && distSqr <= NearSenseRadius * NearSenseRadius)
        {
          ObjectsDetectedThisFrame.Add(sensable);
#if DEBUG_SENSING
          if (!DetectedObjects.Contains(sensable))
          {
            Debug.DrawLine(transform.position, sensable.gameObject.transform.position, Color.cyan, 1.0f);
            Debug.Log(gameObject + " aura sensed: " + sensable.gameObject + " at distance: " + distSqr);
          }
#endif
        }
      }
    }
  }

  // ------------------------------------------------- Static Sensory Functions -------------------------------------------------- //
  public static void ReportHearingEvent(Sensable target, float range, Vector3 origin)
  {
    foreach (Sensing sensing in RegisteredSensers)
    {
      if (target != sensing.gameObject
        && Util.DistSqr(sensing.gameObject.transform.position, origin) < range * range)
      {
        sensing.ObjectsDetectedThisFrame.Add(target);
#if DEBUG_HEARING
        Debug.DrawLine(origin, sensing.gameObject.transform.position, Color.cyan, 1.0f);
#endif
      }
    }
#if DEBUG_HEARING
    Util.DrawCircle(origin, Color.cyan, range, 1.0f);
#endif
  }

  public static void ReportGroupSenseEvent(Sensable target, float range, Vector3 origin, Sensable.FactionEnum faction)
  {
    foreach (Sensing sensing in RegisteredSensers)
    {
      if (target != sensing.gameObject
        && sensing.GetComponent<Sensable>().Faction == faction
        && Util.DistSqr(sensing.transform.position, origin) < range * range)
      {
        sensing.ObjectsDetectedThisFrame.Add(target);
      }
    }
  }

  // ------------------------------------------------- Helpers -------------------------------------------------- //
  private void ResolveDetections()
  {
    // Removing objects detected this frame leaves only objects we lost
    HashSet<Sensable> lostObjects = new HashSet<Sensable>(DetectedObjects);
    lostObjects.ExceptWith(ObjectsDetectedThisFrame);
    foreach (Sensable obj in lostObjects)
    {
      LostObject(obj);
    }

    // Removing objects we detected previously from ObjectsDetectedThisFrame only leaves new objects
    ObjectsDetectedThisFrame.ExceptWith(DetectedObjects);
    foreach (Sensable obj in ObjectsDetectedThisFrame)
    {
      DetectedObject(obj);
    }

    // Objects we are detecting is DetectedObjects U ObjectsDetectedThisFrame - LostObjects
    DetectedObjects.UnionWith(ObjectsDetectedThisFrame);
    DetectedObjects.ExceptWith(lostObjects);
  }
}
