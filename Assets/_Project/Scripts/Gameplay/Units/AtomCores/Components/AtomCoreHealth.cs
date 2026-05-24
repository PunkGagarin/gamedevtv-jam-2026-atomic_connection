using System;
using _Project.Scripts.Audio.Domain;
using _Project.Scripts.Gameplay.Common.Health;
using _Project.Scripts.Gameplay.Units.AtomCores;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Units.AtomCores.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health))]
    public class AtomCoreHealth : MonoBehaviour
    {
        [field: SerializeField] private Health Health { get; set; }
        [field: SerializeField] private AtomCoreShield Shield { get; set; }
        [field: SerializeField] private Sounds DamageSound { get; set; } = Sounds.damage;
        [field: SerializeField] private Sounds DeathSound { get; set; } = Sounds.intensiveDamage;

        [Inject] private AudioService _audioService;

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

            int previousHealth = Health.CurrentHealth;
            Health.TakeDamage(amount);

            if (previousHealth <= Health.CurrentHealth)
                return;

            if (Health.IsAlive)
                _audioService?.PlaySfxWithRandomPitch(DamageSound);
            else
                _audioService?.PlaySound(DeathSound);

            AtomCoreEventBus.RiseOnDamageEvent(amount);
        }

        public void Kill()
        {
            bool wasAlive = Health.IsAlive;
            Health.Kill();

            if (wasAlive)
                _audioService?.PlaySound(DeathSound);
        }
    }
}
