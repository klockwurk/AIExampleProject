/*******************************************************************************/
/*!
\file   BTEditorWindow.cs
\author Khan Sweetman
\par    All content © 2015 DigiPen (USA) Corporation, all rights reserved.
\par    Golden Bullet Games
\brief
  Behavior Tree Editor Window. Allows editing of BTAssets.
  
*/
/*******************************************************************************/
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public enum MouseState
{
  DraggingNode,
  DraggingWindow,
  Scrolling,
  Down,
  Up,
  SelectingNode
}

public class BTEditorWindow : EditorWindow
{
  // ------------------------------------------------- Variables -------------------------------------------------- //
  public View MyView;
  public BTEditorManager EditorManager;
  public BTAssetInspector BTInspector; // BTInspector for active BehaviorTree
  public MouseState CurrMouseState = MouseState.Up;
  public Vector2 ClickOffset = Vector2.zero;

  private bool MouseDown;
  private GUIStyle ButtonStyle;
  private GUIStyle EditorTitleStyle;
  private GUIStyle TreeSelectedStyle;
  private GUIStyle NoTreeSelectedStyle;
  private Texture2D SaveTexture;

  public Rect InspectorRect
  {
    get
    {
      return new Rect(position.width - 400, 0, 400, position.height);
    }
  }

  public Rect MiscRect
  {
    get
    {
      return new Rect(InspectorRect.x, InspectorRect.y, 400, 300);
    }
  }

  // ------------------------------------------------- Life Cycle -------------------------------------------------- //
  [MenuItem("Window/Behavior Tree Editor")]
  public static void ShowWindow()
  {
    BTEditorWindow window = (BTEditorWindow)EditorWindow.GetWindow(typeof(BTEditorWindow));
    window.minSize = new Vector2(100, 100);
    window.titleContent.text = "Behavior Tree Editor";
  }

  // --------------------------------- OnEvent Functions --------------------------------- //
  void OnSelectionChange()
  {
    if (Selection.activeGameObject != null)
    {
      BTAgent agent = Selection.activeGameObject.transform.root.GetComponentInChildren<BTAgent>();
      if (agent != null)
      {
        BTEditorManager.Manager.Asset = agent.Asset;
        //BTEditorManager.Manager.Asset.Deserialize();
        BTEditorManager.Manager.Tree = agent.Tree;
      }
    }
    Repaint();
  }

  void OnEnable()
  {
    if (MyView == null)
    {
      MyView = new View(this);
    }
    GenerateStyles();
    if (BTEditorManager.Manager != null && BTEditorManager.Manager.Asset != null)
    {
      BTInspector = (BTAssetInspector)Editor.CreateEditor(BTEditorManager.Manager.Asset);
    }

    GenerateStyles();
    BTEditorManager.Manager.EditorWindow = this;
  }

  void OnDisable()
  {
    if (BTInspector)
      DestroyImmediate(BTInspector);

    // Shut down current BTEditorManager
    if (BTEditorManager.Manager != null)
      BTEditorManager.Manager.Deselect();
  }

  void OnDestroy()
  {
    // Shut down current BTEditorManager
    if (BTEditorManager.Manager != null)
      BTEditorManager.Manager.Deselect();
  }

  void OnGUI()
  {
    MyView.DrawBackground();

    // Draw BTAsset if there's a BTAsset selected
    bool assetSelected = BTEditorManager.Manager != null && BTEditorManager.Manager.Asset != null;
    if (assetSelected)
    {
      // Connect to the current manager
      BTEditorManager.Manager.EditorWindow = this;
      BTEditorManager.Manager.Tree = BTEditorManager.Manager.Asset.BTree;

      // Draw Tree
      MyView.DrawActiveTree(); // BUG - Uses scrollWheel event. Has to go after HandleMouseEvents
      MyView.ResizeCanvas();
    }

    // UI that is always present
    GUILayout.BeginArea(InspectorRect);
    TitleGUI();
    AssetsGUI();

    EditorGUILayout.BeginHorizontal();
    SaveGUI();
    CenterTreeGUI();
    NewTreeGUI();
    EditorGUILayout.EndHorizontal();

    InspectorGUI();
    GUILayout.EndArea();

    Repaint();
    if (assetSelected)
    {
      HandleMouseEvents();
      HandleKeyboard();
    }
  }

  // ------------------------------------------------- GUI Functions -------------------------------------------------- //
  private void TitleGUI()
  {
    EditorGUILayout.BeginVertical();
    EditorGUILayout.Space();
    EditorGUILayout.LabelField("Behavior Tree Editor", EditorTitleStyle);
    if (BTEditorManager.Manager.Asset != null)
    {
      EditorGUILayout.SelectableLabel(BTEditorManager.Manager.Asset.name, TreeSelectedStyle);
    }
    else
    {
      EditorGUILayout.SelectableLabel("No Tree Selected", NoTreeSelectedStyle);
    }
    EditorGUILayout.Space();
    EditorGUILayout.EndVertical();
  }

  private void AssetsGUI()
  {
    EditorGUILayout.BeginVertical();
    BTEditorManager.Manager.Asset = (BTAsset)EditorGUILayout.ObjectField("Behavior Tree Asset: ", BTEditorManager.Manager.Asset,
       typeof(BTAsset), false);
    EditorGUILayout.EndVertical();
  }

  private void InspectorGUI()
  {
    if (BTInspector == null)
    {
      BTInspector = (BTAssetInspector)Editor.CreateEditor(BTEditorManager.Manager.Asset, typeof(BTAssetInspector));
    }
    if (BTInspector != null)
    {
      BTInspector.InWindowGUI();
    }
  }

  private void SaveGUI()
  {
    EditorGUILayout.BeginVertical();
    if (GUILayout.Button("Save"))
    {
      Save();
    }
    EditorGUILayout.EndVertical();
  }

  private void CenterTreeGUI()
  {
    EditorGUILayout.BeginVertical();
    if (GUILayout.Button("Center Tree"))
    {
      ResetPosition();
    }
    EditorGUILayout.EndVertical();
  }

  private void NewTreeGUI()
  {
    EditorGUILayout.BeginVertical();
    if (GUILayout.Button("New Tree"))
    {
      BTEditorManager.CreateNewBehaviorTree();
    }
    EditorGUILayout.EndVertical();
  }

  // --------------------------------- Editor Manipulation Functions --------------------------------- //
  public void HandleMouseEvents()
  {
    switch (Event.current.type)
    {
      case EventType.MouseDown:
        // See if we're clicking on a node
        BTNode clickedNode = ClickingOnNode(BTEditorManager.Manager.Tree.Root);
        if (clickedNode != null)
        {
          // Special stuff in case we were setting a new parent
          if (CurrMouseState == MouseState.SelectingNode)
          {
            BTNode[] nodes = { BTEditorManager.Manager.SelectedNode, clickedNode };
            BTEditorManager.Manager.Tree.ChangeParent(nodes);
          }

          // Selection
          BTEditorManager.Manager.SelectNode(clickedNode);
          CurrMouseState = MouseState.DraggingNode;
          ClickOffset = BTEditorManager.Manager.SelectedNode.EditorPosition - Event.current.mousePosition;
        }
        // Might be clicking on a BTNode's inspector
        else
        {
          // If not clicking on a node or the inspector, deselect (select null node)
          if (InspectorRect.Contains(Event.current.mousePosition) == false)
          {
            BTEditorManager.Manager.SelectNode(null);
            CurrMouseState = MouseState.Down;
          }
        }

        // Right click
        // 0 - left
        // 1 - right
        // 2 - middle
        if (Event.current.button == 1)
          ShowContextMenu(Event.current.mousePosition);
        else if (Event.current.button == 2)
          CurrMouseState = MouseState.Scrolling;
        break;

      case EventType.MouseDrag:
        // Dragging node?
        if (CurrMouseState == MouseState.DraggingNode)
        {
          BTEditorManager.Manager.SelectedNode.EditorPosition = Event.current.mousePosition + ClickOffset;
          RecursivelyDragChildren(BTEditorManager.Manager.SelectedNode, Event.current.delta);
        }

        // Scrolling screen?
        else if (CurrMouseState == MouseState.Scrolling)
        {
          RecursivelyDragChildren(BTEditorManager.Manager.Tree.Root, Event.current.delta);
          BTEditorManager.Manager.Tree.Root.EditorPosition += Event.current.delta;

          // Scroll background so it looks like we're actually scrolling
          GridRenderer.Offset += Event.current.delta;
        }
        break;

      case EventType.MouseUp:
        // Recalculate child positions when we let go of a node
        if (CurrMouseState == MouseState.DraggingNode && BTEditorManager.Manager.SelectedNode != null)
        {
          BTEditorManager.Manager.SelectedNode.SortSiblings();
        }

        // Update mouse state
        CurrMouseState = MouseState.Up;
        break;

      case EventType.ScrollWheel:
        // Scale tree
        Matrix4x4 scale = Matrix4x4.Scale(new Vector3(1.0f - Event.current.delta.y * 0.016f, 1.0f - Event.current.delta.y * 0.016f, 1.0f));
        Vector2 translate = Event.current.mousePosition;
        ScaleChildrenRecursive(BTEditorManager.Manager.Tree.Root, ref translate, ref scale);

        // Scale grid offset
        Vector2 offset = GridRenderer.Offset;
        offset -= translate;
        offset = scale.MultiplyPoint(offset);
        offset += translate;
        GridRenderer.Offset = offset;

        // Scale grid scale
        Vector2 newScale = scale.MultiplyVector(new Vector3(GridRenderer.TileWidth, GridRenderer.TileHeight));
        GridRenderer.TileWidth = newScale.x;
        GridRenderer.TileHeight = newScale.y;
        break;
    }
  }

  public void ScaleChildrenRecursive(BTNode node, ref Vector2 origin, ref Matrix4x4 scale)
  {
    node.EditorPosition -= origin;
    node.EditorPosition = scale.MultiplyPoint(node.EditorPosition);
    node.EditorPosition += origin;

    foreach (BTNode child in node.Children)
      ScaleChildrenRecursive(child, ref origin, ref scale);
  }

  public void HandleKeyboard()
  {
    if (Event.current.type == EventType.KeyDown)
    {
      // Del
      if (Event.current.keyCode == KeyCode.Delete)
      {
        // Do we have a node selected?
        if (BTEditorManager.Manager.SelectedNode != null)
          BTEditorManager.Manager.SelectedNode.BTree.DeleteNode(BTEditorManager.Manager.SelectedNode);
      }
    }
  }

  BTNode ClickingOnNode(BTNode node)
  {
    if (MyView.NRenderer.GetRect(node, MyView.ScrollPoint).Contains(Event.current.mousePosition))
      return node;

    BTNode selected = null;
    foreach (BTNode child in node.Children)
    {
      selected = ClickingOnNode(child);
      if (selected != null)
        return selected;
    }

    return null;
  }

  void RecursivelyDragChildren(BTNode node, Vector2 delta)
  {
    foreach (BTNode child in node.Children)
    {
      child.EditorPosition += delta;
      RecursivelyDragChildren(child, delta);
    }
  }

  // --------------------------------------------- Button Functions --------------------------------------------- //
  public void ShowContextMenu(Vector2 point)
  {
    var menu = new GenericMenu();

    // Right-clicking on empty space
    if (BTEditorManager.Manager.SelectedNode == null)
    {
      menu.AddItem(new GUIContent("Option Thing"), true, null);
    }

    // Right-clicking on a node
    else
    {
      // Add child
      List<System.Type> childTypes = new List<System.Type>();
      Util.GetChildClassesOf(typeof(BTNode), ref childTypes);
      foreach (System.Type type in childTypes)
      {
        AddChildData childData = new AddChildData(BTEditorManager.Manager.SelectedNode, type);
        menu.AddItem(new GUIContent("Add Child/" + type.ToString()), false, BTEditorManager.Manager.AddChild, childData);
      }

      // Change node type
      foreach (System.Type type in childTypes)
      {
        AddChildData changeData = new AddChildData(BTEditorManager.Manager.SelectedNode, type);
        menu.AddItem(new GUIContent("Change Type/" + type.ToString()), false, BTEditorManager.Manager.ChangeType, changeData);
      }

      // Set as Root
      menu.AddItem(new GUIContent("Set Root"), false, BTEditorManager.Manager.SetRoot, BTEditorManager.Manager.SelectedNode);

      // Set break point
      menu.AddItem(new GUIContent("Toggle Breakpoint"), false, BTEditorManager.Manager.ToggleBreakpoint, BTEditorManager.Manager.SelectedNode);

      // Parent to
      menu.AddItem(new GUIContent("Change Parent"), false, BTEditorManager.Manager.ChangeParent, BTEditorManager.Manager.SelectedNode);

      // Delete
      menu.AddItem(new GUIContent("Delete"), false, BTEditorManager.Manager.DeleteNode, BTEditorManager.Manager.SelectedNode);
    }

    // Make context menu appear?
    menu.DropDown(new Rect(point.x, point.y, 0, 0));
  }

  // Hypothetically saves the behavior tree
  // Not really necessary. Kind of does it on its own. The magic of serialization.
  public void Save()
  {
    BTEditorManager.Manager.Asset.Serialize();
    AssetDatabase.Refresh();
    AssetDatabase.SaveAssets();
  }

  public void ResetPosition()
  {
    Vector2 offset = new Vector2(this.position.width / 4.0f, 25.0f);
    RecursivelyDragChildren(BTEditorManager.Manager.Tree.Root, -BTEditorManager.Manager.Tree.Root.EditorPosition + offset);
    BTEditorManager.Manager.Tree.Root.EditorPosition = offset;

    // Rest grid to make it look like we snapped to the beginning
    GridRenderer.Offset = Vector2.zero;
    GridRenderer.TileHeight = 120.0f;
    GridRenderer.TileWidth = 120.0f;
  }

  // --------------------------------- Helper Functions --------------------------------- //
  void GenerateStyles()
  {
    EditorTitleStyle = new GUIStyle();
    EditorTitleStyle.fontSize = 25;
    EditorTitleStyle.alignment = TextAnchor.MiddleCenter;

    TreeSelectedStyle = new GUIStyle();
    TreeSelectedStyle.fontSize = 18;
    TreeSelectedStyle.alignment = TextAnchor.MiddleCenter;

    NoTreeSelectedStyle = new GUIStyle();
    NoTreeSelectedStyle.fontSize = 14;
    NoTreeSelectedStyle.normal.textColor = Color.gray;
    NoTreeSelectedStyle.alignment = TextAnchor.MiddleCenter;
  }
}
