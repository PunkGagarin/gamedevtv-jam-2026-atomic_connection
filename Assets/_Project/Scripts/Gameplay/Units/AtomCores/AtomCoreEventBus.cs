using System;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.AtomCores
{
    public static class AtomCoreEventBus
    {
        public static event Action<float> OnDamageEvent;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            OnDamageEvent = null;
        }

        public static void RiseOnDamageEvent(float amount) => OnDamageEvent?.Invoke(amount);
    }
}
