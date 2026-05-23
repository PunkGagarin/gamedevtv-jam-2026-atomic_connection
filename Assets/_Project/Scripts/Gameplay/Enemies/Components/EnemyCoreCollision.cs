using _Project.Scripts.Gameplay.Units;
using _Project.Scripts.Gameplay.Units.AtomCores;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Enemies.Components
{
    [RequireComponent(typeof(Collider2D))]
    public class EnemyCoreCollision : MonoBehaviour
    {
        [field: SerializeField] private Collider2D Collider { get; set; }

        private EnemyUnit _enemy;
        private AtomCore _core;
        private float _radius;
        private float _coreRadius;

        protected EnemyUnit Enemy => _enemy;
        protected AtomCore Core => _core;

        private void Awake()
        {
            if (Collider == null)
                Collider = GetComponent<Collider2D>();

            _enemy = GetComponent<EnemyUnit>();
        }

        public void Configure(AtomCore core)
        {
            _core = core;
            _radius = ObjectRadius.RadiusOf(transform);
            _coreRadius = core != null ? ObjectRadius.RadiusOf(core.transform) : 0f;
        }

        public void Tick()
        {
            if (_enemy == null || !_enemy.IsAlive || _core == null || Collider == null)
                return;

            if (!IsOverlappingCore())
                return;

            ApplyCoreCollision();
            _enemy.DieFromCore();
        }

        public void Clear()
        {
            _core = null;
            _coreRadius = 0f;
        }

        protected virtual void ApplyCoreCollision()
        {
            _core.TakeDamage(_enemy.CoreCollisionDamage);
        }

        private bool IsOverlappingCore()
        {
            float distance = _radius + _coreRadius;
            if (distance <= 0f)
                return false;

            Vector2 offset = transform.position - _core.transform.position;
            return offset.sqrMagnitude <= distance * distance;
        }
    }
}
