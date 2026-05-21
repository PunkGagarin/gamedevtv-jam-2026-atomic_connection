using System;

public static class AtomCoreEventBus
{
    public static event Action<float> OnDamageEvent;

    public static void RiseOnDamageEvent(float amount) => OnDamageEvent?.Invoke(amount);
}
