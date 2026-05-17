using System;
using _Project.Scripts.Gameplay.Common.Health;
using _Project.Scripts.Gameplay.Enemies.Components;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Enemies
{
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(EnemyMovement))]
    public class EnemyUnit : MonoBehaviour
    {
        [field: SerializeField] private Health Health { get; set; }
        [field: SerializeField] private EnemyMovement Movement { get; set; }

        public bool IsAlive => Health == null || Health.IsAlive;

        public event Action<EnemyUnit> Died;
        public event Action<EnemyUnit> Killed;

        private void Awake()
        {
            if (Health == null)
                Health = GetComponent<Health>();

            if (Movement == null)
                Movement = GetComponent<EnemyMovement>();

            if (Health != null)
                Health.Died += OnHealthDied;
        }

        private void OnDestroy()
        {
            if (Health != null)
                Health.Died -= OnHealthDied;
        }

        public void PrepareForSpawn()
        {
            Movement?.Clear();

            if (Health != null)
                Health.ResetHealth();

            gameObject.SetActive(true);
        }

        public void PrepareForPool()
        {
            Movement?.Clear();
            gameObject.SetActive(false);
        }

        public void MoveTo(Transform target, float speed)
        {
            Movement?.Configure(target, speed);
        }

        public void Tick(float deltaTime)
        {
            Movement?.Tick(deltaTime);
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
