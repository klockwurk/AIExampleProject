/*******************************************************************************/
/*!
\file   BT_Leaf.cs
\author Khan Sweetman
\par    All content © 2015 DigiPen (USA) Corporation, all rights reserved.
\par    Golden Bullet Games
\brief
  File does does things. COOL things.
  
*/
/*******************************************************************************/

using UnityEngine;

public class LEF_ChangeColor : BTNode
{
  public Color ToChangeTo = Color.magenta;

  public override void Initialize(object[] objs)
  {
    base.Initialize(objs);
  }

  // ---------------------------- Per Frame Functions ---------------------------- //
  override public BT_Status Update()
  {
    foreach(Renderer renderer in Owner.GetComponentsInChildren<Renderer>())
    {
      renderer.material.color = ToChangeTo;
    }
    return SetStatus(BT_Status.Success);
  }
}

