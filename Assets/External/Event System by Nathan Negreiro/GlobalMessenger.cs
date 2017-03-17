/*******************************************************************************/
/*!
\file   GlobalMessenger.cs
\author Nathan Negreiro
\par    All content © 2016-2017 DigiPen (USA) Corporation, all rights reserved.
\par    The Bakery

\brief
  Provides templated types to use with messaging
  
*/
/*******************************************************************************/

// Messaging System By Nathan Negreiro Based on messenger by Ilya Suzdalnitsky

using System;
using System.Collections.Generic;
using UnityEngine;

static internal class GlobalMessenger
{
  #region Internal variables

  private static bool _logBroadcast = false;
  private static bool _logAddListener = false;
  private static bool _logRemoveListener = false;
  private static bool _logCleanup = false;

  //Disable the unused variable warning
  //#pragma warning disable 0414
  // Cleans up our gloablEventTable between scenes
  static private MessengerHelper messengerHelper = (new GameObject("GlobalMessengerHelper")).AddComponent<MessengerHelper>();
  //#pragma warning restore 0414

  // Holds all of our messages
  static public Dictionary<string, Delegate> globalEventTable = new Dictionary<string, Delegate>();
  #endregion

  #region Helper methods
  static public void Cleanup()
  {
    if(_logCleanup)
  		Debug.Log("MESSENGER Cleanup.");


    List<string> messagesToRemove = new List<string>();

    foreach (KeyValuePair<string, Delegate> pair in globalEventTable)
    {
      globalEventTable.Remove(pair.Key);
    }
  }

  static public void PrintEventTable()
  {
    Debug.Log("=== GLOBAL MESSENGER TABLE ===");

    foreach (KeyValuePair<string, Delegate> pair in globalEventTable)
    {
      Debug.Log(pair.Key + "\t\t" + pair.Value);
    }

    Debug.Log("=== END GLOBAL MESSENGER TABLE ===");

    Debug.Log("\n");
  }
  #endregion

  #region Message logging and exception throwing
  static public void OnListenerAdding(string eventType, Delegate listenerBeingAdded)
  {
    if(_logAddListener)
		  Debug.Log("GLOBAL MESSENGER ADDING: \nListener: " + eventType + "\nTarget: " + listenerBeingAdded.Target + "\nMethod: " + listenerBeingAdded.Method);

    if (!globalEventTable.ContainsKey(eventType))
    {
      globalEventTable.Add(eventType, null);
    }


    // TODO: Overload to allow for multiple subscriptions with one listener
    Delegate d = globalEventTable[eventType];
    if (d != null && d.GetType() != listenerBeingAdded.GetType() && listenerBeingAdded.GetType() != null)
    {
      throw new ListenerException("Attempting to add listener with inconsistent signature for event type " + eventType + " ." 
        + "Current listeners have type " + d.GetType() + " and listener being added has type " + listenerBeingAdded.GetType().Name);
    }
  }

  static public void OnListenerRemoving(string eventType, Delegate listenerBeingRemoved)
  {
    if(_logRemoveListener)
		  Debug.Log("GLOBAL MESSENGER REMOVING: \nListener: " + eventType + "\nTarget: " + listenerBeingRemoved.Target + "\nMethod: " + listenerBeingRemoved.Method);


    if (globalEventTable.ContainsKey(eventType))
    {
      Delegate d = globalEventTable[eventType];

      if (d == null)
        throw new ListenerException("Attempting to remove listener with for event type " + eventType + " but current listener is null.");

      else if (d.GetType() != listenerBeingRemoved.GetType())
        throw new ListenerException("Attempting to remove listener with inconsistent signature for event type " + eventType + " ." 
          + "Current listeners have type " + d.GetType() + " and listener being added has type " + listenerBeingRemoved.GetType().Name);
    }
    else
      throw new ListenerException("Attempting to remove listener for type " + eventType + " but Messenger doesn't know about this event type.");
  }

  static public void OnListenerRemoved(string eventType)
  {
    if (globalEventTable[eventType] == null)
      globalEventTable.Remove(eventType);
  }

  static public BroadcastException CreateBroadcastSignatureException(string eventType)
  {
    return new BroadcastException("Broadcasting message + " + eventType + " but listeners have a different signature than the broadcaster.");
  }

  public class BroadcastException : Exception
  {
    public BroadcastException(string msg) : base(msg) { }
  }

  public class ListenerException : Exception
  {
    public ListenerException(string msg) : base(msg) { }
  }
  #endregion

  #region AddListener
  //No parameters
  static public void AddListener(string eventType, Callback handler)
  {
    if (eventType == "ALL")
      throw new Exception("All object subscribing to \"ALL\" MUST take a string.");

    OnListenerAdding(eventType, handler);
    globalEventTable[eventType] = (Callback)globalEventTable[eventType] + handler;
  }

  //Single parameter
  static public void AddListener<T>(string eventType, Callback<T> handler)
  {
    if (eventType == "ALL" && handler.Method.GetParameters()[0].ParameterType != typeof(string))
      throw new Exception("All object subscribing to \"ALL\" MUST take a string, not a " + handler.Method.GetParameters()[0].ParameterType);

    OnListenerAdding(eventType, handler);
    globalEventTable[eventType] = (Callback<T>)globalEventTable[eventType] + handler;
  }

  //Two parameters
  static public void AddListener<T, U>(string eventType, Callback<T, U> handler)
  {
    OnListenerAdding(eventType, handler);
    globalEventTable[eventType] = (Callback<T, U>)globalEventTable[eventType] + handler;
  }

  //Three parameters
  static public void AddListener<T, U, V>(string eventType, Callback<T, U, V> handler)
  {
    OnListenerAdding(eventType, handler);
    globalEventTable[eventType] = (Callback<T, U, V>)globalEventTable[eventType] + handler;
  }
  #endregion

  #region RemoveListener
  //No parameters
  static public void RemoveListener(string eventType, Callback handler)
  {
    OnListenerRemoving(eventType, handler);
    globalEventTable[eventType] = (Callback)globalEventTable[eventType] - handler;
    OnListenerRemoved(eventType);
  }

  //Single parameter
  static public void RemoveListener<T>(string eventType, Callback<T> handler)
  {
    OnListenerRemoving(eventType, handler);
    globalEventTable[eventType] = (Callback<T>)globalEventTable[eventType] - handler;
    OnListenerRemoved(eventType);
  }

  //Two parameters
  static public void RemoveListener<T, U>(string eventType, Callback<T, U> handler)
  {
    OnListenerRemoving(eventType, handler);
    globalEventTable[eventType] = (Callback<T, U>)globalEventTable[eventType] - handler;
    OnListenerRemoved(eventType);
  }

  //Three parameters
  static public void RemoveListener<T, U, V>(string eventType, Callback<T, U, V> handler)
  {
    OnListenerRemoving(eventType, handler);
    globalEventTable[eventType] = (Callback<T, U, V>)globalEventTable[eventType] - handler;
    OnListenerRemoved(eventType);
  }
  #endregion

  #region Broadcast
  //No parameters
  static public void Broadcast(string eventType)
  {
    if(_logBroadcast)
		  Debug.Log("GLOBAL MESSENGER BROADCASTING: " + eventType + " with 0 variables");

    Delegate d;
    if (globalEventTable.TryGetValue(eventType, out d))
    {
      Callback callback = d as Callback;

      if (callback != null)
        callback();
      else
        throw CreateBroadcastSignatureException(eventType);
    }

    if (globalEventTable.TryGetValue("ALL", out d))
    {
      Callback<string> callback = d as Callback<string>;

      if (callback != null)
        callback(eventType);
    }
  }

  //Single parameter
  static public void Broadcast<T>(string eventType, T arg1)
  {
    if (_logBroadcast)
      Debug.Log("GLOBAL MESSENGER BROADCASTING: " + eventType + " with 1 variables");

    Delegate d;
    if (globalEventTable.TryGetValue(eventType, out d))
    {
      Callback<T> callback = d as Callback<T>;

      if (callback != null)
        callback(arg1);
      else
      {
        Callback fallback = d as Callback;
        if (fallback != null)
          fallback();
        else
          throw new BroadcastException(CreateBroadcastSignatureException(eventType).ToString() 
            + "\n Tried to find a suitable match without paremeters, but failed. (From 1 parameters)");
      }
    }

    if (globalEventTable.TryGetValue("ALL", out d))
    {
      Callback<string> callback = d as Callback<string>;

      if (callback != null)
        callback(eventType);
    }
  }

  //Two parameters
  static public void Broadcast<T, U>(string eventType, T arg1, U arg2)
  {
    if (_logBroadcast)
      Debug.Log("GLOBAL MESSENGER BROADCASTING: " + eventType + " with 2 variables");

    Delegate d;
    if (globalEventTable.TryGetValue(eventType, out d))
    {
      Callback<T, U> callback = d as Callback<T, U>;

      if (callback != null)
        callback(arg1, arg2);
      else
      {
        Callback fallback = d as Callback;
        if (fallback != null)
          fallback();
        else
          throw new BroadcastException(CreateBroadcastSignatureException(eventType).ToString()
            + "\n Tried to find a suitable match without paremeters, but failed. (From 2 parameters)");
      }
    }

    if (globalEventTable.TryGetValue("ALL", out d))
    {
      Callback<string> callback = d as Callback<string>;

      if (callback != null)
        callback(eventType);
    }
  }

  //Three parameters
  static public void Broadcast<T, U, V>(string eventType, T arg1, U arg2, V arg3)
  {
    if (_logBroadcast)
      Debug.Log("GLOBAL MESSENGER BROADCASTING: " + eventType + " with 3 variables");

    Delegate d;
    if (globalEventTable.TryGetValue(eventType, out d))
    {
      Callback<T, U, V> callback = d as Callback<T, U, V>;

      if (callback != null)
        callback(arg1, arg2, arg3);
      else
      {
        Callback fallback = d as Callback;
        if (fallback != null)
          fallback();
        else
          throw new BroadcastException(CreateBroadcastSignatureException(eventType).ToString()
            + "\n Tried to find a suitable match without paremeters, but failed. (From 3 parameters)");
      }
    }

    if (globalEventTable.TryGetValue("ALL", out d))
    {
      Callback<string> callback = d as Callback<string>;

      if (callback != null)
        callback(eventType);
    }
  }
  #endregion

  #region Helper Functions
  public static void SetLogBroadcast(bool b)
  {
    _logBroadcast = b;
  }

  public static void SetLogAddListener(bool b)
  {
    _logAddListener = b;
  }

  public static void SetLogRemoveListener(bool b)
  {
    _logRemoveListener = b;
  }

  public static void SetLogCleanup(bool b)
  {
    _logCleanup = b;
  }
  #endregion
}

//This manager will ensure that the messenger's eventTable will be cleaned up upon loading of a new level.
public sealed class MessengerHelper : MonoBehaviour
{
  void Awake()
  {
    DontDestroyOnLoad(gameObject);
  }

  //Clean up eventTable every time a new level loads.
  public void OnLevelWasLoaded(int unused)
  {
    Messenger.Cleanup();
  }
}