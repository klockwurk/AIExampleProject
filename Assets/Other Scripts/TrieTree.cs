/*******************************************************************************/
/*!
\file   CommandPrompt.cs
\author Khan Sweetman
\par    All content © 2015 DigiPen (USA) Corporation, all rights reserved.
\par    The Bakery

\brief
  Trie Tree implementation.
  
*/
/*******************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Text;

public class TrieTree<T> where T : class, new()
{
  // ------------------------------------------------- Variables -------------------------------------------------- //
  public class TrieNode
  {
    public char Key; // identical to the key used to access it. currently redundant.
    public SortedList<char, TrieNode> Children;
    public T Value;
    public int Explored;

    public TrieNode()
    {
      Key = ' ';
      Children = new SortedList<char, TrieNode>();
      Value = null;
      Explored = 0;
    }

    public TrieNode(char key)
    {
      Key = key;
      Children = new SortedList<char, TrieNode>();
      Value = null;
      Explored = 0;
    }
  }
  public delegate void VisitDel(TrieNode node);
  public delegate void VisitDel2(TrieNode node, string fullKey);

  private TrieNode Root;
  private int ExploredVal;

  // ------------------------------------------------- Life Cycle -------------------------------------------------- //
  public TrieTree()
  {
    Root = new TrieNode();
    ExploredVal = 1;
  }

  // ------------------------------------------------- Primary Interface -------------------------------------------------- //
  public void Insert(string key, T value)
  {
    InsertRecursive(key, value, Root, 0);
  }

  private void InsertRecursive(string key, T value, TrieNode node, int index)
  {
    // end of string? base case, insert value, back out
    if (index == key.Length)
    {
      if (node.Value == null)
      {
        node.Value = new T();
      }
      node.Value = value;
      return;
    }

    // node doesn't have child path? create path
    if (!node.Children.ContainsKey(key[index]))
    {
      node.Children.Add(key[index], new TrieNode(key[index]));
    }

    // follow path
    InsertRecursive(key, value, node.Children[key[index]], index + 1);
  }

  public T Get(string key)
  {
    if (key.Length == 0)
      return null;
    return GetValueRecursive(key, Root, 0);
  }

  private T GetValueRecursive(string key, TrieNode node, int index)
  {
    // end of string? return the value
    if (index == key.Length)
    {
      return node.Value;
    }

    // node doesn't have child path? doesn't have value
    if (!node.Children.ContainsKey(key[index]))
    {
      return null;
    }

    // otherwise, follow the path
    return GetValueRecursive(key, node.Children[key[index]], index + 1);
  }

  // ------------------------------------------------- Debugging -------------------------------------------------- //
  public override string ToString()
  {
    StringBuilder builder = new StringBuilder();
    VisitDel2 del = (TrieNode node, string fullKey) =>
    {
      builder.AppendLine(fullKey + " : " + node.Value);
    };
    TraverseDepthFirst(del);
    return builder.ToString();
  }

  public List<TrieNode> GetAllFullNodes()
  {
    List<TrieNode> nodes = new List<TrieNode>();
    VisitDel2 del = (TrieNode node, string fullKey) =>
    {
      if (node.Value != null)
      {
        nodes.Add(node);
      }
    };
    TraverseDepthFirst(del);
    return nodes;
  }

  // ------------------------------------------------- Traversal -------------------------------------------------- //
  public void TraversePreOrder(VisitDel del)
  {
    PreOrderRecursive(del, Root);
  }

  private void PreOrderRecursive(VisitDel del, TrieNode node)
  {
    foreach (KeyValuePair<char, TrieNode> child in node.Children)
    {
      del(child.Value);
      PreOrderRecursive(del, child.Value);
    }
  }

  public void TraverseDepthFirst(VisitDel2 del)
  {
    // stack of nodes
    // start w/root
    Stack<TrieNode> stack = new Stack<TrieNode>();
    stack.Push(Root);
    string currentKey = "";

    // nodes left on stack?
    while (stack.Count != 0)
    {
      // top is unvisited?
      TrieNode top = stack.Peek();
      if (top.Explored != ExploredVal)
      {
        top.Explored = ExploredVal;
        currentKey += (top.Key);
        del(top, currentKey);
      }

      // top has unvisited children?
      bool hasUnvisited = false;
      foreach (KeyValuePair<char, TrieNode> child in top.Children)
      {
        if (child.Value.Explored != ExploredVal)
        {
          // put first unvisited child on stack
          hasUnvisited = true;
          stack.Push(child.Value);
          break;
        }
      }
      // all children have been explored? pop
      if (!hasUnvisited)
      {
        stack.Pop();
        currentKey = currentKey.Substring(0, currentKey.Length - 1);
      }
    }

    // update ExploredVal for subsequent searches
    ++ExploredVal;
  }

  public void TraverseDepthFirst(VisitDel del)
  {
    VisitDel2 del2 = (TrieNode node, string str) => { del(node); };
    TraverseDepthFirst(del2);
  }
}
