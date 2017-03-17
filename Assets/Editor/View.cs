/*******************************************************************************/
/*!
\file   View.cs
\author Khan Sweetman
\par    All content © 2015 DigiPen (USA) Corporation, all rights reserved.
\par    Golden Bullet Games
\brief
  Helper class used to render BTEditorWindow.
  
*/
/*******************************************************************************/
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class View
{
  // ------------------------------------------------- Variables -------------------------------------------------- //
  public GridRenderer GRenderer = new GridRenderer();
  public NodeRenderer NRenderer;
  public Rect Canvas;
  public Vector2 ScrollPoint = Vector2.zero;
  public BTEditorWindow BTEditorWindow;

  // ------------------------------------------------- Life Cycle -------------------------------------------------- //
  public View(BTEditorWindow owner)
  {
    BTEditorWindow = owner;
    Canvas = new Rect(0, 0, BTEditorWindow.position.width, BTEditorWindow.position.height);
    NRenderer = new NodeRenderer();
  }

  public void OnEnable()
  {
    Canvas = new Rect(0, 0, BTEditorWindow.position.width, BTEditorWindow.position.height);
    NRenderer = new NodeRenderer();
  }

  // ------------------------------------------------- Primary Interface -------------------------------------------------- //
  public bool DrawActiveTree()
  {
    GUI.BeginGroup(Canvas);
    ScrollPoint = Canvas.position;

    // Refresh assets?
    if (NRenderer.NodeTexture == null)
    {
      NRenderer.GenerateTextures();
    }

    // Draw run-time status of node
    if (BTEditorManager.Manager.Tree != null)
    {
      DrawRecursive(BTEditorManager.Manager.Tree.Root);
    }

    GUI.EndGroup();
    return true;
  }

  public void DrawBackground()
  {
    // Draw background
    GRenderer.Draw(ScrollPoint, Canvas);
  }

  public void ResizeCanvas()
  {
    // Make new canvas with update dimensions
    Rect newCanvas = new Rect(0, 0, BTEditorWindow.position.width, BTEditorWindow.position.height);
    Canvas = newCanvas;
  }

  // ------------------------------------------------- Helpers -------------------------------------------------- //
  private void DrawRecursive(BTNode node)
  {
    NRenderer.Draw(node);
    foreach (BTNode child in node.Children)
    {
      DrawRecursive(child);
    }
  }
}
