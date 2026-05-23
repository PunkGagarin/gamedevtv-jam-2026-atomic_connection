using UnityEngine;

namespace _Project.Scripts.Gameplay.Enemies.Components
{
    public class EnemyKnockback : MonoBehaviour
    {
        private const float MIN_DIRECTION_SQR_MAGNITUDE = 0.0001f;

        private Vector3 _startPosition;
        private Vector3 _targetPosition;
        private float _duration;
        private float _elapsed;

        public void PushFrom(Vector3 origin, float distance, float duration)
        {
            if (distance <= 0f)
                return;

            Vector3 direction = transform.position - origin;

            if (direction.sqrMagnitude <= MIN_DIRECTION_SQR_MAGNITUDE)
                direction = Vector3.up;

            _startPosition = transform.position;
            _targetPosition = _startPosition + direction.normalized * distance;
            _duration = Mathf.Max(0.01f, duration);
            _elapsed = 0f;
        }

        public void Clear()
        {
            _duration = 0f;
            _elapsed = 0f;
        }

        public void Tick(float deltaTime)
        {
            if (_elapsed >= _duration)
                return;

            _elapsed = Mathf.Min(_duration, _elapsed + Mathf.Max(0f, deltaTime));
            float normalized = _elapsed / _duration;
            float eased = Mathf.SmoothStep(0f, 1f, normalized);

            transform.position = Vector3.Lerp(_startPosition, _targetPosition, eased);
        }
    }
}
