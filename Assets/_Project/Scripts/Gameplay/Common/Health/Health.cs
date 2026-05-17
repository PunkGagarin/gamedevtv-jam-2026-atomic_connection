using System;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Common.Health
{
    public class Health : MonoBehaviour
    {
        public int MaxHealth { get; private set; } = 1;

        public int CurrentHealth { get; private set; }
        public bool IsAlive { get; private set; } = true;

        public event Action Died;

        private void Awake()
        {
            ResetHealth();
        }

        public void Configure(int maxHealth)
        {
            MaxHealth = Mathf.Max(1, maxHealth);
            ResetHealth();
        }

        public void TakeDamage(int amount)
        {
            if (!IsAlive || amount <= 0)
                return;

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);

            if (CurrentHealth == 0)
                Kill();
        }

        public void Kill()
        {
            if (!IsAlive)
                return;

            CurrentHealth = 0;
            IsAlive = false;
            Died?.Invoke();
        }

        public void ResetHealth()
        {
            CurrentHealth = MaxHealth;
            IsAlive = true;
        }
    }
}
