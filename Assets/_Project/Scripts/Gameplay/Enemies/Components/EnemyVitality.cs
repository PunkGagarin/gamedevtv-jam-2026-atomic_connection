using System;
using _Project.Scripts.Audio.Domain;
using _Project.Scripts.Gameplay.Common.Health;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Enemies.Components
{
    [RequireComponent(typeof(EnemyUnit))]
    [RequireComponent(typeof(Health))]
    public class EnemyVitality : MonoBehaviour
    {
        [field: SerializeField] private EnemyUnit Enemy { get; set; }
        [field: SerializeField] private Health Health { get; set; }
        [field: SerializeField] private Sounds DamageSound { get; set; } = Sounds.damabProbably;
        [field: SerializeField] private Sounds DeathSound { get; set; } = Sounds.doublePop;

        [Inject] private AudioService _audioService;

        public bool IsAlive => Health == null || Health.IsAlive;
        public int MaxHealth => Health?.MaxHealth ?? 1;
        public int CurrentHealth => Health?.CurrentHealth ?? MaxHealth;
        public float KillRewardMultiplier { get; private set; } = 1f;

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
            KillRewardMultiplier = 1f;
        }

        public void Configure(int maxHealth, int currentHealth)
        {
            Health?.Configure(maxHealth, currentHealth);
        }

        public void ResetHealth()
        {
            Health?.ResetHealth();
            KillRewardMultiplier = 1f;
        }

        public void TakeDamage(int amount)
        {
            TakeDamage(amount, 1f, false);
        }

        public void TakeDamage(int amount, float killRewardMultiplier, bool isCritical)
        {
            KillRewardMultiplier = Mathf.Max(0f, killRewardMultiplier);
            int previousHealth = CurrentHealth;

            if (Health != null)
                Health.TakeDamage(amount, isCritical);
            else
                Kill();

            if (Health != null && Health.IsAlive && previousHealth > Health.CurrentHealth)
                _audioService?.PlaySfxWithRandomPitch(DamageSound);

            if (IsAlive)
                KillRewardMultiplier = 1f;
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
            KillRewardMultiplier = 1f;
            KillWithoutHealthEvents();
            PlayDeathSound();
            Died?.Invoke(Enemy);
        }

        public void ApplyMergeDamage(int amount, bool isCritical)
        {
            if (Health == null)
                return;

            int previousHealth = Health.CurrentHealth;
            Health.Died -= OnHealthDied;
            Health.TakeDamage(amount, isCritical);
            Health.Died += OnHealthDied;

            if (Health.IsAlive && previousHealth > Health.CurrentHealth)
                _audioService?.PlaySfxWithRandomPitch(DamageSound);
        }

        public void DieFromMergeGroupDamage(float killRewardMultiplier)
        {
            KillRewardMultiplier = Mathf.Max(0f, killRewardMultiplier);
            KillWithoutHealthEvents();
            PlayDeathSound();
            Killed?.Invoke(Enemy);
            Died?.Invoke(Enemy);
        }

        public void DieFromMergeGroupCore()
        {
            KillRewardMultiplier = 1f;
            KillWithoutHealthEvents();
            PlayDeathSound();
            Died?.Invoke(Enemy);
        }

        private void KillWithoutHealthEvents()
        {
            if (Health != null)
            {
                Health.Died -= OnHealthDied;
                Health.Kill();
                Health.Died += OnHealthDied;
            }
        }

        private void OnHealthDied()
        {
            PlayDeathSound();
            Killed?.Invoke(Enemy);
            Died?.Invoke(Enemy);
        }

        private void PlayDeathSound()
        {
            _audioService?.PlaySound(DeathSound);
        }
    }
}
