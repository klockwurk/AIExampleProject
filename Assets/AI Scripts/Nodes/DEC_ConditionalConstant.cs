/*******************************************************************************/
/*!
\file   DEC_ConditionalConstant.cs
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

public class DEC_ConditionalConstant : DEC_Conditional
{
  // ------------------------------------------------- Variables -------------------------------------------------- //
  public float ComparisonValue;

  // ------------------------------------------------- Inheritance -------------------------------------------------- //
  protected override bool CheckConditional()
  {
    object first = Blackboard[BBKey1];
    System.Type type = first.GetType(); // assume that both types are the same

    if (ConditionalType == ConditionalTypes.Equal)
    {
      if (type == typeof(float))
      {
        return (float)first == ComparisonValue;
      }
      if (type == typeof(int))
      {
        return (int)first == (int)ComparisonValue;
      }
    }
    if (ConditionalType == ConditionalTypes.NotEqual)
    {
      if (type == typeof(float))
      {
        return (float)first != ComparisonValue;
      }
      if (type == typeof(int))
      {
        return (int)first != (int)ComparisonValue;
      }
    }
    // > and < (probably)can't be done through reflection
    if (ConditionalType == ConditionalTypes.LessThan)
    {
      if (type == typeof(float))
      {
        return (float)first < ComparisonValue;
      }
      if (type == typeof(int))
      {
        return (int)first < (int)ComparisonValue;
      }
    }
    if (ConditionalType == ConditionalTypes.MoreThan)
    {
      if (type == typeof(float))
      {
        return (float)first > ComparisonValue;
      }
      if (type == typeof(int))
      {
        return (int)first > (int)ComparisonValue;
      }
    }
    if (ConditionalType == ConditionalTypes.LessOrEqual)
    {
      if (type == typeof(float))
      {
        return (float)first <= ComparisonValue;
      }
      if (type == typeof(int))
      {
        return (int)first <= (int)ComparisonValue;
      }
    }
    if (ConditionalType == ConditionalTypes.MoreOrEqual)
    {
      if (type == typeof(float))
      {
        return (float)first >= ComparisonValue;
      }
      if (type == typeof(int))
      {
        return (int)first >= (int)ComparisonValue;
      }
    }

    // should never hit this point
    throw new System.Exception();
  }

#if UNITY_EDITOR
  public override void DrawInspector()
  {
    // draw script inspection field
    UnityEditor.EditorGUILayout.BeginVertical();
    UnityEditor.EditorGUILayout.Space();
    UnityEditor.MonoScript script = UnityEditor.MonoScript.FromScriptableObject(this);
    UnityEditor.EditorGUILayout.ObjectField(new GUIContent("Node Script: "), script, script.GetType(), false);
    UnityEditor.EditorGUILayout.Space();

    // Display serialized fields
    System.Reflection.FieldInfo[] fields = this.GetType().GetFields();
    foreach (System.Reflection.FieldInfo field in fields)
    {
      if (field.IsNotSerialized)
        continue;
      if (field.Name == "EditorPosition")
        continue;
      if (field.Name == "NodeType")
        continue;
      if (field.Name == "HasBreakpoint")
        continue;
      if (field.Name == "BBKey2") // overrode DrawInspector to avoid drawing BBKey2. Kind of jank.
        continue;

      DrawField(field);
    }
    UnityEditor.EditorGUILayout.EndVertical();
  }
#endif
}
