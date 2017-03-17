/*******************************************************************************/
/*!
\file   BlackboardInspector.cs
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
using UnityEditor;

[CustomEditor(typeof(BlackboardComponent))]
public class BlackboardInspector : Editor
{
  // ------------------------------------------------- Variables -------------------------------------------------- //


  // ------------------------------------------------- Life Cycle -------------------------------------------------- //
  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();
    //ScriptUI();
    //EntriesUI();
  }

  private void ScriptUI()
  {
    //EditorGUILayout.Space();
    //MonoScript script = MonoScript.FromScriptableObject(this);
    //EditorGUILayout.ObjectField(new GUIContent("Script: "), script, script.GetType(), false);
    //EditorGUILayout.Space();
  }

  private void EntriesUI()
  {
    //SerializedProperty entries = serializedObject.FindProperty("SerializedEntries");

    //GUILayout.Label("Keys:");
    //for (int i = 0; i < entries.arraySize; ++i)
    //{
    //  GUIContent label = new GUIContent("Key:");
    //  EditorGUILayout.PropertyField(entries.GetArrayElementAtIndex(i), label);
    //}
  }
}

[CustomEditor(typeof(BBEntry))]
public class BBEntryInspector : Editor
{
  public override void OnInspectorGUI()
  {
    serializedObject.Update();

    // find properties
    SerializedProperty keyProp = serializedObject.FindProperty("Key");
    SerializedProperty valueProp = serializedObject.FindProperty("Value");
    SerializedProperty descProp = serializedObject.FindProperty("Description");

    // make layouts
    EditorGUILayout.PropertyField(keyProp);
    EditorGUILayout.PropertyField(valueProp);
    EditorGUILayout.PropertyField(descProp);

    serializedObject.ApplyModifiedProperties();
  }
}
