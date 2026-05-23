using UnityEngine;
using _Project.Scripts.Gameplay.Common.Physics;
using _Project.Scripts.Gameplay.Units.AtomCores;

namespace _Project.Scripts.Gameplay.Enemies.Components
{
    [RequireComponent(typeof(Collider2D))]
    public class EnemyProjectileTargetHit : MonoBehaviour
    {
        [field: SerializeField] private Collider2D Collider { get; set; }
        [field: SerializeField, Min(0f)] private float HitRadius { get; set; } = 0.35f;

        public AtomCore Target { get; private set; }
        public bool HasLiveTarget => Target != null && Target.IsAlive;

        private void Awake()
        {
            if (Collider == null)
                Collider = GetComponent<Collider2D>();
        }

        public void Configure(AtomCore target)
        {
            Target = target;

            if (Collider != null)
                Collider.enabled = true;
        }

        public bool Contains(Vector3 worldPosition)
        {
            return HasLiveTarget && new CircleArea2D(worldPosition, HitRadius).Contains(Target.transform.position);
        }
    }
}
