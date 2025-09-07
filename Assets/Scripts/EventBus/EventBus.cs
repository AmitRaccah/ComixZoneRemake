using System;
using System.Collections.Generic;
using UnityEngine;

public static class ScopedEventBus<TScope>
{
    private static readonly Dictionary<Type, Delegate> listeners = new Dictionary<Type, Delegate>();

    public static void ClearAll() => listeners.Clear();

    public static int CountListeners<T>() where T : struct
    {
        if (!listeners.TryGetValue(typeof(T), out var d) || d == null) return 0;
        return d.GetInvocationList().Length;
    }

    public static void Subscribe<T>(Action<T> listener) where T : struct
    {
        if (listener == null) return;
        var type = typeof(T);
        listeners.TryGetValue(type, out var existing);
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

    public static void Publish<T>(T e) where T : struct
    {
        var type = typeof(T);
        if (!listeners.TryGetValue(type, out var d) || d == null) return;
        var action = d as Action<T>;
        if (action == null) return;

        var inv = action.GetInvocationList();
        for (int i = 0; i < inv.Length; i++)
        {
            try { ((Action<T>)inv[i])(e); }
            catch (Exception ex)
            {
                Debug.LogError($"[ScopedEventBus<{typeof(TScope).Name}>] Listener threw on {type.Name}: {ex}");
            }
        }
    }

    public static void Unsubscribe<T>(Action<T> listener) where T : struct
    {
        if (listener == null) return;
        var type = typeof(T);
        if (!listeners.TryGetValue(type, out var existing) || existing == null) return;

        var action = existing as Action<T>;
        if (action == null) return;

        action -= listener;
        if (action == null) listeners.Remove(type);
        else listeners[type] = action;
    }
}
