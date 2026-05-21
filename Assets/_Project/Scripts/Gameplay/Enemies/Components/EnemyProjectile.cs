using UnityEngine;
using _Project.Scripts.Gameplay.Units.AtomCores;

namespace _Project.Scripts.Gameplay.Enemies.Components
{
    [RequireComponent(typeof(Collider2D))]
    public class EnemyProjectile : MonoBehaviour
    {
        [field: SerializeField] private Collider2D Collider { get; set; }
        [field: SerializeField, Min(0f)] private float HitRadius { get; set; } = 0.35f;

        private AtomCore _target;
        private Vector3 _direction;
        private float _speed;
        private float _lifetime;
        private int _damage;

        public bool IsActive { get; private set; }

        private void Awake()
        {
            if (Collider == null)
                Collider = GetComponent<Collider2D>();
        }

        public bool Launch(AtomCore target, Vector3 direction, float speed, int damage, float lifetime)
        {
            _target = target;
            _direction = direction.sqrMagnitude > Mathf.Epsilon ? direction.normalized : Vector3.zero;
            _speed = Mathf.Max(0f, speed);
            _damage = Mathf.Max(0, damage);
            _lifetime = Mathf.Max(0f, lifetime);
            IsActive = _target != null && _direction != Vector3.zero && _speed > 0f && _lifetime > 0f;

            if (!IsActive)
            {
                DestroySelf();
                return false;
            }

            if (Collider != null)
                Collider.enabled = true;

            return true;
        }

        public void Tick(float deltaTime)
        {
            if (!IsActive)
                return;

            if (deltaTime <= 0f)
                return;

            if (_target == null || !_target.IsAlive)
            {
                DestroySelf();
                return;
            }

            _lifetime -= deltaTime;
            transform.position += _direction * (_speed * deltaTime);

            if (HasHitTarget())
            {
                _target.TakeDamage(_damage);
                DestroySelf();
                return;
            }

            if (_lifetime <= 0f)
                DestroySelf();
        }

        public void DestroySelf()
        {
            IsActive = false;
            Destroy(gameObject);
        }

        private bool HasHitTarget() =>
            Vector2.Distance(transform.position, _target.transform.position) <= HitRadius;
    }
}
