/*******************************************************************************/
/*!
\file   BTNode.cs
\author Khan Sweetman
\par    All content © 2015 DigiPen (USA) Corporation, all rights reserved.
\par    Golden Bullet Games
\brief
  All behavior tree nodes inherit from here. Child nodes should following the
  naming convention:
    - DEC_Decorator
    - LEF_Leaf
    - SEL_Selector

*/
/*******************************************************************************/

using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum BT_Status
{
  Entering,
  Running,
  Success,
  Fail
}

[System.Serializable]
public class BTNode : ScriptableObject, System.IComparable
{
  // ------------------------------------------------- Variables -------------------------------------------------- //
  private static int CurrIDData;

#if UNITY_EDITOR
  public bool HasBreakpoint = false;
  [SerializeField] public Vector2 EditorPosition;
  [System.NonSerialized] public Vector2 EditorOffset;
#endif

  public string Name = "Node";
  public int ID = -1;
  public BT_Status CurrStatus
  {
    get { return CurrStatusData; }
    set
    {
      SetStatus(value);
    }
  }
  private BT_Status CurrStatusData = BT_Status.Entering;

  [System.NonSerialized] public BTNode Root;
  [System.NonSerialized] public int CurrIndex = 0;
  [System.NonSerialized] public GameObject Owner;
  [System.NonSerialized] public BehaviorTree BTree;
  [System.NonSerialized] public List<BTNode> Children = new List<BTNode>();
  [System.NonSerialized] public BTNode Parent;

  public bool Running { get { return CurrStatus == BT_Status.Running; } }
  public BlackboardComponent Blackboard { get { return BTree.Blackboard; } }
  public Transform transform { get { return Owner.transform; } }
  public int Depth
  {
    get
    {
      int depth = 1;
      while (Parent != null)
      {
        depth += Parent.Depth;
      }
      return depth;
    }
  }

  // --------------------------------------- Life Cycle --------------------------------------- //
  public static int GetNewID { get { return ++CurrIDData; } }

  // First parameter should be owner
  // Second parameter should be root
  public virtual void Initialize(object[] objs)
  {
    // Store key variables
    Owner = (GameObject)objs[0];
    Root = (BTNode)objs[1];
    BTree = (BehaviorTree)objs[2];

    // Recursively initialize children
    for (int i = 0; i < Children.Count; ++i)
    {
      Children[i].Initialize(objs);
    }
  }

  public virtual void EnterBehavior()
  {
#if UNITY_EDITOR
    if (HasBreakpoint)
    {
      EditorApplication.isPaused = true;
    }
#endif

    // Base behavior
    SetStatus(BT_Status.Running);
    for (int i = 0; i < Children.Count; ++i)
    {
      Children[i].EnterBehavior();
    }

    // Do whatever you need to do when you enter this node
    // ...
  }

  public virtual void ExitBehavior()
  {
    // Base behavior
    SetStatus(BT_Status.Success);
    for (int i = 0; i < Children.Count; ++i)
    {
      if (Children[i].CurrStatus == BT_Status.Running)
      {
        Children[i].ExitBehavior();
      }
    }

    // Do whatever you need to do if you're interrupted and need to exit
    // ...
  }

  public virtual BT_Status Update()
  {
    return CurrStatus;
  }

  public void OnEnable()
  {
    hideFlags = HideFlags.HideAndDontSave;
  }

  public virtual void EnterAtIndex(int index)
  {
    // used by DEC_ImpulseReceiver
    // used to set active subtree outside of normal means
    CurrIndex = 0;
    SetStatus(BT_Status.Running);
  }

  // Returns current utility of node. Should only be implementd for certain nodes
  public virtual float Utility()
  {
    return 0.5f; // arbitrary, medium value
  }

  public virtual BTNode GetShallowCopy()
  {
    return (BTNode)this.MemberwiseClone();
  }

  // ----------------------------- Editor Manipulation ----------------------------- //
  public virtual void ConnectChild(BTNode child)
  {
    child.Parent = this;
    Children.Add(child);
  }
  public virtual void DisconnectFromParent()
  {
    if (Parent != null)
    {
      Parent.Children.Remove(this);
    }
    Parent = null;
  }

  // IComparable for sorting left-to-right in the visual editor
  public int CompareTo(object other)
  {
    return EditorPosition.x < ((BTNode)other).EditorPosition.x ? -1 : 1;
  }

  // Sorts children by left->right order editor positions
  public void SortSiblings()
  {
    if (Parent != null)
    {
      Parent.Children.Sort();
    }
  }

  // --------------------------------------- Helper Functions ---------------------------------------//
  public virtual BT_Status SetStatus(BT_Status status)
  {
    // Entrance behavior
    CurrStatusData = status;
    if (CurrStatusData == BT_Status.Entering)
    {
      EnterBehavior();
    }

    return status;
  }

  public string PrintTree(string tree = "", int depth = 0)
  {
    // Format self
    string self = "";
    if (depth != 0)
      self += "\n";
    for (int i = 0; i < depth; ++i)
      self += "  ";
    self += Name;
    
    // If leaf, return self
    if (Children == null)
      return self;
    tree += self;

    // Add children to tree
    foreach (BTNode child in Children)
      tree += child.PrintTree(tree, depth + 1);

    // If not root, recurse
    if(depth != 0)
      return PrintTree(tree, depth + 1);

    // If root, print
    Debug.Log(tree);
    return tree;
  }

  public void LogInvalidKey(string key)
  {
    Debug.LogError("AI ERROR: Could not find blackboard key: " + key + " in " + GetType().Name + " in tree: " + BTree.Name);
  }

  // ------------------------------------------------- UI Stuff -------------------------------------------------- //
#if UNITY_EDITOR
  public virtual void DrawInspector()
  {
    // draw script inspection field
    EditorGUILayout.Space();
    MonoScript script = MonoScript.FromScriptableObject(this);
    EditorGUILayout.ObjectField(new GUIContent("Node Script: "), script, script.GetType(), false);
    EditorGUILayout.Space();

    // Display serialized fields
    FieldInfo[] fields = this.GetType().GetFields();
    foreach (FieldInfo field in fields)
    {
      if (field.IsNotSerialized)
        continue;
      if (field.Name == "EditorPosition")
        continue;
      if (field.Name == "NodeType")
        continue;
      if (field.Name == "HasBreakpoint")
        continue;

      DrawField(field);
    }
  }

  public void DrawField(FieldInfo field)
  {
    System.Type type = field.FieldType;

    // Get value text
    object obj = field.GetValue(this);

    // Determine field to draw based on type
    if (type == typeof(string))
      field.SetValue(this, EditorGUILayout.TextField(field.Name, (string)obj));
    else if (type == typeof(float))
      field.SetValue(this, EditorGUILayout.FloatField(field.Name, (float)obj));
    else if (type == typeof(int))
      field.SetValue(this, EditorGUILayout.IntField(field.Name, (int)obj));
    else if (type == typeof(bool))
      field.SetValue(this, EditorGUILayout.Toggle(field.Name, (bool)obj));
    else if (type == typeof(Vector2))
      field.SetValue(this, EditorGUILayout.Vector2Field(field.Name, (Vector2)obj));
    else if (type == typeof(Vector3))
      field.SetValue(this, EditorGUILayout.Vector3Field(field.Name, (Vector3)obj));
    else if (type == typeof(Color))
      field.SetValue(this, EditorGUILayout.ColorField(field.Name, (Color)obj));
    else if (type.IsEnum)
      field.SetValue(this, EditorGUILayout.EnumPopup(field.Name, (System.Enum)obj));
    else if (type == typeof(GameObject))
      field.SetValue(this, EditorGUILayout.ObjectField(field.Name, (GameObject)obj, type, false));
    else
    {
      // ...
    }
  }
#endif
}
