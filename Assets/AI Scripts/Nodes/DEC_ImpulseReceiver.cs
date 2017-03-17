/*******************************************************************************/
/*!
\file   LEF_ImpulseReceiver.cs
\author Khan Sweetman
\par    All content © 2015 DigiPen (USA) Corporation, all rights reserved.
\par    Golden Bullet Games
\brief
  
  
*/
/*******************************************************************************/

#define USING_NATE_EVENTS
//#define DEBUG_IMPULSE

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

[System.Serializable]
public class DEC_ImpulseReceiver : BTNode
{
  // ------------------------------------------------- Variables -------------------------------------------------- //
  public int TargetNodeID = 0;
  public string ObjectName;         // Object with event to respond to. Defaults to self.
  public string ImpulseToRespondTo; // Event name on component

  private BTNode Target;

  // ------------------------------------------------- Life Cycle -------------------------------------------------- //
  public override void Initialize(object[] objs)
  {
    base.Initialize(objs);

    // Listen to impulses
    GameObject obj = ObjectName == null ? Owner : GameObject.Find(ObjectName);
    obj.EventSubscribe(ImpulseToRespondTo, OnImpulse);

    // Get Target Node
    Target = BTree.FindNodeByID(TargetNodeID);
#if DEBUG_IMPULSE
    if (Target == null)
      Debug.LogError("Error: Impulse Receiver did not have a valid target node");
#endif
  }

  void OnImpulse()
  {
    JumpToTargetNode();
  }

  public override void EnterBehavior()
  {
    // Entrance behavior
    base.EnterBehavior();
  }

  public override BT_Status Update()
  {
    CurrStatus = Children[0].Update();
    return CurrStatus;
  }

  public override float Utility()
  {
    return 0.0f;
  }

  // ------------------------------------------------- Helpers -------------------------------------------------- //
  void JumpToTargetNode()
  {
    // fuck it, exit everything
    BTree.Root.ExitBehavior();

    // enter back to jump target
    HashSet<int> exploredNodes = new HashSet<int>();
    Stack<BTNode> stack = new Stack<BTNode>();
    stack.Push(BTree.Root);
    while (stack.Count != 0)
    {
      // not explored? explore
      BTNode top = stack.Peek();
      if (!exploredNodes.Contains(top.ID))
      {
        exploredNodes.Add(top.ID);
        // target node? we're done
        if (top.ID == TargetNodeID)
        {
          break;
        }
      }

      // has an unexplored child? add to stack
      bool hasUnexplored = false;
      foreach(BTNode child in top.Children)
      {
        if (!exploredNodes.Contains(child.ID))
        {
          stack.Push(child);
          hasUnexplored = true;
          break;
        }
      }
      // no unexplored? pop
      if (!hasUnexplored)
      {
        stack.Pop();
      }
    }

    // the stack is now the path to the target node
#if DEBUG_IMPULSE
    string str = "Path to target from root: ";
    foreach(BTNode node in stack)
    {
      str += node.Name + ", ";
    }
    Debug.Log(str);
#endif
    BTNode[] nodes = stack.ToArray();
    int index = nodes.Length - 1;
    while (index >= 1)
    {
      int indexOfChild = nodes[index].Children.IndexOf(nodes[index - 1]);
      nodes[index].EnterAtIndex(indexOfChild);
      --index;
    }
    nodes[0].EnterBehavior();
  }

  // ------------------------------------------------- UI Shit -------------------------------------------------- //
#if UNITY_EDITOR
  public override void DrawInspector()
  {
    base.DrawInspector();
  }
#endif
}
