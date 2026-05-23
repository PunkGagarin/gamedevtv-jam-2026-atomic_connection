using _Project.Scripts.Gameplay.Units.AtomCores;
using _Project.Scripts.Gameplay.Units.AtomCores.Components;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Enemies.Components
{
    [RequireComponent(typeof(Collider2D))]
    public class EnemyCoreCollision : MonoBehaviour
    {
        [field: SerializeField] private Collider2D Collider { get; set; }

        private EnemyUnit _enemy;
        private AtomCore _core;
        private AtomCoreDamageHitArea _coreHitArea;

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
            _coreHitArea = core != null ? core.GetComponent<AtomCoreDamageHitArea>() : null;
        }

        public void Tick()
        {
            if (_enemy == null || !_enemy.IsAlive || _core == null || Collider == null || _coreHitArea == null)
                return;

            if (!IsOverlappingCore())
                return;

            ApplyCoreCollision();
            _enemy.DieFromCore();
        }

        public void Clear()
        {
            _core = null;
            _coreHitArea = null;
        }

        protected virtual void ApplyCoreCollision()
        {
            _core.TakeDamage(_enemy.CoreCollisionDamage);
        }

        private bool IsOverlappingCore()
        {
            return _coreHitArea.IsOverlapping(Collider);
        }
    }
}
