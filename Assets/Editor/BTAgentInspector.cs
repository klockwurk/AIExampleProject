/*******************************************************************************/
/*!
\file   BTAgentInspector.cs
\author Khan Sweetman
\par    All content © 2015 DigiPen (USA) Corporation, all rights reserved.
\par    Golden Bullet Games
\brief
  Custom editor for BTAgent
  
*/
/*******************************************************************************/
using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(BTAgent))]
public class BTAgentInspector : Editor
{
  // ------------------------------------- Life Cycle ------------------------------------- //
  public void OnEnable()
  {
    BTAgent script = target as BTAgent;

    // Make sure the target BTAgent is refreshed and ready for inspection
    if(script.Tree == null)
      script.Awake();

    // Prepare the editor
    if(script.Asset != null)
    {
      if (BTEditorManager.Manager == null)
      {
        BTEditorManager.CreateInstance(script.Asset);
      }
      else
      {
        BTEditorManager.Manager.Tree = script.Tree;
      }
    }
  }

  public override void OnInspectorGUI()
  {
    serializedObject.Update();
    DrawDefaultInspector();
    BTAgent script = (BTAgent)target;

    // Button
    if (GUILayout.Button("Show Behavior Tree Editor"))
    {
      // Set selected BehaviorTree as active BehaviorTree in Editor
      if (script.Asset != null)
        BTEditorManager.CreateInstance(script.Asset);

      // Show BTEditorWindow
      BTEditorWindow.ShowWindow();
    }

    if(script.Asset != null)
      EditorUtility.SetDirty(script.Asset);

    serializedObject.ApplyModifiedProperties();
  }
}
