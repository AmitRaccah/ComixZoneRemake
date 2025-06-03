using UnityEngine;
using System;
using System.Collections.Generic;

public static class EventBus
{
    private static Dictionary<Type, Delegate> listeners = new Dictionary<Type, Delegate>();

    public static void Subscribe<T>(Action<T> listener) where T : struct
    {
        Type type = typeof(T);

        if (listeners.ContainsKey(type))
        {
            listeners[type] = Delegate.Combine(listeners[type], listener);
        }
        else
        {
            listeners[type] = listener;
        }
    }

    public static void Publish<T>(T publishEvent) where T : struct
    {
        Type type = typeof(T);

        if (listeners.ContainsKey(type))
        {
            Action<T> action = listeners[type] as Action<T>;

            if (action != null)
            {
                action.Invoke(publishEvent);
            }

        }

    }

    public static void Unsubscribe<T>(Action<T> listener) where T : struct
    {
        Type type = typeof(T);

        if (listeners.ContainsKey(type))
        {
            Delegate current = listeners[type];

            Delegate updated = Delegate.Remove(current, listener);

            if (updated == null)
                listeners.Remove(type);
            else
                listeners[type] = updated;
        }
    }
}



