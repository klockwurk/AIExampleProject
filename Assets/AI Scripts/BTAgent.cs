/*******************************************************************************/
/*!
\file   BTAgent.cs
\author Khan Sweetman
\par    All content © 2015 DigiPen (USA) Corporation, all rights reserved.
\par    Golden Bullet Games
\brief
  - Basically a container for a BehaviorTreeAsset
  - Needs to be separate from BehaviorTreeAsset..
    - ...so that we only make one custom editor for the BehaviorTreeAsset class,
      as opposed to a custom editor for every single Behavior Tree script
  
*/
/*******************************************************************************/
//#define DEBUG_AGENT
using UnityEngine;
using System.Collections;

[AddComponentMenu("Miscellaneous/BTAgent")]
public class BTAgent : MonoBehaviour
{
  // ------------------------------------------------- Variables -------------------------------------------------- //
  public BTAsset Asset = null;
  [System.NonSerialized] public BehaviorTree Tree = null;

  // ------------------------------------------------- Life Cycle -------------------------------------------------- //
  public void Awake()
  {
#if !DEBUG_AGENT
    Tree = new BehaviorTree();
    if (Asset != null)
    {
      Asset.GetTreeDeepCopy(ref Tree);
      Tree.Agent = this;
    }
#else
    if (Asset != null)
    {
      Tree = new BehaviorTree();
      Asset.GetTreeDeepCopy(ref Tree);
      Tree.Agent = this;
    }
#endif
  }

  void Start()
  {
#if !DEBUG_AGENT
    Tree.Initialize(gameObject);
#else
    // Initialize tree
    if (Tree != null)
      Tree.Initialize(gameObject);
    else
      Debug.LogError("BTAsset not set in " + gameObject.name);
#endif
  }

  public void Update()
  {
    // Just run the BehaviorTree stored
    Tree.Update();
  }
}
