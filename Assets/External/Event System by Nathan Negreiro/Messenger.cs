/*******************************************************************************/
/*!
\file   Messenger.cs
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

//Struct used for GameObject dependant events
public struct EventObject
{
  public GameObject gameObject;
  public string eventName;

  public EventObject(GameObject gameObject, string eventName)
  {
    this.gameObject = gameObject;
    this.eventName = eventName;
  }
}

static internal class Messenger
{
  #region Internal variables

  private static bool _logBroadcast = false;
  private static bool _logAddListener = false;
  private static bool _logRemoveListener = false;
  private static bool _logCleanup = false;

  //Disable the unused variable warning
  //#pragma warning disable 0414
  //Ensures that the MessengerHelper will be created automatically upon start of the game.
  static private MessengerHelper messengerHelper = (new GameObject("MessengerHelper")).AddComponent<MessengerHelper>();
  //#pragma warning restore 0414

  static public Dictionary<EventObject, Delegate> eventTable = new Dictionary<EventObject, Delegate>();
  #endregion

  #region Helper methods
  static public void Cleanup()
  {
    if (_logCleanup)
      Debug.Log("MESSENGER Cleanup.");

    List<EventObject> messagesToRemove = new List<EventObject>();

    foreach (EventObject eventObject in messagesToRemove)
    {
      eventTable.Remove(eventObject);
    }
  }

  static public void PrintEventTable()
  {
    Debug.Log("=== GLOBAL MESSENGER TABLE ===");

    foreach (KeyValuePair<EventObject, Delegate> pair in eventTable)
    {
      Debug.Log(pair.Key + "\t\t" + pair.Value);
    }

    Debug.Log("=== END GLOBAL MESSENGER TABLE ===");

    Debug.Log("\n");
  }
  #endregion

  #region Message logging and exception throwing
  static public void OnListenerAdding(EventObject eventType, Delegate listenerBeingAdded)
  {
    if (_logAddListener)
      Debug.Log("GLOBAL MESSENGER ADDING: \nListener: " + eventType + "\nTarget: " + listenerBeingAdded.Target + "\nMethod: " + listenerBeingAdded.Method);

    if (!eventTable.ContainsKey(eventType))
    {
      eventTable.Add(eventType, null);
    }

    // TODO: Overload to allow for multiple subscriptions with one listener
    Delegate d = eventTable[eventType];
    if (d != null && d.GetType() != listenerBeingAdded.GetType() && listenerBeingAdded.GetType() != null)
    {
      //if (d as Callback != null || listenerBeingAdded as Callback != null)
      //  return; 

      throw new ListenerException("Inconsistent listeners of " + eventType.eventName + " ."
        + "Current have type " + d.GetType().Name + " and new listener has type " + listenerBeingAdded.GetType().Name);
    }
  }

  static public void OnListenerRemoving(EventObject eventType, Delegate listenerBeingRemoved)
  {
    if (_logRemoveListener)
      Debug.Log("GLOBAL MESSENGER REMOVING: \nListener: " + eventType + "\nTarget: " + listenerBeingRemoved.Target + "\nMethod: " + listenerBeingRemoved.Method);


    if (eventTable.ContainsKey(eventType))
    {
      Delegate d = eventTable[eventType];

      if (d == null)
        throw new ListenerException("Attempting to remove listener with for event type " + eventType + " but current listener is null.");

      else if (d.GetType() != listenerBeingRemoved.GetType())
        throw new ListenerException("Attempting to remove listener with inconsistent signature for event " + eventType.eventName + " ."
          + "Current listeners have type " + d.GetType() + " and listener being added has type " + listenerBeingRemoved.GetType().Name);
    }
    else
      throw new ListenerException("Attempting to remove listener for type " + eventType + " but Messenger doesn't know about this event type.");
  }

  static public void OnListenerRemoved(EventObject eventType)
  {
    if (eventTable[eventType] == null)
      eventTable.Remove(eventType);
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
  static public void AddListener(EventObject eventType, Callback handler)
  {
    if (eventType.eventName == "ALL")
      throw new Exception("All object subscribing to \"ALL\" MUST take a string.");

    OnListenerAdding(eventType, handler);
    eventTable[eventType] = (Callback)eventTable[eventType] + handler;
  }

  //Single parameter
  static public void AddListener<T>(EventObject eventType, Callback<T> handler)
  {
    if (eventType.eventName == "ALL" && handler.Method.GetParameters()[0].ParameterType != typeof(string))
      throw new Exception("All object subscribing to \"ALL\" MUST take a string, not a " + handler.Method.GetParameters()[0].ParameterType);

    OnListenerAdding(eventType, handler);
    eventTable[eventType] = (Callback<T>)eventTable[eventType] + handler;
  }

  //Two parameters
  static public void AddListener<T, U>(EventObject eventType, Callback<T, U> handler)
  {
    OnListenerAdding(eventType, handler);
    eventTable[eventType] = (Callback<T, U>)eventTable[eventType] + handler;
  }

  //Three parameters
  static public void AddListener<T, U, V>(EventObject eventType, Callback<T, U, V> handler)
  {
    OnListenerAdding(eventType, handler);
    eventTable[eventType] = (Callback<T, U, V>)eventTable[eventType] + handler;
  }
  #endregion

  #region RemoveListener
  //No parameters
  static public void RemoveListener(EventObject eventType, Callback handler)
  {
    OnListenerRemoving(eventType, handler);
    eventTable[eventType] = (Callback)eventTable[eventType] - handler;
    OnListenerRemoved(eventType);
  }

  //Single parameter
  static public void RemoveListener<T>(EventObject eventType, Callback<T> handler)
  {
    OnListenerRemoving(eventType, handler);
    eventTable[eventType] = (Callback<T>)eventTable[eventType] - handler;
    OnListenerRemoved(eventType);
  }

  //Two parameters
  static public void RemoveListener<T, U>(EventObject eventType, Callback<T, U> handler)
  {
    OnListenerRemoving(eventType, handler);
    eventTable[eventType] = (Callback<T, U>)eventTable[eventType] - handler;
    OnListenerRemoved(eventType);
  }

  //Three parameters
  static public void RemoveListener<T, U, V>(EventObject eventType, Callback<T, U, V> handler)
  {
    OnListenerRemoving(eventType, handler);
    eventTable[eventType] = (Callback<T, U, V>)eventTable[eventType] - handler;
    OnListenerRemoved(eventType);
  }
  #endregion

  #region Broadcast
  //No parameters
  static public void Broadcast(EventObject eventType)
  {
    if (_logBroadcast)
      Debug.Log("GLOBAL MESSENGER BROADCASTING: " + eventType + " with 0 variables");

    Delegate d;
    if (eventTable.TryGetValue(eventType, out d))
    {
      Callback callback = d as Callback;

      if (callback != null)
        callback();
      else
        throw CreateBroadcastSignatureException(eventType.eventName);
    }

    if (eventTable.TryGetValue(new EventObject(eventType.gameObject, "ALL"), out d))
    {
      Callback<string> callback = d as Callback<string>;

      if (callback != null)
        callback(eventType.eventName);
    }
  }

  //Single parameter
  static public void Broadcast<T>(EventObject eventType, T arg1)
  {
    if (_logBroadcast)
      Debug.Log("GLOBAL MESSENGER BROADCASTING: " + eventType + " with 1 variables");

    Delegate d;
    if (eventTable.TryGetValue(eventType, out d))
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
          throw new BroadcastException(CreateBroadcastSignatureException(eventType.eventName).ToString()
            + "\n Tried to find a suitable match without paremeters, but failed. (From 1 parameters)");
      }
    }

    if (eventTable.TryGetValue(new EventObject(eventType.gameObject, "ALL"), out d))
    {
      Callback<string> callback = d as Callback<string>;

      if (callback != null)
        callback(eventType.eventName);
    }
  }

  //Two parameters
  static public void Broadcast<T, U>(EventObject eventType, T arg1, U arg2)
  {
    if (_logBroadcast)
      Debug.Log("GLOBAL MESSENGER BROADCASTING: " + eventType + " with 2 variables");

    Delegate d;
    if (eventTable.TryGetValue(eventType, out d))
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
          throw new BroadcastException(CreateBroadcastSignatureException(eventType.eventName).ToString()
            + "\n Tried to find a suitable match without paremeters, but failed. (From 2 parameters)");
      }
    }

    if (eventTable.TryGetValue(new EventObject(eventType.gameObject, "ALL"), out d))
    {
      Callback<string> callback = d as Callback<string>;

      if (callback != null)
        callback(eventType.eventName);
    }
  }

  //Three parameters
  static public void Broadcast<T, U, V>(EventObject eventType, T arg1, U arg2, V arg3)
  {
    if (_logBroadcast)
      Debug.Log("GLOBAL MESSENGER BROADCASTING: " + eventType + " with 3 variables");

    Delegate d;
    if (eventTable.TryGetValue(eventType, out d))
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
          throw new BroadcastException(CreateBroadcastSignatureException(eventType.eventName).ToString()
            + "\n Tried to find a suitable match without paremeters, but failed. (From 3 parameters)");
      }
    }

    if (eventTable.TryGetValue(new EventObject(eventType.gameObject, "ALL"), out d))
    {
      Callback<string> callback = d as Callback<string>;

      if (callback != null)
        callback(eventType.eventName);
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