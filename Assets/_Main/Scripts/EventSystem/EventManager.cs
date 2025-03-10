using System;
using System.Collections.Generic;
using UnityEngine;

public static class EventManager
{
    // Her event tipi için Action delegate listesi tutuluyor.
    // Action'ın parametresi object[] olduğundan, istediğiniz sayıda parametre gönderilebilir.
    private static Dictionary<GameEvents, Action<object[]>> eventDictionary = new Dictionary<GameEvents, Action<object[]>>();

    /// <summary>
    /// Belirtilen event'e abone olur.
    /// </summary>
    public static void Subscribe(GameEvents eventType, Action<object[]> listener)
    {
        if (eventDictionary.TryGetValue(eventType, out Action<object[]> thisEvent))
        {
            thisEvent += listener;
            eventDictionary[eventType] = thisEvent;
        }
        else
        {
            eventDictionary.Add(eventType, listener);
        }
    }

    /// <summary>
    /// Belirtilen event'ten aboneliği kaldırır.
    /// </summary>
    public static void Unsubscribe(GameEvents eventType, Action<object[]> listener)
    {
        if (eventDictionary.TryGetValue(eventType, out Action<object[]> thisEvent))
        {
            thisEvent -= listener;
            if (thisEvent == null)
            {
                eventDictionary.Remove(eventType);
            }
            else
            {
                eventDictionary[eventType] = thisEvent;
            }
        }
    }
    
    /// <summary>
    /// Belirtilen event'i tetikler ve parametreleri ilgili listener'lara iletir.
    /// </summary>
    public static void TriggerEvent(GameEvents eventType, params object[] parameters)
    {
        if (eventDictionary.TryGetValue(eventType, out Action<object[]> thisEvent))
        {
            thisEvent.Invoke(parameters);
        }
        else
        {
            Debug.LogWarning("EventManager: Event " + eventType + " için abone bulunamadı!");
        }
    }
}