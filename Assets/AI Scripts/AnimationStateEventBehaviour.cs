/*******************************************************************************/
/*!
\file   WeakPointHolder.cs
\author Khan Sweetman
\par    All content © 2017 DigiPen (USA) Corporation, all rights reserved.
\par    The Bakery

\brief
  ...
  
*/
/*******************************************************************************/
#define DEBUG_CHECK_EVENT_NAMES

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationStateEventBehaviour : StateMachineBehaviour
{
  // ------------------------------------------------- Variables -------------------------------------------------- //
  public static readonly string DEFAULT_NAME = "";

  public delegate void AnimDel(AnimatorStateInfo stateInfo, int layerIndex);
  public AnimDel StateEnterDel;
  public AnimDel StateUpdateDel;
  public AnimDel StateExitDel;

  public string NameID = DEFAULT_NAME;

  // ------------------------------------------------- Animation Events -------------------------------------------------- //
#if DEBUG_CHECK_EVENT_NAMES
  public void Awake()
  {
    if (string.Equals(NameID, DEFAULT_NAME))
    {
      Debug.LogError("Unset NameID on: " + name);
    }
  }
#endif

  // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
  override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
  {
    //Debug.Log("State enter");
    if (StateEnterDel != null)
    {
      StateEnterDel(stateInfo, layerIndex);
    }
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
  {
    //Debug.Log("State update");
    if (StateUpdateDel != null)
    {
      StateUpdateDel(stateInfo, layerIndex);
    }
  }

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
  {
    //Debug.Log("State exit");
    if (StateExitDel != null)
    {
      StateExitDel(stateInfo, layerIndex);
    }
  }

  // OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects 
  // root motion should be implemented here
	override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
  {
	  
	}

  // OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up 
  // animation IK (inverse kinematics) should be implemented here.
  override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
  {
	  
	}
}
