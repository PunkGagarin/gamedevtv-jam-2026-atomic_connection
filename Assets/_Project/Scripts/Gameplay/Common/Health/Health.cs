using System;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Common.Health
{
    public class Health : MonoBehaviour
    {
        public bool IsAlive { get; private set; } = true;

        public event Action Died;

        public void Kill()
        {
            if (!IsAlive)
                return;

            IsAlive = false;
            Died?.Invoke();
        }

        public void ResetHealth()
        {
            IsAlive = true;
        }
    }
}
