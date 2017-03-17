/*******************************************************************************/
/*!
\file   Melee.cs
\author Khan Sweetman
\par    All content © 2015 DigiPen (USA) Corporation, all rights reserved.
\par    Golden Bullet Games
\brief
  AI Melee logic.
  
*/
/*******************************************************************************/

//#define ANIM_DEBUG

using UnityEngine;
using System.Collections;

public class AIMelee : MonoBehaviour
{
  Animator _Animator;

  public float Damage = 1.0f;
  public float Range = 2.0f;
  public float HandHeight = 0.8f;
  public float RecoverTime = 1.0f;

  [System.NonSerialized]
  public GameObject Target;
  [System.NonSerialized]
  private int BitMask = int.MaxValue;
  private int MeleeTriggerKey;
  private int IsMeleeingKey;

  void Start()
  {
    BitMask = LayerMask.GetMask("Player", "Default", "Furniture");
    _Animator = GetComponentInChildren<Animator>();

    MeleeTriggerKey = _Animator.GetParameterHash("MeleeTrigger");
    IsMeleeingKey = _Animator.GetParameterHash("IsMeleeingKey");

    AnimationStateEventBehaviour meleeBehaviour = _Animator.GetBehaviour<AnimationStateEventBehaviour>();
    meleeBehaviour.StateExitDel += OnMeleeAnimFinished;
  }

  // ------------------------------------------------- Interface -------------------------------------------------- //
  public GameObject Swing()
  {
#if ANIM_DEBUG
    Debug.Log("Started Melee");
#endif
    // Animation/fake animation if we don't have animation
    if (_Animator.runtimeAnimatorController != null)
    {
      _Animator.SetTrigger(MeleeTriggerKey);
      _Animator.SetBool(IsMeleeingKey, true);
    }

    // Return who we are swinging towards
    return Target;
  }

  // ------------------------------------------------- Event Stuff ------------------------------------------------- //
  public delegate void MeleeEvent();
  public event MeleeEvent MeleeFinished;
  public void MeleeFinish()
  {
#if ANIM_DEBUG
    Debug.Log("Finished Melee");
#endif
    if (MeleeFinished != null)
      MeleeFinished();

    // Animation
    _Animator.SetBool(IsMeleeingKey, false);
  }

  private void OnMeleeAnimFinished(AnimatorStateInfo stateInfo, int layerIndex)
  {
    MeleeFinish();
  }

  public void MeleeStart1() { MeleeStart(); }
  public void MeleeStart2() { MeleeStart(); }
  public void MeleeStart3() { MeleeStart(); }
  public void MeleeStart4() { MeleeStart(); }
  public void MeleePeak1() { MeleePeak(); }
  public void MeleePeak2() { MeleePeak(); }
  public void MeleePeak3() { MeleePeak(); }

  public void MeleePeak4() { MeleePeak(); }

  public void MeleePeak()
  {
#if ANIM_DEBUG
    Debug.Log("Melee peak");
#endif

    // Calculate forward and position
    Vector3 forward = transform.forward;
    Vector3 pos = transform.position; pos.y += HandHeight;

    // Find closest player, since only enemies use this
    Sensable player = Sensable.FindClosestWithFactionTo(Sensable.FactionEnum.Baker, transform.position);
    if (Util.DistSqr(player.transform.position, transform.position) < Range * Range)
    {
      // Raycast check
      RaycastHit hitInfo;
      Physics.Raycast(pos, player.transform.position - pos, out hitInfo, Range, BitMask);
      if (Util.IsRelatedTo(hitInfo.collider, player.transform))
      {
        Target = hitInfo.collider.gameObject;

        // Damage
        
        EnemyHealth logic = Target.GetComponentInChildren<EnemyHealth>();
        if (logic != null)
        {
          //logic.TakeDamage(1);
        }
        else
        {
          //logic = Target.transform.root.GetComponentInChildren<HealthLogic>();
          //if (logic != null)
          //{
//            logic.TakeDamage(1);
  //        }
        }
      }
    }

    // Reset target
    Target = null;

    // Debug draw sphere
#if ANIM_DEBUG
    GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    Destroy(sphere.GetComponent<Collider>());
    sphere.AddComponent<TimedDeath>();
    sphere.GetComponent<TimedDeath>().TimeUntilDeath = 0.2f;
    sphere.GetComponent<TimedDeath>().Destroys = true;
    sphere.GetComponent<MeshRenderer>().material.SetFloat("_Mode", 3.0f);
    sphere.GetComponent<MeshRenderer>().material.SetColor("_Color", new Color(0.5f, 0.3f, 0.0f, 0.3f));
    sphere.transform.position = new Vector3(transform.position.x, transform.position.y + HandHeight, transform.position.z) + transform.forward * 0.5f;
    sphere.transform.localScale = new Vector3(Range, Range, Range);
#endif
  }

  public void MeleeStart()
  {
  }

  // ------------------------------------------------- Life Cycle -------------------------------------------------- //
  public void OnDestroy()
  {
    if(_Animator != null)
    {
      _Animator.Stop();
    }
  }
}