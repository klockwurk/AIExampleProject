/*******************************************************************************/
/*!
\file   BTAsset.cs
\author Khan Sweetman
\par    All content © 2015 DigiPen (USA) Corporation, all rights reserved.
\par    Golden Bullet Games
\brief
  Basically a serialization wrapper around a BehaviorTree

  TODO:
   - Serializing/deserializing enums

*/
/*******************************************************************************/

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Xml;
using System.Reflection;
using System.Collections.Generic;

[System.Serializable]
public class BTAsset : ScriptableObject
{
  // ------------------------------------------------- Variables -------------------------------------------------- //
  [SerializeField]
  public BehaviorTree BTree;
  [SerializeField]
  public string SerializedBTree;
  // Deep copy variables. Cached in class for speed.
  Stack<BTNode> OGStack = new Stack<BTNode>();
  Stack<BTNode> CopyStack = new Stack<BTNode>();

  // ------------------------------------------------- Interface -------------------------------------------------- //
  public void GetTreeDeepCopy(ref BehaviorTree other)
  {
    if (BTree == null || BTree.Root == null)
    {
      Deserialize();
    }
    // set root of other
    other.Root = this.BTree.Root.GetShallowCopy();
    other.Root.Parent = null;
    other.Root.Children = new List<BTNode>(this.BTree.Root.Children.Count);

    // push root of this onto stack
    OGStack.Push(BTree.Root);
    CopyStack.Push(other.Root);

    //while nodes left on stack
    BTNode ogTop = null;
    BTNode copyTop = null;
    BTNode copyChild = null;
    while (OGStack.Count != 0)
    {
      // pop top
      ogTop = OGStack.Pop();
      copyTop = CopyStack.Pop();

      // add unexplored children to stack
      // explore children (shallow copy, set parent of all children to current node)
      for (int i = 0; i < ogTop.Children.Count; ++i)
      {
        // Make copies of children (same process as root, manual inlining)
        copyChild = ogTop.Children[i].GetShallowCopy();
        copyChild.Parent = copyTop;
        copyChild.Children = new List<BTNode>(ogTop.Children[i].Children.Count);
        copyTop.Children.Add(copyChild);

        OGStack.Push(ogTop.Children[i]);
        CopyStack.Push(copyTop.Children[i]);
      }
    }
  }

  // ------------------------------------------------- Life Cycle -------------------------------------------------- //
  public void OnEnable()
  {
    Deserialize();
  }

  // ---------------------------------- Serialization/Deserialization Functions ---------------------------------- //
  public BehaviorTree Deserialize()
  {
    // Deserialize the asset into memory the first time we ask for an instance of the BTree
    BTree = new BehaviorTree();
    if (SerializedBTree == null)
    {
      BTree.SetRoot(ScriptableObject.CreateInstance<SEL_Sequencer>());
    }
    else
    {
      XmlDocument doc = new XmlDocument();
      doc.LoadXml(SerializedBTree);
      XmlElement rootEl = (XmlElement)doc.GetElementsByTagName("Root").Item(0);
      BTree.SetRoot(DeserializeSubtree(rootEl, BTree));
    }

    // Return shallow copy
    return BTree;
  }

  BTNode DeserializeSubtree(XmlElement el, BehaviorTree btree)
  {
    BTNode node = null;

    // Create new node type based on stored data
    node = (BTNode)ScriptableObject.CreateInstance(System.Type.GetType(el.GetAttribute("Type")));
    node.BTree = BTree;

    // Parse type-specific data
    FieldInfo[] fields = node.GetType().GetFields();
    int length = fields.Length;
    for (int i = 0; i < length; ++i)
    {
      if (fields[i].IsNotSerialized)
        continue;

      string attribute = el.GetAttribute(fields[i].Name);
      if (attribute != "")
      {
        object value = null;
        try
        {
          value = Util.StringToValue(attribute, fields[i].FieldType);
        }
        catch (System.FormatException)
        {
          Debug.LogError("Input string was not in the correct format while deserializing attribute: " + attribute + " in BTAsset: " + name);
        }
        fields[i].SetValue(node, value);
      }
    }

    // Recurse
    foreach (XmlNode xmlNode in el.ChildNodes)
    {
      XmlElement childEl = xmlNode as XmlElement;
      if (childEl != null && childEl.Name != "param")
      {
        BTNode child = DeserializeSubtree(childEl, btree);
        node.ConnectChild(child);
      }
    }

    return node;
  }

  // Makes an XML document that we fill with what will someday sprout into a full-grown behavior tree
  public void Serialize()
  {
    XmlDocument doc = new XmlDocument();

    // Behavior tree
    XmlElement treeEl = doc.CreateElement(typeof(BehaviorTree).ToString());
    doc.AppendChild(treeEl);

    // Subtrees
    if (BTree == null)
    {
      BTree = new BehaviorTree();
    }
    SerializeSubtree(BTree.Root, treeEl);

    // Store XML
    SerializedBTree = doc.InnerXml;
  }

  void SerializeSubtree(BTNode node, XmlElement parentEl)
  {
    XmlDocument doc = parentEl.OwnerDocument;

    // Serialize name/root/type
    XmlElement el;
    if (node == BTree.Root)
      el = doc.CreateElement("Root"); // Root needs to have a special name. Don't name something "Root"
    else
    {
      //string name = node.Name;
      //name.Replace(' ', '_');
      el = doc.CreateElement(node.Name);
    }
    el.SetAttribute("Type", node.GetType().ToString());

    // Serialize generic node data
    FieldInfo[] fields = typeof(BTNode).GetFields();
    foreach (FieldInfo field in fields)
    {
      if (field.IsNotSerialized)
        continue;
      if (field.GetValue(node) == null)
        continue;

      el.SetAttribute(field.Name, field.GetValue(node).ToString());
    }

    // Serialize type-specific node data
    fields = node.GetType().GetFields();
    foreach (FieldInfo field in fields)
    {
      if (field.IsNotSerialized)
        continue;
      if (field.GetValue(node) == null)
        continue;

      el.SetAttribute(field.Name, field.GetValue(node).ToString());
    }

    // Attach child
    parentEl.AppendChild(el);

    // Recurse
    for (int i = 0; i < node.Children.Count; ++i)
    {
      SerializeSubtree(node.Children[i], el);
    }
  }
}



