/*******************************************************************************/
/*!
\file   GameObjectExtensions.cs
\author Nathan Negreiro
\par    All content © 2016-2017 DigiPen (USA) Corporation, all rights reserved.
\par    The Bakery

\brief
  Extends the GameObject class to have more functions assosiated with it
  
*/
/*******************************************************************************/
using UnityEngine;
using System.Collections;

public static class GameObjectExtensions
{

  #region EventSend
  /// <summary>
  /// Sends an event to this GameObject
  /// </summary>
  /// <param name="gameObject"></param>
  /// <param name="eventName">String of the event name - Upper CammelCase</param>
  public static void EventSend(this GameObject gameObject, string eventName)
  {
    Messenger.Broadcast(new EventObject(gameObject, eventName));
  }

  /// <summary>
  /// Sends an event to this GameObject with one parameter
  /// </summary>
  /// <param name="gameObject"></param>
  /// <param name="eventName">String of the event name - Upper CammelCase</param>
  /// <param name="param1">First Parameter</param>
  public static void EventSend<T>(this GameObject gameObject, string eventName, T param1)
  {
    Messenger.Broadcast(new EventObject(gameObject, eventName), param1);
  }

  /// <summary>
  /// Sends an event to this GameObject with two parameters
  /// </summary>
  /// <param name="gameObject"></param>
  /// <param name="eventName">String of the event name - Upper CammelCase</param>
  /// <param name="param1">First Parameter</param>
  /// <param name="param2">Second Parameter</param>
  public static void EventSend<T, U>(this GameObject gameObject, string eventName, T param1, U param2)
  {
    Messenger.Broadcast(new EventObject(gameObject, eventName), param1, param2);
  }

  /// <summary>
  /// Sends an event to this GameObject with three paramters
  /// </summary>
  /// <param name="gameObject"></param>
  /// <param name="eventName">String of the event name - Upper CammelCase</param>
  /// <param name="param1">First Parameter</param>
  /// <param name="param2">Second Parameter</param>
  /// <param name="param3">Third Parameter</param>
  public static void EventSend<T, U, V>(this GameObject gameObject, string eventName, T param1, U param2, V param3)
  {
    Messenger.Broadcast(new EventObject(gameObject, eventName), param1, param2, param3);
  }
  #endregion

  #region EventSubscribe
  /// <summary>
  /// Subscribes this GameObject to an event
  /// </summary>
  /// <param name="gameObject"></param>
  /// <param name="eventName">String of the event name - Upper CammelCase</param>
  /// <param name="callback">Function to call when the event is triggered</param>
  public static void EventSubscribe(this GameObject gameObject, string eventName, Callback callback)
  {
    Messenger.AddListener(new EventObject(gameObject, eventName), callback);
  }

  /// <summary>
  /// Subscribes this GameObject to an event with one parameter
  /// </summary>
  /// <param name="gameObject"></param>
  /// <param name="eventName">String of the event name - Upper CammelCase</param>
  /// <param name="callback">Function to call when the event is triggered</param>
  public static void EventSubscribe<T>(this GameObject gameObject, string eventName, Callback<T> callback)
  {
    Messenger.AddListener(new EventObject(gameObject, eventName), callback);
  }

  /// <summary>
  /// Subscribes this GameObject to an event with two parameters
  /// </summary>
  /// <param name="gameObject"></param>
  /// <param name="eventName">String of the event name - Upper CammelCase</param>
  /// <param name="callback">Function to call when the event is triggered</param>
  public static void EventSubscribe<T, U>(this GameObject gameObject, string eventName, Callback<T, U> callback)
  {
    Messenger.AddListener(new EventObject(gameObject, eventName), callback);
  }

  /// <summary>
  /// Subscribes this GameObject to an event with three parameters
  /// </summary>
  /// <param name="gameObject"></param>
  /// <param name="eventName">String of the event name - Upper CammelCase</param>
  /// <param name="callback">Function to call when the event is triggered</param>
  public static void EventSubscribe<T, U, V>(this GameObject gameObject, string eventName, Callback<T, U, V> callback)
  {
    Messenger.AddListener(new EventObject(gameObject, eventName), callback);
  }
  #endregion

  #region EventUnsubscribe
  /// <summary>
  /// Subscribes this GameObject to an event
  /// </summary>
  /// <param name="gameObject"></param>
  /// <param name="eventName">String of the event name - Upper CammelCase</param>
  /// <param name="callback">Function to call when the event is triggered</param>
  public static void EventUnsubscribe(this GameObject gameObject, string eventName, Callback callback)
  {
    Messenger.RemoveListener(new EventObject(gameObject, eventName), callback);
  }

  /// <summary>
  /// Subscribes this GameObject to an event with one parameter
  /// </summary>
  /// <param name="gameObject"></param>
  /// <param name="eventName">String of the event name - Upper CammelCase</param>
  /// <param name="callback">Function to call when the event is triggered</param>
  public static void EventUnsubscribe<T>(this GameObject gameObject, string eventName, Callback<T> callback)
  {
    Messenger.RemoveListener(new EventObject(gameObject, eventName), callback);
  }

  /// <summary>
  /// Subscribes this GameObject to an event with two parameters
  /// </summary>
  /// <param name="gameObject"></param>
  /// <param name="eventName">String of the event name - Upper CammelCase</param>
  /// <param name="callback">Function to call when the event is triggered</param>
  public static void EventUnsubscribe<T, U>(this GameObject gameObject, string eventName, Callback<T, U> callback)
  {
    Messenger.RemoveListener(new EventObject(gameObject, eventName), callback);
  }

  /// <summary>
  /// Subscribes this GameObject to an event with three parameters
  /// </summary>
  /// <param name="gameObject"></param>
  /// <param name="eventName">String of the event name - Upper CammelCase</param>
  /// <param name="callback">Function to call when the event is triggered</param>
  public static void EventUnsubscribe<T, U, V>(this GameObject gameObject, string eventName, Callback<T, U, V> callback)
  {
    Messenger.RemoveListener(new EventObject(gameObject, eventName), callback);
  }
  #endregion

}
