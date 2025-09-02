using System;
using System.Collections.Generic;
using UnityEngine;

public static class EventBus
{
    private static readonly Dictionary<Type, Delegate> listeners = new Dictionary<Type, Delegate>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlayEnter()
    {
        listeners.Clear();
    }

    public static void ClearAll()
    {
        listeners.Clear();
    }

    public static int CountListeners<T>() where T : struct
    {
        Delegate d;
        if (!listeners.TryGetValue(typeof(T), out d) || d == null) return 0;
        return d.GetInvocationList().Length;
    }

    public static void Subscribe<T>(Action<T> listener) where T : struct
    {
        if (listener == null) return;

        var type = typeof(T);
        Delegate existing;
        listeners.TryGetValue(type, out existing);

        var action = existing as Action<T>;

        if (action != null)
        {
            var inv = action.GetInvocationList();
            for (int i = 0; i < inv.Length; i++)
            {
                var a = (Action<T>)inv[i];
                if (a.Target == listener.Target && a.Method == listener.Method)
                    return; 
            }
            action += listener;
        }
        else
        {
            action = listener;
        }

        listeners[type] = action;
    }

    public static void Publish<T>(T publishEvent) where T : struct
    {
        var type = typeof(T);

        Delegate d;
        if (!listeners.TryGetValue(type, out d) || d == null) return;

        var action = d as Action<T>;
        if (action == null) return;

        var inv = action.GetInvocationList();
        for (int i = 0; i < inv.Length; i++)
        {
            try
            {
                ((Action<T>)inv[i])(publishEvent);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EventBus] Listener threw on {type.Name}: {ex}");
            }
        }
    }

    public static void Unsubscribe<T>(Action<T> listener) where T : struct
    {
        if (listener == null) return;

        var type = typeof(T);
        Delegate existing;
        if (!listeners.TryGetValue(type, out existing) || existing == null) return;

        var action = existing as Action<T>;
        if (action == null) return;

        action -= listener;
        if (action == null)
            listeners.Remove(type);
        else
            listeners[type] = action;
    }
}
