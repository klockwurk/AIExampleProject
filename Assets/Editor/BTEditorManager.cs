/*******************************************************************************/
/*!
\file   BTEditorManager.cs
\author Khan Sweetman
\par    All content © 2015 DigiPen (USA) Corporation, all rights reserved.
\par    Golden Bullet Games
\brief
  - BTInspector creates an instance of this class every time OnEnable is called
  - Main use is as the sole route for BehaviorTree manipulation
    - Exposes a selected BehaviorTree to BTEditorWindow
  
*/
/*******************************************************************************/
using UnityEngine;
using UnityEditor;
using System.IO;

public class BTEditorManager : ScriptableObject
{
  // ------------------------------------------------- Variables -------------------------------------------------- //
  // Reference to the single EditorWindow
  public BTEditorWindow EditorWindow;

  // Data being currently edited
  public BTNode SelectedNode;
  public BehaviorTree Tree;
  public BTAsset Asset;

  // Manager is created through calls to CreateInstance, and destroyed whenever. Singleton
  public static BTEditorManager Manager
  {
    get
    {
      if (ManagerData == null)
      {
        ManagerData = CreateInstance(null);
      }
      return ManagerData;
    }
  }
  private static BTEditorManager ManagerData;

  // ------------------------------------------------- Primary Interface -------------------------------------------------- //
  // Shortcut for creating a new BTAsset
  // Placed in Assets folder
  [MenuItem("Assets/Create/Behavior Tree", false, 1)]
  public static void CreateNewBehaviorTree(MenuCommand menuCommand)
  {
    CreateNewBehaviorTree();
  }

  public static void CreateNewBehaviorTree()
  {
    // Make asset
    BTAsset asset = ScriptableObject.CreateInstance<BTAsset>();
    asset.BTree = new BehaviorTree();
    BTNode node = asset.BTree.MakeNode(typeof(SEL_Parallel));
    asset.BTree.SetRoot(node);
    asset.Serialize();

    // Put asset where it needs to be
    string path = AssetDatabase.GetAssetPath(Selection.activeObject);
    string name = "/New Behavior Tree.asset";
    if (path == "")
    {
      path = "Assets/";
    }
    else
    {
      if (File.Exists(path))
      {
        path = Path.GetDirectoryName(path);
      }
    }
    int currNum = 0;
    while (File.Exists(path + name))
    {
      name = "/New Behavior Tree " + currNum + ".asset";
      ++currNum;
    }

    path = AssetDatabase.GenerateUniqueAssetPath(path + name);
    AssetDatabase.CreateAsset(asset, path);
    AssetDatabase.Refresh();
    Selection.activeObject = asset;
  }

  // Creates a new manager and sets its data
  public static BTEditorManager CreateInstance(BTAsset asset)
  {
    if (ManagerData != null)
    {
      DestroyImmediate(ManagerData);
      ManagerData = null;
    }

    // Make new manager, set its data
    ManagerData = ScriptableObject.CreateInstance<BTEditorManager>();
    ManagerData.Asset = asset;
    if (ManagerData.Asset != null)
    {
      ManagerData.Tree = asset.Deserialize();
    }

    return ManagerData;
  }

  // ------------------------------------------------- Life Cycle -------------------------------------------------- //
  public void OnEnable()
  {
    hideFlags = HideFlags.HideAndDontSave;
  }

  public void OnDestroy()
  {
    ManagerData = null;
  }

  // ------------------------------------------------- Editor Functions -------------------------------------------------- //
  public void AddChild(object obj)
  {
    Manager.Tree.AddChild(obj);
    ((AddChildData)obj).selectedNode.SortSiblings();
    Dirty();
  }

  public void ChangeType(object obj)
  {
    Manager.Tree.ChangeType(obj);
    ((AddChildData)obj).selectedNode.SortSiblings();
    Dirty();
  }

  public void SetRoot(object obj)
  {
    Manager.Tree.SetRoot(obj);
    Dirty();
  }

  public void DeleteNode(object obj)
  {
    Manager.Tree.DeleteNode(obj);
    Dirty();
  }

  public void ToggleBreakpoint(object obj)
  {
    BTNode node = (BTNode)obj;
    node.HasBreakpoint = !node.HasBreakpoint;
  }

  public void ChangeParent(object obj)
  {
    // Change mouse state
    EditorWindow.CurrMouseState = MouseState.SelectingNode;
    Dirty();
  }

  public void ChangeParentReal(object obj)
  {
    // Call function
    Manager.Tree.ChangeParent(obj);
    Dirty();
  }

  public void Dirty()
  {
    if (EditorWindow != null)
      EditorWindow.Repaint();
    Asset.Serialize();
    EditorUtility.SetDirty(Asset);
  }

  // ------------------------------------------------- Helpers -------------------------------------------------- //
  public void Deselect()
  {
    Manager.EditorWindow = null;
    Manager.SelectedNode = null;
  }

  public void SelectNode(BTNode node)
  {
    BTEditorManager.Manager.SelectedNode = node;
  }
}

