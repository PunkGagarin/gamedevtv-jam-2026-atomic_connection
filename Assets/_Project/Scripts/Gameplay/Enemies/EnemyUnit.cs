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
        private EnemyUnit _mergePartner;
        private EnemyMergeLinkView _mergeLinkView;
        private int _preMergeMaxHealth;
        private int _preMergeCurrentHealth;
        private int _preMergeCoreCollisionDamage;

        public EnemyId Id => _definition?.Id ?? EnemyId.Standard;
        public bool IsAlive => Health == null || Health.IsAlive;
        public bool IsMergeLinked => _mergePartner != null;
        public int MaxHealth => Health?.MaxHealth ?? 1;
        public int CurrentHealth => Health?.CurrentHealth ?? MaxHealth;
        public int CoreCollisionDamage => _coreCollisionDamage + (_mergePartner != null ? _mergePartner._coreCollisionDamage : 0);
        public int NucleotideReward => _definition?.NucleotideReward ?? 0;

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
            ClearMergeLink(false, true);

            if (Health != null)
                Health.Configure(maxHealth);
        }

        public void PrepareForSpawn()
        {
            ClearMergeLink(false, true);
            Movement?.Clear();
            CoreCollision?.Clear();
            ClearRuntimeBehaviors();

            if (Health != null)
                Health.ResetHealth();

            gameObject.SetActive(true);
        }

        public void PrepareForPool()
        {
            ClearMergeLink(false, true);
            Movement?.Clear();
            CoreCollision?.Clear();
            ClearRuntimeBehaviors();
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
            if (Health != null)
                Health.Kill();
            else
                Died?.Invoke(this);
        }

        public void DieFromCore()
        {
            EnemyUnit partner = _mergePartner;
            EnemyMergeLinkView linkView = _mergeLinkView;
            ClearMergeLink(false, false);

            if (partner != null)
            {
                partner.ClearMergeLink(false, false);
                partner.DieFromCoreSingle();
            }

            if (linkView != null)
                Destroy(linkView.gameObject);

            DieFromCoreSingle();
        }

        public void TakeDamage(int amount)
        {
            if (_mergePartner != null)
            {
                TakeMergedDamage(amount);
                return;
            }

            if (Health != null)
                Health.TakeDamage(amount);
            else
                Kill();
        }

        public void BeginMergeLink(EnemyUnit partner, EnemyMergeLinkView linkView, int mergedMaxHealth, int mergedCurrentHealth)
        {
            if (partner == null)
                return;

            _mergePartner = partner;
            _mergeLinkView = linkView;
            _preMergeMaxHealth = MaxHealth;
            _preMergeCurrentHealth = CurrentHealth;
            _preMergeCoreCollisionDamage = _coreCollisionDamage;

            if (Health != null)
                Health.Configure(mergedMaxHealth, mergedCurrentHealth);
        }

        public void RestoreAfterMergeBreak()
        {
            ClearMergeLink(true, false);
        }

        public void ClearMergeLinkView()
        {
            _mergeLinkView = null;
        }

        private void TakeMergedDamage(int amount)
        {
            if (Health == null)
            {
                Kill();
                return;
            }

            Health.TakeDamage(amount);

            if (_mergePartner != null && Health.IsAlive)
                _mergePartner.SyncMergeHealth(CurrentHealth);
        }

        private void SyncMergeHealth(int currentHealth)
        {
            if (Health != null)
                Health.Configure(MaxHealth, currentHealth);
        }

        private void DieFromCoreSingle()
        {
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
            BreakMergeBecauseOfDeath();
            Killed?.Invoke(this);
            Died?.Invoke(this);
        }

        private void BreakMergeBecauseOfDeath()
        {
            EnemyUnit partner = _mergePartner;
            EnemyMergeLinkView linkView = _mergeLinkView;
            ClearMergeLink(false, false);

            if (partner != null)
                partner.RestoreAfterMergeBreak();

            if (linkView != null)
                Destroy(linkView.gameObject);
        }

        private void ClearMergeLink(bool restoreStats, bool destroyView)
        {
            EnemyMergeLinkView linkView = _mergeLinkView;
            _mergePartner = null;
            _mergeLinkView = null;

            if (restoreStats)
            {
                _coreCollisionDamage = Mathf.Max(1, _preMergeCoreCollisionDamage);

                if (Health != null)
                    Health.Configure(_preMergeMaxHealth, _preMergeCurrentHealth);
            }

            if (destroyView && linkView != null)
                Destroy(linkView.gameObject);
        }
    }
}
