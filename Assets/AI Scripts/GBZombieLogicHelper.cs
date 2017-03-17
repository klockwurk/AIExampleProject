/*******************************************************************************/
/*!
\file   GBZombieAnimationHelper.cs
\author Khan Sweetman
\par    All content © 2016-2017 DigiPen (USA) Corporation, all rights reserved.
\par    The Bakery
 
\brief
  ...
 
*/
/*******************************************************************************/

using UnityEngine;
using System.Collections;

public enum ZombieTypes
{
  Normal,
  Stalker
}

public class GBZombieLogicHelper : AILogicHelper
{
  // ------------------------------------------------- Variables -------------------------------------------------- //
  public AnimationCurve ProximityBlend;
  public float GiggleMaxDistance;
  public Collider Buffer;

  private Aggro AggroComponent;
  private Vector3 prevPosition;
  private Vector3 CurrVelocity;
  private float PrevRightness;
  private int ProximityHash;
  private int RightnessHash;

  // ------------------------------------------------- Initialization -------------------------------------------------- //
  public void Start()
  {
    Anim = GetComponent<Animator>();
    Blackboard = GetComponent<BlackboardComponent>();

    // Animation variables
    prevPosition = transform.position;
    ProximityHash = Anim.GetParameterHash("DistanceVal");
    RightnessHash = Anim.GetParameterHash("Rightness");

    // Check if we fall through the floor
    StartCoroutine(KillYCheck());

    // Horde registration
    HordeManager.RegisterZombie(this);
    if (HordeManager.HordeActive || Type == ZombieTypes.Stalker)
    {
      GetComponent<Aggro>().SetTarget(Sensable.RegisteredObjects[Sensable.FactionEnum.Baker].RandomElement());
#if DEBUG_STALKER
      if (Type == ZombieTypes.Stalker)
      {
        Debug.DrawLine(transform.position + Vector3.up, Sensable.GetPlayer1().position, Color.red, 5.0f);
      }
#endif
    }

    // Variant(s). Currently just stalker
    switch (Type)
    {
      case ZombieTypes.Normal:
        break;
      case ZombieTypes.Stalker:
        GetComponent<Aggro>().AggroTime = 9000;
        break;
    }

    // Initialize blackboard
    Blackboard["Player"] = Sensable.FindClosestWithFactionTo(Sensable.FactionEnum.Baker, transform.position);
    Blackboard["Target"] = ((Sensable)Blackboard["Player"]).transform;

    // Events
    gameObject.EventSubscribe<GameObject>("Death", OnDeath);

    // Make each zombie look a little different
    Randomize();
  }

  private void Randomize()
  {
    // Scale
    transform.localScale *= Random.Range(0.95f, 1.2f);
  }

  // ------------------------------------------------- Updating -------------------------------------------------- //
  public void Update()
  {
    if (string.Equals("GBZombie_AnimDummy", gameObject.name, System.StringComparison.Ordinal))
    {
      if (Input.GetKeyDown(KeyCode.Y))
      {
        gameObject.EventSend<DamageInfo>("Damage", new DamageInfo(1.0f, Sensable.GetPlayer1().gameObject));
      }
    }

    AnimUpdate();
    VelocityUpdate();
  }

  private void VelocityUpdate()
  {
    CurrVelocity = (transform.position - prevPosition) / Time.deltaTime;
    prevPosition = transform.position;
  }

  private void AnimUpdate()
  {
    // distance value
    float val = Util.DistSqr(Sensable.GetPlayer1().transform.position, transform.position);
    val = ProximityBlend.Evaluate(val);
    Anim.SetFloat(ProximityHash, val);

    // rightness value
    Vector3 localVel = transform.InverseTransformVector(CurrVelocity);
    localVel.Normalize();
    // clamp maximum rightness delta
    float diff = localVel.x - PrevRightness;
    float sign = diff != 0 ? diff / Mathf.Abs(diff) : 1;
    if (Mathf.Abs(diff) > 0.07f)
    {
      diff = 0.07f * sign;
    }
    float rightness = PrevRightness + diff;
    Anim.SetFloat(RightnessHash, rightness);
    PrevRightness = rightness;
  }

  private IEnumerator KillYCheck()
  {
    yield return new WaitForSeconds(5.0f);
    if (transform.position.y < -100)
    {
      Despawn();
    }
    StartCoroutine(KillYCheck());
  }

  private IEnumerator GiggleUpdate()
  {
    while (true)
    {
      // Stalking
      if (AggroComponent.Aggroed)
      {
        const float stalkMinTime = 1;
        const float stalkMaxTime = 5;
        yield return new WaitForSeconds(Random.Range(stalkMinTime, stalkMaxTime));
        //AkSoundEngine.PostEvent("Play_GingerBreadStalk", gameObject);
      }
      // Giggling
      else
      {
        const float giggleMinTime = 2;
        const float giggleMaxTime = 4;
        yield return new WaitForSeconds(Random.Range(giggleMinTime, giggleMaxTime));
        if (Util.DistSqr(Sensable.GetPlayer1().transform.position, transform.position) < GiggleMaxDistance * GiggleMaxDistance)
        {
          //AkSoundEngine.PostEvent("Play_GingerBreadLaugh", gameObject);
        }
      }
    }
  }

  // ------------------------------------------------- Events -------------------------------------------------- //
  private void OnDeath(GameObject instigator)
  {
    HordeManager.ReportAIDeath(this);
    if (OwnerPartition != null)
    {
      OwnerPartition.ReportAIDeath(gameObject);
    }

    // Get rid of buffer sphere
    Destroy(Buffer);

    // Change layer to so our corpse can be walked over/through
    foreach (Transform child in GetComponentsInChildren<Transform>())
    {
      const int corpseLayer = 9;
      child.gameObject.layer = corpseLayer; 
    }
    gameObject.EventUnsubscribe<GameObject>("Death", OnDeath);
  }
}
