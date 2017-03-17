/*******************************************************************************/
/*!
\file   BTInspector.cs
\author Khan Sweetman
\par    All content © 2015 DigiPen (USA) Corporation, all rights reserved.
\par    Golden Bullet Games
\brief
  File does does things. COOL things.
  
*/
/*******************************************************************************/
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Reflection;

[CustomEditor(typeof(BTAsset))]
public class BTAssetInspector : Editor
{
  // ------------------------------------------------- Variables -------------------------------------------------- //
  private GUIStyle NodeSelectedStyle;
  private GUIStyle NoNodeSelectedStyle;
  private GUIStyle TreeSelectedStyle;
  private GUIStyle NoTreeSelectedStyle;

  // ------------------------------------------------- Life Cycle -------------------------------------------------- //
  public void OnEnable()
  {
    GenerateStyles();
    if (BTEditorManager.Manager != null)
    {
      DestroyImmediate(BTEditorManager.Manager);
    }
    BTAsset asset = (BTAsset)serializedObject.targetObject;
    BTEditorManager.CreateInstance(asset);
  }            

  public void InWindowGUI()
  {
    // Draw the node GUI if we have a node selected
    NodeTitleGUI();
    if (BTEditorManager.Manager && BTEditorManager.Manager.SelectedNode != null)
    {
      BTEditorManager.Manager.SelectedNode.DrawInspector();
    }
  }

  // Controls what appears when looking at a BTAsset in the inspector
  public override void OnInspectorGUI()
  {
    serializedObject.Update();
    BTAsset script = target as BTAsset;

    // Title
    TreeTitleGUI();

    // Draw the node GUI if we have a node selected
    NodeTitleGUI();
    if (BTEditorManager.Manager && BTEditorManager.Manager.SelectedNode != null)
    {
      BTEditorManager.Manager.SelectedNode.DrawInspector();
    }
    // Button for showing BTEditorWindow with this BTAsset
    if (GUILayout.Button("Show Behavior Tree Editor"))
    {
      BTEditorManager.CreateInstance(script);
      BTEditorWindow.ShowWindow();
    }

    if (GUI.changed)
    {
      BTEditorManager.Manager.Dirty();
    }

    Repaint();
    serializedObject.ApplyModifiedProperties();
  }

  // ------------------------------------------------- Draw Functions -------------------------------------------------- //
  private void NodeTitleGUI()
  {
    if (BTEditorManager.Manager.SelectedNode != null)
    {
      string title = "Node Selected: " + BTEditorManager.Manager.SelectedNode.GetType().ToString();
      EditorGUILayout.LabelField(title, NodeSelectedStyle);
    }
    else
    {
      string title = "No Node Selected";
      EditorGUILayout.LabelField(title, NoNodeSelectedStyle);
    }
  }

  private void TreeTitleGUI()
  {
    if (BTEditorManager.Manager != null && BTEditorManager.Manager.Tree != null)
    {
      string title = BTEditorManager.Manager.Asset.name;
      EditorGUILayout.LabelField(title, TreeSelectedStyle);
    }
    else
    {
      string title = "No Behavior Tree Selected";
      EditorGUILayout.LabelField(title, NoTreeSelectedStyle);
    }
    
    EditorGUILayout.Space();
  }

  // ------------------------------------------------- Helpers -------------------------------------------------- //
  private void GenerateStyles()
  {
    NodeSelectedStyle = new GUIStyle();
    NodeSelectedStyle.fontSize = 14;

    NoNodeSelectedStyle = new GUIStyle();
    NoNodeSelectedStyle.fontSize = 14;
    NoNodeSelectedStyle.normal.textColor = Color.gray;

    TreeSelectedStyle = new GUIStyle();
    TreeSelectedStyle.fontSize = 20;

    NoTreeSelectedStyle = new GUIStyle();
    NoTreeSelectedStyle.fontSize = 20;
    NoTreeSelectedStyle.normal.textColor = Color.gray;
  }
}
