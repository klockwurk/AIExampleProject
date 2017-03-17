/*******************************************************************************/
/*!
\file   Util.cs
\author Khan Sweetman
\par    All content © 2015 DigiPen (USA) Corporation, all rights reserved.
\par    Golden Bullet Games
\brief
  File does does things. COOL things.
  
*/
/*******************************************************************************/

#define DEBUG_JUMP

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class Immutables
{
  public static readonly Vector3 VecZero = new Vector3(0, 0, 0);
}


public static class Extensions
{
  // ------------------------------------------------- AnimationCurve Extensions -------------------------------------------------- //
  public static float TotalTime(this AnimationCurve curve)
  {
    return curve.keys[curve.length - 1].time;
  }

  public static T RandomElement<T>(this ICollection<T> e)
  {
    int count = e.Count;
    if (count == 0)
    {
      throw new System.ArgumentOutOfRangeException();
    }
    return e.ElementAt(Random.Range(0, e.Count));
  }

  // ------------------------------------------------- Animator Extensions -------------------------------------------------- //
  public static bool HasParameter(this Animator animator, string parameter)
  {
    for (int i = 0; i < animator.parameters.Length; ++i)
    {
      if (animator.parameters[i].name == parameter)
      {
        return true;
      }
    }
    return false;
  }

  public static int GetParameterHash(this Animator animator, string parameter)
  {
    int hash = -1;
    for (int i = 0; i < animator.parameters.Length; ++i)
    {
      if (animator.parameters[i].name == parameter)
      {
        hash = animator.parameters[i].nameHash;
        if (hash == -1) { throw new System.Exception("The anim has was -1, and I don't know how to feel about that"); }
        break;
      }
    }
    return hash;
  }
}

public class Util : MonoBehaviour
{
  // ------------------------------------------------- Randomization Functions -------------------------------------------------- //
  public static float GaussRand(float center, float edgeDist)
  {
    float total = Random.Range(center - edgeDist, center + edgeDist)
                + Random.Range(center - edgeDist, center + edgeDist)
                + Random.Range(center - edgeDist, center + edgeDist);
    total /= 3.0f;
    return total;
  }

  // ------------------------------------------------- Debug Drawing Functions -------------------------------------------------- //
  public static void DrawCircle(Vector3 center, Color color, float radius, float duration = 0.0f, int numPoints = 8, bool depthTest = false)
  {
    float increment = Mathf.PI * 2.0f / (float)numPoints;
    for (float x = 0; x <= numPoints; x += increment)
    {
      Vector3 point = center + new Vector3(Mathf.Cos(x), 0.0f, Mathf.Sin(x)) * radius;
      Vector3 point2 = center + new Vector3(Mathf.Cos(x + increment), 0.0f, Mathf.Sin(x + increment)) * radius;
      Debug.DrawLine(point, point2, color, duration, depthTest);
    }
  }

  public static void DrawFullPath(UnityEngine.AI.NavMeshPath path, Color color, float duration = 0.0f)
  {
    Vector3 prevPoint = path.corners[0];
    for (int i = 0; i < path.corners.Length; ++i)
    {
      Debug.DrawLine(prevPoint, path.corners[i], color, duration);
      prevPoint = path.corners[i];
    }
    Util.DrawSphere(path.corners[path.corners.Length - 1], color, 0.25f, duration);
  }

  public static void DrawSphere(Vector3 center, Color color, float radius, float duration)
  {
    GameObject obj = new GameObject();
    DebugDrawer drawer = obj.AddComponent<DebugDrawer>();
    drawer.Center = center;
    drawer.Radius = radius;
    drawer.Lifetime = duration;
    drawer.Color = color;
  }

  public static void DrawColumn(Vector3 center, Color color, float radius, int layers = 3, float duration = 0.0f, int numPoints = 8)
  {
    for(int i = 0; i < layers; ++i)
    {
      DrawCircle(center + Vector3.up * i, color, radius, duration, numPoints);
    }
  }

  // ------------------------------------------ Parsing Functions ------------------------------------------ //
  public static object StringToValue(string value, System.Type type)
  {
    if (type == typeof(string)) return (string)value;
    else if (type == typeof(float)) return float.Parse(value);
    else if (type == typeof(int)) return int.Parse(value);
    else if (type == typeof(bool)) return bool.Parse(value);
    else if (type == typeof(Color)) return Util.StringToColor(value);
    else if (type == typeof(Vector2)) return Util.StringToVector2(value);
    else if (type == typeof(Vector3)) return Util.StringToVector3(value);
    else if (type == typeof(GameObject)) return Util.StringToGameObject(value);
    else if (type.IsEnum) return System.Enum.Parse(type, value);

    // Shouldn't happen
    Debug.LogError("ERROR: Tried to parse invalid type: " + type);
    return null;
  }

  public static GameObject StringToGameObject(string str)
  {
    return (GameObject)Resources.Load(str);
  }

  // str should be in format: RGBA(1.000, 0.000, 1.000, 1.000)
  // Color.ToString() does this format
  public static Color StringToColor(string str)
  {
    Color color = new Color();

    // red
    int front = str.IndexOf("(") + 1;
    int back = str.IndexOf(",", front);
    string colorString = str.Substring(front, back - front);
    color.r = float.Parse(colorString);
    // green
    front = back + 2;
    back = str.IndexOf(",", front);
    colorString = str.Substring(front, back - front);
    color.g = float.Parse(colorString);
    // blue
    front = back + 2;
    back = str.IndexOf(",", front);
    colorString = str.Substring(front, back - front);
    color.b = float.Parse(colorString);
    // alpha
    front = back + 2;
    back = str.IndexOf(")", front);
    colorString = str.Substring(front, back - front);
    color.a = float.Parse(colorString);

    return color;
  }

  // str should be in format: (0.0, 0.0)
  public static Vector2 StringToVector2(string str)
  {
    Vector2 vec2 = new Vector2();

    // x
    int front = str.IndexOf("(") + 1;
    int back = str.IndexOf(",", front);
    string valString = str.Substring(front, back - front);
    vec2.x = float.Parse(valString);
    // y
    front = back + 2;
    back = str.IndexOf(")", front);
    valString = str.Substring(front, back - front);
    vec2.y = float.Parse(valString);

    return vec2;
  }

  // str should be in format: (0.0, 0.0)
  public static Vector3 StringToVector3(string str)
  {
    Vector3 vec3 = new Vector3();

    // x
    int front = str.IndexOf("(") + 1;
    int back = str.IndexOf(",", front);
    string valString = str.Substring(front, back - front);
    vec3.x = float.Parse(valString);
    // y
    front = back + 2;
    back = str.IndexOf(",", front);
    valString = str.Substring(front, back - front);
    vec3.y = float.Parse(valString);
    // z
    front = back + 2;
    back = str.IndexOf(")", front);
    valString = str.Substring(front, back - front);
    vec3.y = float.Parse(valString);

    return vec3;
  }

  // -------------------------------------------- Relationship Functions --------------------------------------------- //
  public static GameObject FindClosestWithTag(string tag, Vector3 to, GameObject excluding = null)
  {
    GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
    GameObject closest = null;
    float dist = float.MaxValue;
    foreach (GameObject obj in objects)
    {
      // Don't check against given object
      if (excluding == obj)
        continue;

      if (Vector3.SqrMagnitude(obj.transform.position - to) < dist)
      {
        closest = obj;
        dist = Vector3.SqrMagnitude(obj.transform.position - to);
      }
    }

    return closest;
  }

  public static void GetChildClassesOf(System.Type parentType, ref List<System.Type> childTypes)
  {
    foreach (System.Type type in System.Reflection.Assembly.GetAssembly(typeof(BTNode)).GetTypes())
      if (type.IsSubclassOf(typeof(BTNode)))
        childTypes.Add(type);
  }

  public static bool IsRelatedTo(Collider col, Transform me)
  {
    if (col == null || me == null)
      return false;
    if (col.transform == me)
      return true;

    // check if roots are the same
    // NOTE:
    // -will not work if objects are in a "folder" object
    // -Possible fix:
    //    -Make a folder script that deparents child objects at initialization
    return me.root == col.transform.root;

    // check children
    // note: don't check from root in case we're parented to a "folder" object
    //foreach (Collider child in me.GetComponentsInChildren<Collider>(true))
    //  if (child == col)
    //    return true;
    //foreach (Transform t in col.transform.GetComponentsInChildren<Transform>(true))
    //  if (t == me)
    //    return true;
    //return false;
  }

  // ------------------------------------------------- Vector Math -------------------------------------------------- //
  public static float DistSqr(Vector3 lhs, Vector3 rhs)
  {
    return (lhs.x - rhs.x) * (lhs.x - rhs.x)
         + (lhs.y - rhs.y) * (lhs.y - rhs.y) 
         + (lhs.z - rhs.z) * (lhs.z - rhs.z);
  }

  // ------------------------------------------------- Physics Shit -------------------------------------------------- //
  public static Vector3 CalculateJumpVel(Vector3 start, Vector3 end, float peakHeight, float drag = 0.0f, bool debug = false)
  {
    // calculate y-velocity we have to reach to stop at the peak height
    float d = Mathf.Abs(peakHeight - start.y);
    float grav = -Physics.gravity.magnitude;
    float yVel = Mathf.Sqrt(Mathf.Abs(-2 * grav * d));

    // calculate time it takes for us to hit the ground after jumping
    float d2 = Mathf.Abs(peakHeight - end.y);
    float time = Mathf.Sqrt(Mathf.Abs((2 * d) / grav)) + Mathf.Sqrt(Mathf.Abs((2 * d2) / grav));

    // calculate ground velocity we need to have to stop at the target location
    Vector2 xzVec = new Vector2(end.x - start.x, end.z - start.z);
    float d3 = xzVec.magnitude;
    float xzVel = (d3 - 0.5f * drag * time * time) / time;

    // finalize vector
    xzVec = xzVec.normalized * xzVel;
    Vector3 finalVec = new Vector3(xzVec.x, yVel, xzVec.y);

#if DEBUG_JUMP
    if (debug)
    {
      Vector3 prevPoint = start;
      for (float currTime = 0.0f; currTime < time; currTime += 0.1f)
      {
        float dx = finalVec.x * currTime + 0.5f * drag * (currTime * currTime);
        float dy = finalVec.y * currTime + 0.5f * grav * (currTime * currTime);
        float dz = finalVec.z * currTime + 0.5f * drag * (currTime * currTime);
        Vector3 currPoint = new Vector3(start.x + dx, start.y + dy, start.z + dz);
        Debug.DrawLine(prevPoint, currPoint, Color.white, 1.0f);
        prevPoint = currPoint;
      }
    }
#endif

    return finalVec;
  }

  // ------------------------------------------------- Misc -------------------------------------------------- //
  public static void Swap<T>(ref T a, ref T b)
  {
    T temp = a;
    a = b;
    b = temp;
  }
}

// ------------------------------------------------- Helper Classes -------------------------------------------------- //
public class DebugDrawer : MonoBehaviour
{
  public Vector3 Center;
  public float Radius;
  public float Lifetime;
  public Color Color;

  public void Start()
  {
    StartCoroutine(DestroySelf());
  }

  IEnumerator DestroySelf()
  {
    yield return new WaitForSeconds(Lifetime);
    Destroy(this);
  }

  void OnDrawGizmos()
  {
    Gizmos.color = Color;
    Gizmos.DrawWireSphere(Center, Radius);
  }
}
