using System;

public static class CombatBus
{
    public static void Publish<T>(T e) where T : struct
        => EventBus.Publish(e);

    public static void Subscribe<T>(Action<T> l) where T : struct
        => EventBus.Subscribe(l);

    public static void Unsubscribe<T>(Action<T> l) where T : struct
        => EventBus.Unsubscribe(l);
}
