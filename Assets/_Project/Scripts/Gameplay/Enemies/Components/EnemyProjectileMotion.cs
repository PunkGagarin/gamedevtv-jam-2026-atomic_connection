using _Project.Scripts.Gameplay.Common.Physics;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Enemies.Components
{
    public class EnemyProjectileMotion : MonoBehaviour
    {
        private Vector3 _direction;
        private float _speed;

        public bool CanMove => _direction != Vector3.zero && _speed > 0f;

        private void Awake()
        {
            Rigidbody2DUtility.EnsureKinematicForMovingCollider(gameObject);
        }

        public void Configure(Vector3 direction, float speed)
        {
            _direction = direction.sqrMagnitude > Mathf.Epsilon ? direction.normalized : Vector3.zero;
            _speed = Mathf.Max(0f, speed);
        }

        public void Tick(float deltaTime)
        {
            if (!CanMove || deltaTime <= 0f)
                return;

            transform.position += _direction * (_speed * deltaTime);
        }
    }
}
