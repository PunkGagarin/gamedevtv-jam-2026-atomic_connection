using System;
using _Project.Scripts.Gameplay.Enemies.Components;
using _Project.Scripts.Gameplay.Units.AtomCores;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Enemies
{
    [RequireComponent(typeof(EnemyIdentity))]
    [RequireComponent(typeof(EnemyVitality))]
    [RequireComponent(typeof(EnemyMergeState))]
    [RequireComponent(typeof(EnemyRuntimeBehaviors))]
    [RequireComponent(typeof(EnemyLifecycle))]
    [RequireComponent(typeof(EnemyMovement))]
    [RequireComponent(typeof(EnemyCoreCollision))]
    public class EnemyUnit : MonoBehaviour
    {
        [field: SerializeField] private EnemyIdentity Identity { get; set; }
        [field: SerializeField] private EnemyVitality Vitality { get; set; }
        [field: SerializeField] private EnemyMergeState Merge { get; set; }
        [field: SerializeField] private EnemyRuntimeBehaviors RuntimeBehaviors { get; set; }
        [field: SerializeField] private EnemyLifecycle Lifecycle { get; set; }
        [field: SerializeField] private EnemyMovement Movement { get; set; }
        [field: SerializeField] private EnemyCoreCollision CoreCollision { get; set; }

        public EnemyId Id => Identity.Id;
        public bool IsAlive => Vitality.IsAlive;
        public bool IsMergeLinked => Merge.IsLinked;
        public int MaxHealth => Vitality.MaxHealth;
        public int CurrentHealth => Vitality.CurrentHealth;
        public int CoreCollisionDamage => Merge.CoreCollisionDamage;
        public int NucleotideReward => Identity.NucleotideReward;

        public event Action<EnemyUnit> Died
        {
            add => Vitality.Died += value;
            remove => Vitality.Died -= value;
        }

        public event Action<EnemyUnit> Killed
        {
            add => Vitality.Killed += value;
            remove => Vitality.Killed -= value;
        }

        private void Awake()
        {
            if (Identity == null)
                Identity = GetComponent<EnemyIdentity>();

            if (Vitality == null)
                Vitality = GetComponent<EnemyVitality>();

            if (Merge == null)
                Merge = GetComponent<EnemyMergeState>();

            if (RuntimeBehaviors == null)
                RuntimeBehaviors = GetComponent<EnemyRuntimeBehaviors>();

            if (Lifecycle == null)
                Lifecycle = GetComponent<EnemyLifecycle>();

            if (Movement == null)
                Movement = GetComponent<EnemyMovement>();

            if (CoreCollision == null)
                CoreCollision = GetComponent<EnemyCoreCollision>();
        }

        public void Configure(EnemyDefinition definition, int maxHealth, int coreCollisionDamage)
        {
            Identity.Configure(definition, coreCollisionDamage);
            Merge.Clear(false);
            Vitality.Configure(maxHealth);
        }

        public void PrepareForSpawn()
        {
            Lifecycle.PrepareForSpawn();
        }

        public void PrepareForPool()
        {
            Lifecycle.PrepareForPool();
        }

        public void MoveTo(Transform target, float speed, float groupMovementSign)
        {
            Movement?.ConfigureGroupMovement(groupMovementSign);
            Movement?.Configure(target, speed);
            RuntimeBehaviors.Configure(target);
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
            RuntimeBehaviors.Tick(deltaTime);
        }

        public void TickCoreCollision()
        {
            CoreCollision?.Tick();
        }

        public void DieFromCore()
        {
            Merge.DieFromCore();
        }

        public void TakeDamage(int amount)
        {
            Merge.TakeDamage(amount);
        }

        public void BeginMergeLink(EnemyUnit partner, int mergedMaxHealth, int mergedCurrentHealth)
        {
            Merge.BeginLink(partner, mergedMaxHealth, mergedCurrentHealth);
        }
    }
}
