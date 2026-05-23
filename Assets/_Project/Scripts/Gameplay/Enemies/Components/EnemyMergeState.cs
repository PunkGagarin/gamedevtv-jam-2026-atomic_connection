using UnityEngine;

namespace _Project.Scripts.Gameplay.Enemies.Components
{
    [RequireComponent(typeof(EnemyUnit))]
    [RequireComponent(typeof(EnemyIdentity))]
    [RequireComponent(typeof(EnemyVitality))]
    public class EnemyMergeState : MonoBehaviour
    {
        [field: SerializeField] private EnemyUnit Enemy { get; set; }
        [field: SerializeField] private EnemyIdentity Identity { get; set; }
        [field: SerializeField] private EnemyVitality Vitality { get; set; }

        private EnemyMergeGroup _group;
        private int _preMergeMaxHealth;
        private int _preMergeCurrentHealth;
        private int _preMergeCoreCollisionDamage;

        internal EnemyMergeGroup Group => _group;
        internal bool IsDeathWaveActive => _group != null && _group.IsDeathWaveActive;
        internal int CoreCollisionDamage => _group?.CoreCollisionDamage ?? Identity.CoreCollisionDamage;
        internal int MaxHealthContribution => _group == null ? Vitality.MaxHealth : _preMergeMaxHealth;
        internal int CurrentHealthContribution => _group == null ? Vitality.CurrentHealth : _preMergeCurrentHealth;
        internal int CoreCollisionDamageContribution => _group == null ? Identity.CoreCollisionDamage : _preMergeCoreCollisionDamage;

        private void Awake()
        {
            if (Enemy == null)
                Enemy = GetComponent<EnemyUnit>();

            if (Identity == null)
                Identity = GetComponent<EnemyIdentity>();

            if (Vitality == null)
                Vitality = GetComponent<EnemyVitality>();
        }

        internal void AssignGroup(EnemyMergeGroup group)
        {
            if (group == null)
                return;

            if (_group == null)
            {
                _preMergeMaxHealth = Vitality.MaxHealth;
                _preMergeCurrentHealth = Vitality.CurrentHealth;
                _preMergeCoreCollisionDamage = Identity.CoreCollisionDamage;
            }

            _group = group;
        }

        internal void ClearGroupReference(EnemyMergeGroup group)
        {
            if (_group == group)
                _group = null;
        }

        internal void ClearGroup()
        {
            EnemyMergeGroup group = _group;

            if (group != null && group.IsDeathWaveActive)
            {
                group.ReleaseDeathWaveMember(Enemy);
                return;
            }

            _group = null;

            group?.ClearMembers();
        }

        internal void DieFromCore()
        {
            if (_group != null)
            {
                _group.DieFromCore(Enemy);
                return;
            }

            Vitality.DieFromCore();
        }

        internal void TakeDamage(int amount, float killRewardMultiplier, bool isCritical)
        {
            if (_group != null)
            {
                _group.TakeDamage(amount, Enemy, killRewardMultiplier, isCritical);
                return;
            }

            Vitality.TakeDamage(amount, killRewardMultiplier, isCritical);
        }

        internal void SyncHealth(int maxHealth, int currentHealth)
        {
            Vitality.Configure(maxHealth, currentHealth);
        }

        internal void ApplyDamage(int amount, bool isCritical)
        {
            Vitality.ApplyMergeDamage(amount, isCritical);
        }
    }
}
