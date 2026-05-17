using System;
using _Project.Scripts.Gameplay.Common.Health;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Enemies
{
    [RequireComponent(typeof(Health))]
    public class EnemyUnit : MonoBehaviour
    {
        [field: SerializeField] private Health Health { get; set; }

        public bool IsAlive => Health == null || Health.IsAlive;

        public event Action<EnemyUnit> Died;

        private void Awake()
        {
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

        public void PrepareForSpawn()
        {
            if (Health != null)
                Health.ResetHealth();

            gameObject.SetActive(true);
        }

        public void PrepareForPool()
        {
            gameObject.SetActive(false);
        }

        public void Kill()
        {
            if (Health != null)
                Health.Kill();
            else
                Died?.Invoke(this);
        }

        private void OnHealthDied()
        {
            Died?.Invoke(this);
        }
    }
}
