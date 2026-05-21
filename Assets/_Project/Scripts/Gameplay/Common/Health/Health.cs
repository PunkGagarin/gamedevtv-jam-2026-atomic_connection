using System;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Common.Health
{
    public class Health : MonoBehaviour
    {
        public int MaxHealth { get; private set; } = 1;

        public int CurrentHealth { get; private set; }
        public bool IsAlive { get; private set; } = true;

        public event Action Changed;
        public event Action<int> Damaged;
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

        public void Configure(int maxHealth, int currentHealth)
        {
            MaxHealth = Mathf.Max(1, maxHealth);
            CurrentHealth = Mathf.Clamp(currentHealth, 1, MaxHealth);
            IsAlive = true;
            Changed?.Invoke();
        }

        public void TakeDamage(int amount)
        {
            if (!IsAlive || amount <= 0)
                return;

            int previousHealth = CurrentHealth;
            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            int actualDamage = previousHealth - CurrentHealth;

            if (actualDamage > 0)
                Damaged?.Invoke(actualDamage);

            if (CurrentHealth == 0)
                Kill();
            else
                Changed?.Invoke();
        }

        public void Kill()
        {
            if (!IsAlive)
                return;

            CurrentHealth = 0;
            IsAlive = false;
            Changed?.Invoke();
            Died?.Invoke();
        }

        public void ResetHealth()
        {
            CurrentHealth = MaxHealth;
            IsAlive = true;
            Changed?.Invoke();
        }
    }
}
