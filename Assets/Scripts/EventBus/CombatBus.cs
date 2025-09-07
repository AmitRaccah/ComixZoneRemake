using System;
using UnityEngine;

public static class CombatBus
{
    public static void Publish<T>(T e) where T : struct => ScopedEventBus<CombatScope>.Publish(e);
    public static void Subscribe<T>(Action<T> l) where T : struct => ScopedEventBus<CombatScope>.Subscribe(l);
    public static void Unsubscribe<T>(Action<T> l) where T : struct => ScopedEventBus<CombatScope>.Unsubscribe(l);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset() => ScopedEventBus<CombatScope>.ClearAll();
}
