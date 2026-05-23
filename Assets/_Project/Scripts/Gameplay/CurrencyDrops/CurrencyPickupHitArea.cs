using _Project.Scripts.Gameplay.Common.Physics;
using UnityEngine;

namespace _Project.Scripts.Gameplay.CurrencyDrops
{
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(ColliderSet))]
    public class CurrencyPickupHitArea : MonoBehaviour
    {
        [field: SerializeField] private Collider2D Collider { get; set; }
        [field: SerializeField] private ColliderSet Colliders { get; set; }

        private void Awake()
        {
            if (Collider == null)
                Collider = GetComponent<Collider2D>();

            if (Colliders == null)
                Colliders = GetComponent<ColliderSet>();
        }

        public bool IsInside(Vector3 areaCenter, float areaHalfSize)
        {
            Vector3 pickupCenter = Collider != null
                ? Collider.bounds.center
                : transform.position;

            return new SquareArea2D(areaCenter, areaHalfSize).Contains(pickupCenter);
        }

        public void Disable()
        {
            Colliders?.SetEnabled(false);
        }
    }
}
