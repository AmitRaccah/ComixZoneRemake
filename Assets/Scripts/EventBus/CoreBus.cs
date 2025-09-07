using System;
using UnityEngine;

public static class CoreBus
{
    public static void Publish<T>(T e) where T : struct => ScopedEventBus<CoreScope>.Publish(e);
    public static void Subscribe<T>(Action<T> l) where T : struct => ScopedEventBus<CoreScope>.Subscribe(l);
    public static void Unsubscribe<T>(Action<T> l) where T : struct => ScopedEventBus<CoreScope>.Unsubscribe(l);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset() => ScopedEventBus<CoreScope>.ClearAll();
}
