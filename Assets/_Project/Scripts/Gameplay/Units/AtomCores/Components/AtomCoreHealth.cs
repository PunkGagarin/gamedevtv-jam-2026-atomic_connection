using System;
using _Project.Scripts.Gameplay.Common.Health;
using _Project.Scripts.Gameplay.Units.AtomCores;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.AtomCores.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health))]
    public class AtomCoreHealth : MonoBehaviour
    {
        [field: SerializeField] private Health Health { get; set; }
        [field: SerializeField] private AtomCoreShield Shield { get; set; }

        public bool IsAlive => Health.IsAlive;

        public event Action Died
        {
            add => Health.Died += value;
            remove => Health.Died -= value;
        }

        private void Awake()
        {
            if (Health == null)
                Health = GetComponent<Health>();

            if (Shield == null)
                Shield = GetComponent<AtomCoreShield>();
        }

        public void Configure(int maxHealth)
        {
            Health.Configure(maxHealth);
        }

        public void Tick(float deltaTime)
        {
            Shield?.Tick(deltaTime);
        }

        public void TakeDamage(int amount)
        {
            if (Shield != null && Shield.TryAbsorbDamage(amount))
                return;

            Health.TakeDamage(amount);
            AtomCoreEventBus.RiseOnDamageEvent(amount);
        }

        public void Kill()
        {
            Health.Kill();
        }
    }
}
