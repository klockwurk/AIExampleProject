/*******************************************************************************/
/*!
\file   NodeRenderer.cs
\author Khan Sweetman
\par    All content © 2015 DigiPen (USA) Corporation, all rights reserved.
\par    Golden Bullet Games
\brief
  - NodeRenderer is owned by an instance of the View class
  - Used to render nodes
  - Nodes are fed into the Draw, and NodeRenderer draws them base on input
  
*/
/*******************************************************************************/
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class NodeRenderer
{
  // ------------------------------------------------- Variables -------------------------------------------------- //
  public static readonly Vector2 NODE_SIZE = new Vector2(100, 100);

  public static Vector2 NodeSize
  {
    get
    {
      return new Vector2(GridRenderer.Step.x * 8.0f, GridRenderer.Step.y * 8.0f);
    }
  }

  public static Vector2 BreakpointSize
  {
    get
    {
      return NodeSize / 8;
    }
  }

  public Texture2D NodeTexture;
  public Texture2D NodeDebugTexture;
  public Texture2D NodeRunningTexture;
  public Texture2D ShadowTexture;
  public Texture2D RootTexture;
  public Texture2D BreakpointTexture;

  private Texture2D SelectionTexture;

  // ------------------------------------------------- Life Cycle -------------------------------------------------- //
  public NodeRenderer()
  {
    // Make node textures
    GenerateTextures();
  }

  // ------------------------------------------------- Primary Interface -------------------------------------------------- //
  public void Draw (BTNode node)
  {
    Rect rect = new Rect(node.EditorPosition + node.EditorOffset, NodeSize);

    // Draw the node itself
    Texture texture = null;
    if (node.Running && Application.isPlaying)
    {
      texture = NodeRunningTexture;
    }
    else
    {
      texture = node == BTEditorManager.Manager.SelectedNode ? SelectionTexture : NodeTexture;
    }
    GUI.DrawTexture(rect, texture);

    // Draw connections
    DrawEdge(node, node.Parent);

    // Root label
    if (node == BTEditorManager.Manager.Tree.Root)
    {
      GUIContent rootContent = new GUIContent("Root");
      Vector2 textSize = GUI.skin.label.CalcSize(rootContent);
      Rect rootRect = new Rect(rect.x + (rect.size.x - textSize.x) / 2, rect.y + (rect.size.y - textSize.y) / 2, textSize.x, textSize.y);
      GUI.Label(rootRect, rootContent);
    }

    // Breakpoint?
    if (node.HasBreakpoint)
    {
      Vector2 breakPos = rect.position;
      breakPos.x += NodeSize.x - BreakpointSize.x / 2;
      Vector2 size = rect.size / 4;
      GUI.DrawTexture(new Rect(breakPos, size), BreakpointTexture);
    }

    // Draw title
    DrawTitle(node, rect);
  }

  // ----------------------------- Drawing Functions ----------------------------- //
  void DrawEdge(BTNode child, BTNode parent)
  {
    if (parent == null)
      return;

    float px = (parent.EditorPosition.x + parent.EditorOffset.x) + NodeSize.x / 2.0f;
    float py = (parent.EditorPosition.y + parent.EditorOffset.y) + NodeSize.y;
    float cx = (child.EditorPosition.x + child.EditorOffset.x) + NodeSize.x / 2.0f;
    float cy = (child.EditorPosition.y + child.EditorOffset.y);
    Vector3 startPos = new Vector3(px, py);
    Vector3 endPos = new Vector3(cx, cy);
    Vector3 startTan = startPos + Vector3.up * GridRenderer.Step.x * 2;
    Vector3 endTan = endPos + Vector3.down * GridRenderer.Step.x * 2;

    Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.white, null, 4);
  }

  void DrawSelected(Rect rect)
  {
    GUI.DrawTexture(rect, SelectionTexture);
  }

  void DrawTitle(BTNode node, Rect nodeRect)
  {
    // Draw title
    string title = node.Name;

    // Strip out prefix
    int index = title.IndexOf("_");
    if (index != -1)
    {
      title = title.Substring(index + 1);
    }

    // Calculate size
    Vector2 textSize = GUI.skin.label.CalcSize(new GUIContent(title));
    Rect titleRect = new Rect(nodeRect.x + (nodeRect.size.x / 2) - ((textSize.x + 10) / 2.0f), nodeRect.y, textSize.x + 10, textSize.y);

    // Actually draw title
    EditorGUI.DropShadowLabel(titleRect, new GUIContent(title));
  }

  // ----------------------------- Texture Generation Functions ----------------------------- //
  public void GenerateTextures()
  {
    // Node texture
    NodeTexture = new Texture2D(1, 1);
    NodeTexture.SetPixel(0, 0, Color.grey);
    NodeTexture.Apply();

    // Editor Selection texture
    SelectionTexture = new Texture2D(1, 1);
    SelectionTexture.SetPixel(0, 0, Color.yellow);
    SelectionTexture.Apply();

    // Run-time Running Texture
    NodeRunningTexture = new Texture2D(1, 1);
    NodeRunningTexture.SetPixel(0, 0, new Color(1.0f, 0.6f, 0.0f));
    NodeRunningTexture.Apply();

    // Breakpoint texture
    BreakpointTexture = new Texture2D((int)BreakpointSize.x, (int)BreakpointSize.y);
    int length = (int)BreakpointSize.x;
    Vector2 midPoint = new Vector2(length / 2.0f, length / 2.0f);
    for (int x = 0; x < length; ++x)
    {
      for (int y = 0; y < length; ++y)
      {
        Color color = Vector2.Distance(midPoint, new Vector2(x, y)) < length / 2.0f ? Color.red : Color.clear;
        BreakpointTexture.SetPixel(x, y, color);
      }
    }
    BreakpointTexture.Apply();
  }

  // ----------------------------- Helper Functions ----------------------------- //
  // Returns a node's rectangle in the editor
  public Rect GetRect(BTNode node, Vector2 offset)
  {
    return new Rect(node.EditorPosition.x - offset.x, node.EditorPosition.y - offset.y, NodeSize.x, NodeSize.y);
  }
}
