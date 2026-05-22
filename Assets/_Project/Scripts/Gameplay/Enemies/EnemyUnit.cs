using System;
using System.Collections.Generic;
using _Project.Scripts.Gameplay.Common.Health;
using _Project.Scripts.Gameplay.Enemies.Components;
using _Project.Scripts.Gameplay.Units.AtomCores;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Enemies
{
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(EnemyMovement))]
    [RequireComponent(typeof(EnemyCoreCollision))]
    public class EnemyUnit : MonoBehaviour
    {
        [field: SerializeField] private Health Health { get; set; }
        [field: SerializeField] private EnemyMovement Movement { get; set; }
        [field: SerializeField] private EnemyCoreCollision CoreCollision { get; set; }

        private readonly List<IEnemyRuntimeBehavior> _runtimeBehaviors = new();
        private EnemyDefinition _definition;
        private int _coreCollisionDamage = 1;
        private EnemyMergeGroup _mergeGroup;
        private int _preMergeMaxHealth;
        private int _preMergeCurrentHealth;
        private int _preMergeCoreCollisionDamage;

        public EnemyId Id => _definition?.Id ?? EnemyId.Standard;
        public bool IsAlive => (Health == null || Health.IsAlive) && (_mergeGroup == null || !_mergeGroup.IsDeathWaveActive);
        public int MaxHealth => Health?.MaxHealth ?? 1;
        public int CurrentHealth => Health?.CurrentHealth ?? MaxHealth;
        public int CoreCollisionDamage => _mergeGroup?.CoreCollisionDamage ?? _coreCollisionDamage;
        public int NucleotideReward => _definition?.NucleotideReward ?? 0;
        public float KillRewardMultiplier { get; private set; } = 1f;

        internal EnemyMergeGroup MergeGroup => _mergeGroup;
        internal bool IsMergeLinkEndpointAlive => Health == null || Health.IsAlive;
        internal int MergeMaxHealthContribution => _mergeGroup == null ? MaxHealth : _preMergeMaxHealth;
        internal int MergeCurrentHealthContribution => _mergeGroup == null ? CurrentHealth : _preMergeCurrentHealth;
        internal int MergeCoreCollisionDamageContribution => _mergeGroup == null ? _coreCollisionDamage : _preMergeCoreCollisionDamage;

        public event Action<EnemyUnit> Died;
        public event Action<EnemyUnit> Killed;

        private void Awake()
        {
            if (Health == null)
                Health = GetComponent<Health>();

            if (Movement == null)
                Movement = GetComponent<EnemyMovement>();

            if (CoreCollision == null)
                CoreCollision = GetComponent<EnemyCoreCollision>();

            _runtimeBehaviors.Clear();
            _runtimeBehaviors.AddRange(GetComponents<IEnemyRuntimeBehavior>());

            if (Health != null)
                Health.Died += OnHealthDied;
        }

        private void OnDestroy()
        {
            if (Health != null)
                Health.Died -= OnHealthDied;
        }

        public void Configure(EnemyDefinition definition, int maxHealth, int coreCollisionDamage)
        {
            _definition = definition ?? throw new ArgumentNullException(nameof(definition));
            _coreCollisionDamage = Mathf.Max(1, coreCollisionDamage);
            ClearMergeGroup();

            if (Health != null)
                Health.Configure(maxHealth);
        }

        public void PrepareForSpawn()
        {
            ClearMergeGroup();
            Movement?.Clear();
            CoreCollision?.Clear();
            ClearRuntimeBehaviors();
            KillRewardMultiplier = 1f;

            if (Health != null)
                Health.ResetHealth();

            gameObject.SetActive(true);
        }

        public void PrepareForPool()
        {
            ClearMergeGroup();
            Movement?.Clear();
            CoreCollision?.Clear();
            ClearRuntimeBehaviors();
            KillRewardMultiplier = 1f;
            gameObject.SetActive(false);
        }

        public void MoveTo(Transform target, float speed)
        {
            Movement?.Configure(target, speed);
            ConfigureRuntimeBehaviors(target);
        }

        public void MoveTo(Transform target, float speed, float groupMovementSign)
        {
            Movement?.ConfigureGroupMovement(groupMovementSign);
            Movement?.Configure(target, speed);
            ConfigureRuntimeBehaviors(target);
        }

        public void CollideWithCore(AtomCore target)
        {
            CoreCollision?.Configure(target);
        }

        public void TickMovement(float deltaTime)
        {
            Movement?.Tick(deltaTime);
        }

        public void TickRuntimeBehaviors(float deltaTime)
        {
            foreach (IEnemyRuntimeBehavior behavior in _runtimeBehaviors)
                behavior.Tick(deltaTime);
        }

        public void TickCoreCollision()
        {
            CoreCollision?.Tick();
        }

        public void Kill()
        {
            if (_mergeGroup != null)
            {
                _mergeGroup.DieFromMemberKill(this);
                return;
            }

            if (Health != null)
                Health.Kill();
            else
                Died?.Invoke(this);
        }

        public void DieFromCore()
        {
            if (_mergeGroup != null)
            {
                _mergeGroup.DieFromCore(this);
                return;
            }

            DieFromCoreSingle();
        }

        public void TakeDamage(int amount)
        {
            TakeDamage(amount, 1f, false);
        }

        public void TakeDamage(int amount, float killRewardMultiplier, bool isCritical)
        {
            if (_mergeGroup != null)
            {
                _mergeGroup.TakeDamage(amount, this, killRewardMultiplier, isCritical);
                return;
            }

            KillRewardMultiplier = Mathf.Max(0f, killRewardMultiplier);

            if (Health != null)
                Health.TakeDamage(amount, isCritical);
            else
                Kill();

            if (IsAlive)
                KillRewardMultiplier = 1f;
        }

        internal void AssignMergeGroup(EnemyMergeGroup mergeGroup)
        {
            if (mergeGroup == null)
                return;

            if (_mergeGroup == null)
            {
                _preMergeMaxHealth = MaxHealth;
                _preMergeCurrentHealth = CurrentHealth;
                _preMergeCoreCollisionDamage = _coreCollisionDamage;
            }

            _mergeGroup = mergeGroup;
        }

        internal void ClearMergeGroupReference(EnemyMergeGroup mergeGroup)
        {
            if (_mergeGroup == mergeGroup)
                _mergeGroup = null;
        }

        internal void SyncMergeHealth(int maxHealth, int currentHealth)
        {
            if (Health != null)
                Health.Configure(maxHealth, currentHealth);
        }

        internal void ApplyMergeDamage(int amount, bool isCritical)
        {
            if (Health == null)
                return;

            Health.Died -= OnHealthDied;
            Health.TakeDamage(amount, isCritical);
            Health.Died += OnHealthDied;
        }

        internal void DieFromMergeGroupDamage(float killRewardMultiplier)
        {
            DieFromMergeGroupSingle(true, killRewardMultiplier);
        }

        internal void DieFromMergeGroupCore()
        {
            DieFromMergeGroupSingle(false);
        }

        private void DieFromMergeGroupSingle(bool killedByPlayer, float killRewardMultiplier = 1f)
        {
            KillRewardMultiplier = Mathf.Max(0f, killRewardMultiplier);

            if (Health != null)
            {
                Health.Died -= OnHealthDied;
                Health.Kill();
                Health.Died += OnHealthDied;
            }

            if (killedByPlayer)
                Killed?.Invoke(this);

            Died?.Invoke(this);
        }

        private void DieFromCoreSingle()
        {
            KillRewardMultiplier = 1f;

            if (Health != null)
            {
                Health.Died -= OnHealthDied;
                Health.Kill();
                Health.Died += OnHealthDied;
            }

            Died?.Invoke(this);
        }

        private void ConfigureRuntimeBehaviors(Transform target)
        {
            foreach (IEnemyRuntimeBehavior behavior in _runtimeBehaviors)
                behavior.Configure(target);
        }

        private void ClearRuntimeBehaviors()
        {
            foreach (IEnemyRuntimeBehavior behavior in _runtimeBehaviors)
                behavior.Clear();
        }

        private void OnHealthDied()
        {
            if (_mergeGroup != null)
            {
                _mergeGroup.DieFromMemberKill(this);
                return;
            }

            Killed?.Invoke(this);
            Died?.Invoke(this);
        }

        private void ClearMergeGroup()
        {
            EnemyMergeGroup mergeGroup = _mergeGroup;

            if (mergeGroup != null && mergeGroup.IsDeathWaveActive)
            {
                mergeGroup.ReleaseDeathWaveMember(this);
                return;
            }

            _mergeGroup = null;

            mergeGroup?.ClearMembers();
        }
    }
}
