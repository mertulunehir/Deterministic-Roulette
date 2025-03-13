using System;
using System.Collections.Generic;
using UnityEngine;

public static class EventManager
{
    private static Dictionary<GameEvents, Action<object[]>> eventDictionary = new Dictionary<GameEvents, Action<object[]>>();


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