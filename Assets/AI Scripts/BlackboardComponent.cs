/*******************************************************************************/
/*!
\file   Blackboard.cs
\author Khan Sweetman
\par    All content © 2016-2017 DigiPen (USA) Corporation, all rights reserved.
\par    The Bakery
 
\brief
  ...
 
*/
/*******************************************************************************/
#define DEBUG_BB

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public struct BBEntry
{
  public static readonly BBEntry DEFAULT = new BBEntry();

  public string Key;
  public object Value;
  public System.Type ValueType;
  public string Description;

  public BBEntry(string key, object value, string description)
  {
    Key = key;
    Value = value;
    ValueType = typeof(Transform);
    Description = description;
  }

  public static bool operator==(BBEntry lhs, BBEntry rhs)
  {
    return String.Equals(lhs.Key, rhs.Key, StringComparison.Ordinal);
  }

  public static bool operator !=(BBEntry lhs, BBEntry rhs)
  {
    return !String.Equals(lhs.Key, rhs.Key, StringComparison.Ordinal);
  }

  public override bool Equals(object obj)
  {
    return base.Equals(obj);
  }

  public override int GetHashCode()
  {
    return base.GetHashCode();
  }
}

public class BlackboardComponent : MonoBehaviour, ISerializationCallbackReceiver
{
  // ------------------------------------------------- Variables -------------------------------------------------- //
  private Dictionary<string, object> Entries = new Dictionary<string, object>();
  [SerializeField] private List<BBEntry> SerializedEntries = new List<BBEntry>();

  // ------------------------------------------------- Interface -------------------------------------------------- //
  public bool TryGetValue(string key, out object obj)
  {
#if DEBUG_BB
    if (key == null)
    {
      Debug.LogError("AI ERROR: key was null in " + gameObject.GetComponent<BTAgent>().Tree.Name);
    }
#endif
    return Entries.TryGetValue(key, out obj);
  }

  public GameObject GetEntryAsGameObject(string entry)
  {
    object obj;
    TryGetValue(entry, out obj);
    if (obj.GetType().IsSubclassOf(typeof(MonoBehaviour)))
    {
      return ((MonoBehaviour)obj).gameObject;
    }
    if (obj.GetType() == (typeof(Transform)))
    {
      return ((Transform)obj).gameObject;
    }
    return (GameObject)obj;
  }

  public Transform GetEntryAsTransform(string entry)
  {
    object obj;
    TryGetValue(entry, out obj);
    if (obj.GetType() == typeof(GameObject))
    {
      return ((GameObject)obj).transform;
    }
    if (obj.GetType().IsSubclassOf(typeof(MonoBehaviour)))
    {
      return ((MonoBehaviour)obj).transform;
    }
    return (Transform)obj;
  }

  public Vector3 GetEntryAsVector3(string entry)
  {
    object obj;
    TryGetValue(entry, out obj);
    System.Type type = obj.GetType();
    if (type == typeof(Vector3))
    {
      return (Vector3)obj;
    }
    if (type == typeof(GameObject))
    {
      return ((GameObject)obj).transform.position;
    }
    if (type == typeof(Transform))
    {
      return ((Transform)obj).position;
    }
    if (type.IsSubclassOf(typeof(MonoBehaviour)))
    {
      return ((MonoBehaviour)obj).transform.position;
    }
    return Vector3.zero;
  }

  public object this[string key]
  {
    get
    {
#if DEBUG_BB
      if (!Entries.ContainsKey(key))
      {
        throw new SystemException("AI ERROR: Invalid blackboard key: \"" + key + "\" requested by: " + gameObject.name);
      }
      return Entries[key];
#else
    return Entries[entry];
#endif
    }
    set
    {
#if DEBUG_BB
      if (!Entries.ContainsKey(key))
      {
        throw new SystemException("AI ERROR: Invalid blackboard key: \"" + key + "\" requested by: " + gameObject.name);
      }
      Entries[key] = value;
#else
    return Entries[entry];
#endif
    }
  }

  // ------------------------------------------------- Serialization -------------------------------------------------- //
  void ISerializationCallbackReceiver.OnBeforeSerialize()
  {
    // only need the checks when we're editing
#if UNITY_EDITOR
    if (!Application.isPlaying)
    {
      // remove all serialized entries that no longer have a corresponding dictionary entry
      SerializedEntries.RemoveAll((BBEntry toRemove) =>
      {
        return !Entries.ContainsKey(toRemove.Key);
      });

      // work with entries that still belong
      foreach (string key in Entries.Keys)
      {
        // new entry? add it in.
        Predicate<BBEntry> pred = (BBEntry bbEntry) => { return bbEntry.Key == key; };
        BBEntry existingEntry = SerializedEntries.Find(pred);
        if (existingEntry == BBEntry.DEFAULT)
        {
          SerializedEntries.Add(new BBEntry(key, null, ""));
        }
      }
    }
#endif
  }

  void ISerializationCallbackReceiver.OnAfterDeserialize()
  {
    // Only need to reset entries if we're editing
#if UNITY_EDITOR
    Entries.Clear();
#endif
    for (int i = 0; i < SerializedEntries.Count; ++i)
    {
      Entries[SerializedEntries[i].Key] = SerializedEntries[i].Value;
    }
  }
}
