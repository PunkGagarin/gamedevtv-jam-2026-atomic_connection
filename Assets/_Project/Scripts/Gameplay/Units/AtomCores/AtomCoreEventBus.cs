using System;

public static class AtomCoreEventBus
{
    public static event Action<float> OnDamageEvent;

    public static event Action OnClickEvent;

    public static void RiseOnDamageEvent(float amount) => OnDamageEvent?.Invoke(amount);

    public static void RiseOnClickEvent() => OnClickEvent?.Invoke();
}
