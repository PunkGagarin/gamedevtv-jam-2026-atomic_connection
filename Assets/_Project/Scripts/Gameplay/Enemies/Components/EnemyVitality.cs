using System;
using _Project.Scripts.Gameplay.Common.Health;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Enemies.Components
{
    [RequireComponent(typeof(EnemyUnit))]
    [RequireComponent(typeof(Health))]
    public class EnemyVitality : MonoBehaviour
    {
        [field: SerializeField] private EnemyUnit Enemy { get; set; }
        [field: SerializeField] private Health Health { get; set; }

        public bool IsAlive => Health == null || Health.IsAlive;
        public int MaxHealth => Health?.MaxHealth ?? 1;
        public int CurrentHealth => Health?.CurrentHealth ?? MaxHealth;

        public event Action<EnemyUnit> Died;
        public event Action<EnemyUnit> Killed;

        private void Awake()
        {
            if (Enemy == null)
                Enemy = GetComponent<EnemyUnit>();

            if (Health == null)
                Health = GetComponent<Health>();

            if (Health != null)
                Health.Died += OnHealthDied;
        }

        private void OnDestroy()
        {
            if (Health != null)
                Health.Died -= OnHealthDied;
        }

        public void Configure(int maxHealth)
        {
            Health?.Configure(maxHealth);
        }

        public void Configure(int maxHealth, int currentHealth)
        {
            Health?.Configure(maxHealth, currentHealth);
        }

        public void ResetHealth()
        {
            Health?.ResetHealth();
        }

        public void TakeDamage(int amount)
        {
            if (Health != null)
                Health.TakeDamage(amount);
            else
                Kill();
        }

        private void Kill()
        {
            if (Health != null)
                Health.Kill();
            else
                Died?.Invoke(Enemy);
        }

        public void DieFromCore()
        {
            if (Health != null)
            {
                Health.Died -= OnHealthDied;
                Health.Kill();
                Health.Died += OnHealthDied;
            }

            Died?.Invoke(Enemy);
        }

        private void OnHealthDied()
        {
            Killed?.Invoke(Enemy);
            Died?.Invoke(Enemy);
        }
    }
}
