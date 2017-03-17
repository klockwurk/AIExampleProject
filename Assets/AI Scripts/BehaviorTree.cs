/*******************************************************************************/
/*!
\file   BehaviorTree.cs
\author Khan Sweetman
\par    All content © 2016 DigiPen (USA) Corporation, all rights reserved.
\par    The Bakery
\brief
  
  
*/
/*******************************************************************************/
//#define DEBUG_BT

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

// Class passed into AddChild member function
public class AddChildData
{
  public BTNode selectedNode;  // reference to selected node. BTEditorManager is not accessable in non-editor contexts.
  public System.Type type;     // type of node to add.

  public AddChildData(BTNode _selectedNode, System.Type _type)
  {
    selectedNode = _selectedNode;
    type = _type;
  }
}

[System.Serializable]
public class BehaviorTree /*: ScriptableObject*/
{
  public BTAgent Agent;
  public BlackboardComponent Blackboard;
  public BTNode Root = null;

  public string Name = "Tree";

  // -------------------------------- Public Interface -------------------------------- //
  public BTNode FindNodeByID(int id)
  {
    return FindNodeRecursive(Root, id);
  }

  BTNode FindNodeRecursive(BTNode node, int id)
  {
    // Check self
    if (node.ID == id)
      return node;

    // Check children
    BTNode toReturn = null;
    foreach (BTNode child in node.Children)
    {
      toReturn = FindNodeRecursive(child, id);
      if (toReturn != null)
        return toReturn;
    }

    // Defalt to null if we can't find it
    return null;
  }

  public void ExitBehaviorRecursive()
  {
    // BTNodes should call this on running children/clean up as they exit
    foreach (var child in Root.Children)
      if (child.CurrStatus == BT_Status.Running)
        child.ExitBehavior();
  }

  void DestroyNodesRecursive(BTNode node)
  {
    foreach (BTNode child in node.Children)
      DestroyNodesRecursive(child);
    MonoBehaviour.DestroyImmediate(node);
  }

  private BTNode helperDeepCopy(BTNode ogNode)
  {
    //{
    //  BTNode newNode = ogNode.GetShallowCopy();
    //  for (int i = 0; i < newNode.Children.Count; ++i)
    //  {
    //    newNode.Children[i].Parent = null;
    //  }
    //  return newNode;
    //}

    //{
    //return ogNode.GetShallowCopy();
    //}

    //Construct - set method
    System.Type nodeType = null;
    System.Reflection.FieldInfo[] fieldInfos = { };
    {
      // Construct new object
      nodeType = ogNode.GetType();
      fieldInfos = nodeType.GetFields();
      //System.Type[] types = { };
      //System.Reflection.ConstructorInfo info = nodeType.GetConstructor(types);
      //object[] objects = { };
      //BTNode node = (BTNode)info.Invoke(objects);
      BTNode node = (BTNode)ScriptableObject.CreateInstance(nodeType);

      // Set fields
      for (int i = 0; i < fieldInfos.Length; ++i)
      {
        if (fieldInfos[i].IsNotSerialized)
        {
          continue;
        }

        //System.TypedReference newNodeRef = System.TypedReference.MakeTypedReference(node, fieldInfos);
        //System.TypedReference ogNodeRef = System.TypedReference.MakeTypedReference(ogNode, fieldInfos);
        //fieldInfos[i].SetValueDirect(newNodeRef, fieldInfos[i].GetValueDirect(ogNodeRef));
        fieldInfos[i].SetValue(node, fieldInfos[i].GetValue(ogNode));
      }
      return node;
    }
  }

  public void DeepCopy(ref BehaviorTree other)
  {
    // set root of other
    other.Root = helperDeepCopy(this.Root);

    // push root of this onto stack
    Stack<BTNode> stack = new Stack<BTNode>();
    Stack<BTNode> copyStack = new Stack<BTNode>();
    stack.Push(this.Root);
    copyStack.Push(other.Root);

    //while nodes left on stack
    while (stack.Count != 0)
    {
      // pop top
      BTNode ogTop = stack.Pop();
      BTNode copyTop = copyStack.Pop();

      // add unexplored children to stack
      // explore children (shallow copy, set parent of all children to current node)
      for (int i = 0; i < ogTop.Children.Count; ++i)
      {
        BTNode copyChild = helperDeepCopy(ogTop.Children[i]);
        copyTop.ConnectChild(copyChild);

        stack.Push(ogTop.Children[i]);
        copyStack.Push(copyTop.Children[i]);
      }
    }
  }

  // -------------------------------- Life Cycle -------------------------------- //
  public void Initialize(GameObject obj)
  {
    // Initialize blackboard
    Blackboard = obj.GetComponent<BlackboardComponent>();
#if DEBUG_BT
    if (Blackboard == null)
    {
      Debug.LogError("AI ERROR: Blackboard component missing on: " + obj);
    }
#endif

    // Initialize tree
    // - Initialization just handles passing Owner, Root, debug level, BehaviorTree
    object[] parameters = { obj, Root, this };
    Root.Initialize(parameters);
    Root.SetStatus(BT_Status.Entering);
  }

  public void OnEnable()
  {
    //hideFlags = HideFlags.HideAndDontSave;
  }

  public void OnDestroy()
  {
    DestroyNodesRecursive(Root);
  }

  // --------------------------------------- Editor Functions --------------------------------------- //
  public BTNode MakeNode(System.Type type)
  {
    // Make node based on its data type
    BTNode node = (BTNode) ScriptableObject.CreateInstance(type);

    // Add node
    node.BTree = this;
    return node;
  }
  
  // obj passed in needs to be an AddChildData
  public void AddChild(object obj)
  {
    AddChildData data = (AddChildData)obj;

    // Make node based on its data type
    BTNode node = (BTNode)ScriptableObject.CreateInstance(data.type);
    node.EditorPosition = data.selectedNode.EditorPosition + new Vector2(0, 120);

    // Add node
    node.BTree = this;
    data.selectedNode.Children.Add(node);
    node.Parent = data.selectedNode;
    node.Name = node.GetType().ToString();
    if (node.ID == -1) node.ID = BTNode.GetNewID; // Only get new ID if we're at the default one. CreateInstance populates fields before calling constructor.
  }

  // obj passed in needs to be an AddChildData
  // Works by replacing the old node with a new node of the different type
  public void ChangeType(object obj)
  {
    AddChildData data = (AddChildData)obj;

    // Make new node based on its data type
    System.Type type = System.Type.GetType(data.type.ToString());
    BTNode node = (BTNode)ScriptableObject.CreateInstance(type);

    // Copy old node
    node.BTree = data.selectedNode.BTree;
    node.Parent = data.selectedNode.Parent;
    node.EditorPosition = data.selectedNode.EditorPosition;
    node.Owner = data.selectedNode.Owner;
    node.Root = data.selectedNode.Root;

    if(node.Parent != null)
      node.Parent.Children.Add(node);

    foreach (BTNode child in data.selectedNode.Children)
      node.Children.Add(child);
    foreach (BTNode child in data.selectedNode.Children)
      child.Parent = node;

    // Edge case: Root
    bool changedRoot = false;
    if (data.selectedNode == Root)
      changedRoot = true;

    // Destroy old node
    data.selectedNode.Children.Clear();
    DeleteNode(data.selectedNode);

    // Root changing
    if (changedRoot)
      Root = node;
  }

  // obj needs to be of type BTNode
  public void DeleteNode(object obj)
  {
    // Remove node from parent's children
    BTNode node = (BTNode)obj;
    if(node.Parent != null)
      node.Parent.Children.Remove(node);

    // Remove children's references
    if(node.Children != null)
    {
      // Reassign parent if possible
      if (node.Parent != null)
        foreach (BTNode child in node.Children)
          child.Parent = node.Parent;

      // Orphan children otherwise
      else
        foreach (BTNode child in node.Children)
          child.Parent = null;
    }

    // Actually destroy the node
    // - Think I'm actually leaking the node
    MonoBehaviour.DestroyImmediate(node);
  }

  // obj must be an BTNode
  public void SetRoot(object obj)
  {
    BTNode root = (BTNode)obj;
    // Checks
    if (root.Parent != null)
    {
      Debug.Log("Only unparented nodes may become root");
      return;
    }

    Root = root;
  }

  // obj must be BTNode[2]
  // [0] = child node
  // [1] = parent node
  public void ChangeParent(object obj)
  {
    Debug.Log("Changing tree");
    BTNode[] nodes = (BTNode[]) obj;
    nodes[0].DisconnectFromParent();
    nodes[1].ConnectChild(nodes[0]);
  }

  // -------------------------------- Per frame Functions -------------------------------- //
  public virtual void Update()
  {
    BT_Status status = Root.Update();
    if (status == BT_Status.Success
    || status == BT_Status.Fail)
      Root.SetStatus(BT_Status.Entering);
  }
}

