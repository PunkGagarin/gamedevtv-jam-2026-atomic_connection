using UnityEngine;

namespace _Project.Scripts.Gameplay.Enemies.Components
{
    [RequireComponent(typeof(EnemyIdentity))]
    [RequireComponent(typeof(EnemyVitality))]
    public class EnemyMergeState : MonoBehaviour
    {
        [field: SerializeField] private EnemyIdentity Identity { get; set; }
        [field: SerializeField] private EnemyVitality Vitality { get; set; }

        private EnemyMergeState _partner;
        private int _preMergeMaxHealth;
        private int _preMergeCurrentHealth;

        public bool IsLinked => _partner != null;
        public int CoreCollisionDamage => Identity.CoreCollisionDamage + (_partner != null ? _partner.Identity.CoreCollisionDamage : 0);

        private void Awake()
        {
            if (Identity == null)
                Identity = GetComponent<EnemyIdentity>();

            if (Vitality == null)
                Vitality = GetComponent<EnemyVitality>();

            Vitality.Killed += OnKilled;
        }

        private void OnDestroy()
        {
            if (Vitality != null)
                Vitality.Killed -= OnKilled;
        }

        public void BeginLink(EnemyUnit partner, int mergedMaxHealth, int mergedCurrentHealth)
        {
            if (partner == null)
                return;

            _partner = partner.GetComponent<EnemyMergeState>();
            _preMergeMaxHealth = Vitality.MaxHealth;
            _preMergeCurrentHealth = Vitality.CurrentHealth;
            Vitality.Configure(mergedMaxHealth, mergedCurrentHealth);
        }

        public void TakeDamage(int amount)
        {
            if (!IsLinked)
            {
                Vitality.TakeDamage(amount);
                return;
            }

            Vitality.TakeDamage(amount);

            if (IsLinked && Vitality.IsAlive)
                _partner.SyncMergeHealth(Vitality.CurrentHealth);
        }

        public void DieFromCore()
        {
            EnemyMergeState partner = _partner;

            Clear(false);

            if (partner != null)
            {
                partner.Clear(false);
                partner.Vitality.DieFromCore();
            }

            Vitality.DieFromCore();
        }

        public void Clear(bool restoreStats)
        {
            bool wasLinked = IsLinked;
            _partner = null;

            if (restoreStats && wasLinked)
                Vitality.Configure(_preMergeMaxHealth, _preMergeCurrentHealth);
        }

        private void SyncMergeHealth(int currentHealth)
        {
            Vitality.Configure(Vitality.MaxHealth, currentHealth);
        }

        private void OnKilled(EnemyUnit _)
        {
            BreakBecauseOfDeath();
        }

        private void BreakBecauseOfDeath()
        {
            EnemyMergeState partner = _partner;

            Clear(false);

            if (partner != null)
                partner.RestoreAfterBreak();
        }

        private void RestoreAfterBreak()
        {
            Clear(true);
        }
    }
}
