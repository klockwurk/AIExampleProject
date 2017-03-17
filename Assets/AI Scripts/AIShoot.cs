/*******************************************************************************/
/*!
\file   AIShoot.cs
\author Khan Sweetman
\par    All content © 2017 DigiPen (USA) Corporation, all rights reserved.
\par    The Bakery

\brief
  ...
  
*/
/*******************************************************************************/
//#define DEBUG_ACCURACY
//#define USING_GAUSS

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AIShoot : NetworkBehaviour
{
  // ------------------------------------------------- Variables -------------------------------------------------- //
  public GameObject ProjectilePrefab;
  public Transform Muzzle;
  public float ProjectileSpeed;

  //public void Awake()
  //{
  //  Debug.Log("Shoot awakened");
  //}

  //public void OnEnable()
  //{
  //  Debug.Log("Shoot OnEnable: " + GetComponent<NetworkIdentity>().isActiveAndEnabled);
  //}

  //public void Start()
  //{
  //  Debug.Log("Shoot Start: " + GetComponent<NetworkIdentity>().isActiveAndEnabled);
  //}

  //public void OnNetworkInstantiate(NetworkMessageInfo info)
  //{
  //  Debug.Log("Network instantiation from: " + info.sender);
  //}

  // ------------------------------------------------- Primary Interface -------------------------------------------------- //
  [Command]
  public void CmdShoot(Vector3 target, int numShots, float destroyAfterTime, float spread)
  {
    // Spawn bullets
    Vector3 spawnPos = Muzzle.position;
    for (int i = 0; i < numShots; ++i)
    {
      // calculate offset angle to fire
      Vector3 pointOnCircle = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)).normalized;
      pointOnCircle = Muzzle.TransformPoint(pointOnCircle);
      float angleMagnitude = Util.GaussRand(0.0f, spread) * Mathf.PI / 180.0f;
      Vector3 dir = Vector3.RotateTowards(Muzzle.transform.forward, pointOnCircle, angleMagnitude, 0.0f);
      Quaternion quat = Quaternion.LookRotation(dir);

#if DEBUG_ACCURACY
      Debug.DrawLine(Muzzle.transform.position, pointOnCircle, Color.grey, 3.0f); // circle
      Debug.DrawRay(Muzzle.transform.position, Muzzle.transform.forward * 3.0f, Color.grey, 3.0f); // forward
      Debug.DrawRay(Muzzle.transform.position, dir * 3.0f, Color.green, 3.0f); // shot direction
      Debug.DrawLine(Muzzle.transform.position, target, Color.white, 3.0f); // to target
#endif
      // spawn bullet locally
      var bullet = (GameObject)Instantiate(ProjectilePrefab, spawnPos, quat);
      // set speed
      bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * ProjectileSpeed;
      // spawn bullet on clients
      NetworkServer.Spawn(bullet);

      // Destroy the bullet after time
      if (destroyAfterTime > 0)
      {
        Invoke("CmdDestroyThis", destroyAfterTime);
      }
    }
  }

  [Command]
  public void CmdArcShoot(Vector3 target, int numShots, float destroyAfterTime, float spread, float peakHeight, float peakRandom)
  {
    // Spawn bullets
    Vector3 spawnPos = Muzzle.position;
    for (int i = 0; i < numShots; ++i)
    {
      // calculate offset point on ground
#if USING_GAUSS
      Vector2 pointOnCircle2d = Random.insideUnitCircle.normalized * Util.GaussRand(0.0f, spread);
#else
      Vector2 pointOnCircle2d = Random.insideUnitCircle.normalized * Random.Range(0.0f, spread);
#endif
      Vector3 pointOnCircle = new Vector3(pointOnCircle2d.x, 0.0f, pointOnCircle2d.y);

      peakHeight += Random.Range(-peakRandom, peakRandom);
#if DEBUG_ACCURACY
      Vector3 dir = Util.CalculateJumpVel(spawnPos, target + pointOnCircle, peakHeight, 0.0f, true);
      Util.DrawCircle(target + pointOnCircle, Color.red, 1.0f, 5.0f, 5);
#else
      Vector3 dir = Util.CalculateJumpVel(spawnPos, target + pointOnCircle, peakHeight);
#endif
      Quaternion quat = Quaternion.LookRotation(dir);

      // spawn bullet
      var bullet = (GameObject)Instantiate(ProjectilePrefab, spawnPos, quat);
      bullet.GetComponent<Rigidbody>().velocity = dir;
      NetworkServer.Spawn(bullet);

      // Destroy the bullet after time
      if (destroyAfterTime > 0)
      {
        Invoke("CmdDestroyThis", destroyAfterTime);
      }
    }
  }
}
