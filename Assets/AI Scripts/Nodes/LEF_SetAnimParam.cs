/*******************************************************************************/
/*!
\file   LEF_SetAnimVariable.cs
\author Khan Sweetman
\par    All content © 2016-2017 DigiPen (USA) Corporation, all rights reserved.
\par    The Bakery
 
\brief
  ...
 
*/
/*******************************************************************************/
#define DEBUG_ANIM
#define SAFE_HASH

using UnityEngine;
using System.Collections;

public enum MoveState
{
  Idle = 0,
  Walk = 1,
  Run = 2
}

public enum ParamTypes
{
  Bool,
  Float,
  Int,
  Trigger
}

public class LEF_SetAnimParam : BTNode
{
  // ------------------------------------------------- Variables -------------------------------------------------- //
  public string ParamName;
  public ParamTypes ParamType;
  public bool BoolVal;
  public float FloatVal;
  public int IntVal;
  private Animator Anim;
  private int Hash;

  // ------------------------------------------------- Life Cycle -------------------------------------------------- //
  public override void Initialize(object[] objs)
  {
    base.Initialize(objs);
    Anim = Owner.GetComponent<Animator>();
    Hash = Anim.GetParameterHash(ParamName);
#if SAFE_HASH
    if (Hash == -1)
    {
      Debug.LogError("SetAnimParam did not have a proper ParamName on: " + Owner.name);
    }
#endif
  }

  public override void EnterBehavior()
  {
    switch(ParamType)
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
    SetStatus(BT_Status.Success);

#if DEBUG_ANIM
    switch (ParamType)
    {
      case ParamTypes.Bool:
        if (Anim.GetBool(Hash) != BoolVal)
        {
          Debug.Log("Animation parameter not consistent");
        }
        break;

      case ParamTypes.Float:
        if (Anim.GetFloat(Hash) != FloatVal)
        {
          Debug.Log("Animation parameter not consistent");
        }
        break;

      case ParamTypes.Int:
        if (Anim.GetInteger(Hash) != IntVal)
        {
          Debug.Log("Animation parameter not consistent");
        }
        break;
    }
#endif
  }

  public override void ExitBehavior()
  {
    base.ExitBehavior();
  }
}
