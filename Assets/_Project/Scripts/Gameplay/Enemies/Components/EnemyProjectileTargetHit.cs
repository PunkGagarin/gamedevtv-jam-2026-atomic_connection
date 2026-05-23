using UnityEngine;
using _Project.Scripts.Gameplay.Units.AtomCores;
using _Project.Scripts.Gameplay.Units.AtomCores.Components;

namespace _Project.Scripts.Gameplay.Enemies.Components
{
    [RequireComponent(typeof(Collider2D))]
    public class EnemyProjectileTargetHit : MonoBehaviour
    {
        [field: SerializeField] private Collider2D Collider { get; set; }

        public AtomCore Target { get; private set; }
        public bool HasLiveTarget => Target != null && Target.IsAlive;

        private AtomCoreDamageHitArea _targetHitArea;

        private void Awake()
        {
            if (Collider == null)
                Collider = GetComponent<Collider2D>();
        }

        public void Configure(AtomCore target)
        {
            Target = target;
            _targetHitArea = target != null ? target.GetComponent<AtomCoreDamageHitArea>() : null;

            if (Collider != null)
                Collider.enabled = true;
        }

        public bool IsOverlappingTarget()
        {
            return HasLiveTarget && _targetHitArea != null && _targetHitArea.IsOverlapping(Collider);
        }
    }
}
