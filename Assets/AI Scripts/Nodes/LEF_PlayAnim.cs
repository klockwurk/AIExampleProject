/*******************************************************************************/
/*!
\file   LEF_PlayAnim.cs
\author Khan Sweetman
\par    All content © 2016-2017 DigiPen (USA) Corporation, all rights reserved.
\par    The Bakery
 
\brief
  -Similar to LEF_SetAnimParam. 
  -Automatically ends after animation finishes
 
*/
/*******************************************************************************/

//#define DEBUG_ANIM

using UnityEngine;
using System.Collections;

public class LEF_PlayAnim : BTNode
{
  // ------------------------------------------------- Variables -------------------------------------------------- //
  public string ParamName;
  public ParamTypes ParamType;
  public bool BoolVal;
  public float FloatVal;
  public int IntVal;

  private Animator Anim;
  private int Hash;
  private IEnumerator FinishCoroutine;

  // ------------------------------------------------- Life Cycle -------------------------------------------------- //
  public override void Initialize(object[] objs)
  {
    base.Initialize(objs);
    Anim = Owner.GetComponent<Animator>();
    Hash = Anim.GetParameterHash(ParamName);
    FinishCoroutine = Finish();
#if SAFE_HASH
    if (Hash == -1)
    {
      Debug.LogError("SetAnimParam did not have a proper ParamName on: " + Owner.name);
    }
#endif
  }

  public override void EnterBehavior()
  {
    base.EnterBehavior();

    // Change state by changing variable
    switch (ParamType)
    {
      case ParamTypes.Bool:
        Anim.SetBool(Hash, BoolVal);
        break;

      case ParamTypes.Float:
        Anim.SetFloat(Hash, FloatVal);
        break;

      case ParamTypes.Int:
        Anim.SetInteger(Hash, IntVal);
        break;

      case ParamTypes.Trigger:
        Anim.SetTrigger(Hash);
        break;
    }
    
    // Finish after animation finishes
    Blackboard.StopCoroutine(FinishCoroutine);
    FinishCoroutine = Finish();
    Blackboard.StartCoroutine(FinishCoroutine);
  }

  public override void ExitBehavior()
  {
    base.ExitBehavior();
    Blackboard.StopCoroutine(FinishCoroutine);
  }

  // ------------------------------------------------- Helpers -------------------------------------------------- //
  private IEnumerator Finish()
  {
    // Wait until animation finishes
    yield return new WaitForEndOfFrame(); // make sure we're in the correct state

    float currStateLength = Anim.GetCurrentAnimatorStateInfo(0).length;
#if DEBUG_ANIM
    Debug.Log("CurrStateLength: " + currStateLength);
#endif
    yield return new WaitForSeconds(currStateLength);

    // Finish
    SetStatus(BT_Status.Success);

#if DEBUG_ANIM
    Debug.Log("Finished animation");
#endif
  }
}
