/*******************************************************************************/
/*!
\file   GridRenderer.cs
\author Khan Sweetman
\par    All content © 2015 DigiPen (USA) Corporation, all rights reserved.
\par    Golden Bullet Games
\brief
  GridRenderer is used to draw the background of BTEditorWindow.
  
*/
/*******************************************************************************/
using UnityEngine;
using System.Collections;
using UnityEditor;

public class GridRenderer 
{
  Texture2D GridTex
  {
    get
    {
      if (GridTexData == null)
      {
        GenerateTileTexture();
      }
      return GridTexData;
    }
    set { GridTexData = value; }
  }
  Texture2D GridTexData;

  private static float TileWidthData = 120.0f;
  private static float TileHeightData = 120.0f;
  public static float TileWidth { get { return TileWidthData; } set { TileWidthData = value; }  }  // Cell width
  public static float TileHeight { get { return TileHeightData; } set { TileHeightData = value; }  } // Cell height
  public static Vector2 Step { get { return new Vector2(TileWidth / 10, TileHeight / 10); } } // Subcell dimensions
  private static Vector2 OffsetData = Vector2.zero;
  public static Vector2 Offset
  {
    get { return OffsetData; }
    set
    {
      OffsetData.x = (int) value.x % TileWidth - (value.x - (int) value.x) - TileWidth;
      OffsetData.y = (int) value.y % TileWidth - (value.y - (int) value.y) - TileHeight;
    }
  }
  
  public void OnEnable()
  {
    GenerateTileTexture();
  }

  public void Draw(Vector2 scrollPoint, Rect canvas)
  {
    float yOffset = scrollPoint.y % TileHeight;
    float yStart = scrollPoint.y - yOffset + Offset.y;
    float yEnd = scrollPoint.y + canvas.height + yOffset;

    float xOffset = scrollPoint.x % TileWidth;
    float xStart = scrollPoint.x - xOffset + Offset.x;
    float xEnd = scrollPoint.x + canvas.width + xOffset;

    // Fill canvas with grid texture
    for (float x = xStart; x < xEnd; x += TileWidth)
      for (float y = yStart; y < yEnd; y += TileHeight)
        GUI.DrawTexture(new Rect(x, y, TileWidth, TileHeight), GridTex);
  }

  // ----------------------------- Helper Functions ----------------------------- //
  // Generates a single cell of the full grid
  void GenerateTileTexture()
  {
    // Make new grid texture
    GridTex = new Texture2D((int) TileWidth,(int) TileHeight);
    GridTex.hideFlags = HideFlags.DontSave;

    // ---- COLOR ----
    Color bg = new Color(0.64f, 0.64f, 0.64f);
    // Make dark color based on bg
    Color dark = Color.Lerp(bg, Color.black, 0.15f);
    Color darkIntersection = Color.Lerp(bg, Color.black, 0.2f);
    // Make light color based on bg
    Color light = Color.Lerp(bg, Color.black, 0.05f);
    Color lightIntersection = Color.Lerp(bg, Color.black, 0.1f);

    // Draw subcells w/colors
    for (int x = 0; x < TileWidth; ++x)
    {
      for (int y = 0; y < TileHeight; ++y)
      {
        // Left top corner, dark intersection
        if (x == 0 || y == 0)
          GridTex.SetPixel(x, y, darkIntersection);

        // Left and top edges, dark color
        else if (x == 0 || y == 0)
          GridTex.SetPixel(x, y, dark);

        // Subtile grid intersection color
        else if (x % Step.x == 0 && y % Step.y == 0)
          GridTex.SetPixel(x, y, lightIntersection);

        // Subtile grid color
        else if (x % Step.x == 0 || y % Step.y == 0)
          GridTex.SetPixel(x, y, light);

        // Default to bg color
        else
          GridTex.SetPixel(x, y, bg);
      }
    }

    // Actually apply all the SetPixel changes
    GridTex.Apply();
  }
}
