using UnityEngine;

namespace _Project.Scripts.Gameplay.Enemies.Components
{
    public class EnemyProjectileLifetime : MonoBehaviour
    {
        private float _remainingSeconds;

        public bool HasTime => _remainingSeconds > 0f;
        public bool IsExpired => _remainingSeconds <= 0f;

        public void Configure(float lifetime)
        {
            _remainingSeconds = Mathf.Max(0f, lifetime);
        }

        public void Tick(float deltaTime)
        {
            _remainingSeconds -= Mathf.Max(0f, deltaTime);
        }
    }
}
