/*******************************************************************************/
/*!
\file   LEF_PlaySound.cs
\author Khan Sweetman
\par    All content © 2016-2017 DigiPen (USA) Corporation, all rights reserved.
\par    The Bakery
 
\brief
  ...
 
*/
/*******************************************************************************/

using UnityEngine;
using System.Collections;
using System;

public class LEF_PlaySound : BTNode
{
  // ------------------------------------------------- Variables -------------------------------------------------- //
  public string SoundPath;
  public AudioClip SoundClip;

  // ------------------------------------------------- Life Cycle -------------------------------------------------- //
  public override void Initialize(object[] objs)
  {
    base.Initialize(objs);
    SoundClip = (AudioClip)Resources.Load(SoundPath);
  }

  public override void EnterBehavior()
  {
    AudioSource.PlayClipAtPoint(SoundClip, transform.position);
    CurrStatus = BT_Status.Success;
  }

  public override BT_Status Update()
  {
    return CurrStatus;
  }
}
