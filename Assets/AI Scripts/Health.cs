/*******************************************************************************/
/*!
\file   EnemyHealth.cs
\author Khan Sweetman
\par    All content © 2016-2017 DigiPen (USA) Corporation, all rights reserved.
\par    The Bakery
\brief
  Basic healt logic.
 
*/
/*******************************************************************************/
//#define DEBUG_HEALTH

using UnityEngine;
using UnityEngine.Networking;

public class DamageInfo
{
  public float Damage;
  public GameObject Instigator; // player that fired the bullet
  public Vector3 VelocityAtImpact;
  public Vector3 PointOfImpact;

  public DamageInfo()
  {
    Damage = 1.0f;
    Instigator = null;
    VelocityAtImpact = Immutables.VecZero;
    PointOfImpact = Immutables.VecZero;
  }

  public DamageInfo(float damage)
  {
    Damage = damage;
    Instigator = null;
    VelocityAtImpact = Immutables.VecZero;
    PointOfImpact = Immutables.VecZero;
  }

  public DamageInfo(float damage, GameObject instigator)
  {
    Damage = damage;
    Instigator = instigator;
    VelocityAtImpact = Immutables.VecZero;
    PointOfImpact = Immutables.VecZero;
  }

  public DamageInfo(float damage, GameObject instigator, Vector3 velocity)
  {
    Damage = damage;
    Instigator = instigator;
    VelocityAtImpact = velocity;
    PointOfImpact = Immutables.VecZero;
  }

  public DamageInfo(float damage, GameObject instigator, Vector3 velocity, Vector3 pointOfImpact)
  {
    Damage = damage;
    Instigator = instigator;
    VelocityAtImpact = velocity;
    PointOfImpact = pointOfImpact;
  }
}

public class EnemyHealth : MonoBehaviour
{
  // ------------------------------------------------- Variables -------------------------------------------------- //
  public float MaxHealth = 3;
  [System.NonSerialized] public float CurrentHealth;

  public GameObject KilledBy { get; private set; }
  public GameObject LastPointOfImpact { get; private set; }

  // ------------------------------------------------- Life Cycle -------------------------------------------------- //
  public void Awake()
  {
    CurrentHealth = MaxHealth;
  }

  void Start()
  {
    // Store Components in Variables
    gameObject.EventSubscribe<DamageInfo>("Damage", OnDamage);
  }

  // ------------------------------------------------- Death/Damage -------------------------------------------------- //
  public void OnDamage(DamageInfo info)
  {
    Damage(info);
  }

  private void Damage(DamageInfo info)
  {
    if (CurrentHealth > 0)
    {
      CurrentHealth -= info.Damage;

      if (CurrentHealth <= 0)
      {
        CurrentHealth = 0.0f;
        KilledBy = info.Instigator;
        gameObject.EventSend("Death", info.Instigator);
      }
    }
  }
}
