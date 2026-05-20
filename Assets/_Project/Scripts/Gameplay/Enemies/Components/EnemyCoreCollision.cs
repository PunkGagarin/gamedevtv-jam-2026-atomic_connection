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
        private Collider2D _coreCollider;

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
            _coreCollider = core != null ? core.GetComponent<Collider2D>() : null;
        }

        public void Tick()
        {
            if (_enemy == null || !_enemy.IsAlive || _core == null || _coreCollider == null || Collider == null)
                return;

            if (!Collider.Distance(_coreCollider).isOverlapped)
                return;

            ApplyCoreCollision();
            _enemy.DieFromCore();
        }

        public void Clear()
        {
            _core = null;
            _coreCollider = null;
        }

        protected virtual void ApplyCoreCollision()
        {
            _core.TakeDamage(_enemy.CoreCollisionDamage);
        }
    }
}
