using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.AtomCores.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public class AtomCoreDamageHitArea : MonoBehaviour
    {
        [field: SerializeField] public Collider2D Collider { get; private set; }

        private void Awake()
        {
            if (Collider == null)
                Collider = GetComponent<Collider2D>();
        }

        public bool IsOverlapping(Collider2D other)
        {
            return GetOverlapInfo(other, out _);
        }

        public bool GetOverlapInfo(Collider2D other, out ColliderDistance2D distance)
        {
            if (Collider == null || other == null || !Collider.enabled || !other.enabled)
            {
                distance = default;
                return false;
            }

            distance = Collider.Distance(other);
            return distance.isValid && (distance.isOverlapped || distance.distance <= 0f);
        }
    }
}
