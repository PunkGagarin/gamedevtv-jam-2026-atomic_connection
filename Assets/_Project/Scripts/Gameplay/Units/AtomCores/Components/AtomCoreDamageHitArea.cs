using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.AtomCores.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public class AtomCoreDamageHitArea : MonoBehaviour
    {
        [field: SerializeField] private Collider2D Collider { get; set; }

        private void Awake()
        {
            if (Collider == null)
                Collider = GetComponent<Collider2D>();
        }

        public bool IsOverlapping(Collider2D other)
        {
            if (Collider == null || other == null || !Collider.enabled || !other.enabled)
                return false;

            ColliderDistance2D distance = Collider.Distance(other);
            return distance.isValid && (distance.isOverlapped || distance.distance <= 0f);
        }
    }
}
