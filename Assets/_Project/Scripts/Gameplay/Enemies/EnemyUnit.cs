using System;
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

        private EnemyDefinition _definition;
        private int _coreCollisionDamage = 1;

        public EnemyId Id => _definition?.Id ?? EnemyId.Standard;
        public bool IsAlive => Health == null || Health.IsAlive;
        public int CoreCollisionDamage => _coreCollisionDamage;
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

            if (Health != null)
                Health.Configure(maxHealth);
        }

        public void PrepareForSpawn()
        {
            Movement?.Clear();
            CoreCollision?.Clear();

            if (Health != null)
                Health.ResetHealth();

            gameObject.SetActive(true);
        }

        public void PrepareForPool()
        {
            Movement?.Clear();
            CoreCollision?.Clear();
            gameObject.SetActive(false);
        }

        public void MoveTo(Transform target, float speed)
        {
            Movement?.Configure(target, speed);
        }

        public void CollideWithCore(AtomCore target)
        {
            CoreCollision?.Configure(target);
        }

        public void TickMovement(float deltaTime)
        {
            Movement?.Tick(deltaTime);
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
            if (Health != null)
            {
                Health.Died -= OnHealthDied;
                Health.Kill();
                Health.Died += OnHealthDied;
            }

            Died?.Invoke(this);
        }

        public void TakeDamage(int amount)
        {
            if (Health != null)
                Health.TakeDamage(amount);
            else
                Kill();
        }

        private void OnHealthDied()
        {
            Killed?.Invoke(this);
            Died?.Invoke(this);
        }
    }
}
