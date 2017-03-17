/*******************************************************************************/
/*!
\file   AggroFX.cs
\author Khan Sweetman
\par    All content © 2016-2017 DigiPen (USA) Corporation, all rights reserved.
\par    The Bakery

\brief
  ...
 
*/
/*******************************************************************************/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Aggro))]
public class AggroFX : MonoBehaviour
{
  // ------------------------------------------------- Variables -------------------------------------------------- //
  public Color TargetColor = Color.red;
  public AnimationCurve EyeEmissionCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
  public List<Renderer> RenderersToLight = new List<Renderer>();

  private Aggro AggroComponent;
  private List<Color> OGColors = new List<Color>();

  // ------------------------------------------------- Life Cycle -------------------------------------------------- //
  void Start()
  {
    AggroComponent = GetComponent<Aggro>();
    gameObject.EventSubscribe<GameObject>("AggroTriggered", OnAggroTriggered);
    gameObject.EventSubscribe<GameObject>("AggroLost", OnAggroLost);
    for (int i = 0; i < RenderersToLight.Count; ++i)
    {
      OGColors.Add(RenderersToLight[i].material.GetColor("_EmissionColor"));
    }
  }

  private void OnAggroTriggered(GameObject target)
  {
    LightUpEyes();
  }

  private void OnAggroLost(GameObject target)
  {
    LightDownEyes();
  }

  // ------------------------------------------------- VFX -------------------------------------------------- //
  public void LightUpEyes()
  {
    StartCoroutine(LightUpEyesCoroutine());
  }

  private IEnumerator LightUpEyesCoroutine()
  {
    float timer = 0.0f;
    while (timer < EyeEmissionCurve.TotalTime())
    {
      timer += Time.deltaTime;
      for (int i = 0; i < RenderersToLight.Count; ++i)
      {
        RenderersToLight[i].material.SetColor("_EmissionColor", OGColors[i] + (TargetColor - OGColors[i]) * EyeEmissionCurve.Evaluate(timer));
      }
      yield return null;
    }
  }

  public void LightDownEyes()
  {
    StartCoroutine(LightDownEyesCoroutine());
  }

  private IEnumerator LightDownEyesCoroutine()
  {
    float timer = EyeEmissionCurve.TotalTime();
    while (timer > 0.0f)
    {
      timer -= Time.deltaTime;
      for (int i = 0; i < RenderersToLight.Count; ++i)
      {
        RenderersToLight[i].material.SetColor("_EmissionColor", OGColors[i] + (TargetColor - OGColors[i]) * EyeEmissionCurve.Evaluate(timer));
      }
      yield return null;
    }
  }
}
