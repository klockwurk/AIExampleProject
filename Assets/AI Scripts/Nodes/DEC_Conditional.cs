/*******************************************************************************/
/*!
\file   DEC_Conditional.cs
\author Khan Sweetman
\par    All content © 2017 DigiPen (USA) Corporation, all rights reserved.
\par    The Bakery

\brief
  Compares blackboard entries.
  
*/
/*******************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

public class DEC_Conditional : BTNode
{
  public enum ConditionalTypes
  {
    Equal,
    NotEqual,
    LessThan,
    MoreThan,
    LessOrEqual,
    MoreOrEqual
  }

  // ------------------------------------------------- Variables -------------------------------------------------- //
  public string BBKey1;
  public string BBKey2;
  public ConditionalTypes ConditionalType;

  // ------------------------------------------------- Life Cycle -------------------------------------------------- //
  public override void Initialize(object[] objs)
  {
    base.Initialize(objs);
  }

  public override void EnterBehavior()
  {
#if UNITY_EDITOR
    if (HasBreakpoint)
    {
      UnityEditor.EditorApplication.isPaused = true;
    }
#endif
    bool succeeded = CheckConditional();
    if (succeeded)
    {
      CurrStatus = BT_Status.Running;
      Children[0].EnterBehavior();
    }
    else
    {
      CurrStatus = BT_Status.Fail;
    }
  }

  public override BT_Status Update()
  {
    if (CurrStatus == BT_Status.Fail || CurrStatus == BT_Status.Success)
    {
      return CurrStatus;
    }
    return SetStatus(Children[0].Update());
  }

  protected virtual bool CheckConditional()
  {
    object first = Blackboard[BBKey1];
    object second = Blackboard[BBKey2];
    System.Type type = first.GetType(); // assume that both types are the same

    if (ConditionalType == ConditionalTypes.Equal)
    {
      if (type == typeof(float))
      {
        return (float)first == (float)second;
      }
      if (type == typeof(int))
      {
        return (int)first == (int)second;
      }
    }
    if (ConditionalType == ConditionalTypes.NotEqual)
    {
      if (type == typeof(float))
      {
        return (float)first != (float)second;
      }
      if (type == typeof(int))
      {
        return (int)first != (int)second;
      }
    }
    // > and < (probably)can't be done through reflection
    if (ConditionalType == ConditionalTypes.LessThan)
    {
      if (type == typeof(float))
      {
        return (float)first < (float)second;
      }
      if (type == typeof(int))
      {
        return (int)first < (int)second;
      }
    }
    if (ConditionalType == ConditionalTypes.MoreThan)
    {
      if (type == typeof(float))
      {
        return (float)first > (float)second;
      }
      if (type == typeof(int))
      {
        return (int)first > (int)second;
      }
    }
    if (ConditionalType == ConditionalTypes.LessOrEqual)
    {
      if (type == typeof(float))
      {
        return (float)first <= (float)second;
      }
      if (type == typeof(int))
      {
        return (int)first <= (int)second;
      }
    }
    if (ConditionalType == ConditionalTypes.MoreOrEqual)
    {
      if (type == typeof(float))
      {
        return (float)first >= (float)second;
      }
      if (type == typeof(int))
      {
        return (int)first >= (int)second;
      }
    }

    // should never hit this point
    throw new System.Exception();
  }
}
