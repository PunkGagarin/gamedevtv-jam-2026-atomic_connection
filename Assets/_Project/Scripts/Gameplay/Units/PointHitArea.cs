using UnityEngine;

namespace _Project.Scripts.Gameplay.Units
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public class PointHitArea : MonoBehaviour
    {
        [field: SerializeField] private Collider2D Collider { get; set; }

        private void Awake()
        {
            if (Collider == null)
                Collider = GetComponent<Collider2D>();
        }

        public bool Contains(Vector2 worldPosition)
        {
            return Collider != null && Collider.OverlapPoint(worldPosition);
        }
    }
}
