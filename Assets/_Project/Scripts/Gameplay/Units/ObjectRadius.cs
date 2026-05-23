using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units
{
    [DisallowMultipleComponent]
    public class ObjectRadius : MonoBehaviour
    {
        private readonly List<SpriteRenderer> _spriteRenderers = new();
        private float _radius;
        private Vector3 _lastLossyScale;
        private Quaternion _lastRotation;
        private bool _isDirty = true;

        [field: SerializeField] private Collider2D Collider { get; set; }
        [field: SerializeField] private bool IncludeChildRenderers { get; set; } = true;

        public float Radius
        {
            get
            {
                RefreshIfDirty();
                return _radius;
            }
        }

        private void Awake()
        {
            if (Collider == null)
                Collider = GetComponent<Collider2D>();

            Refresh();
        }

        private void OnTransformChildrenChanged()
        {
            _isDirty = true;
        }

        private void RefreshIfDirty()
        {
            if (!_isDirty && HasTransformGeometryChanged())
                _isDirty = true;

            if (_isDirty)
                Refresh();
        }

        private void Refresh()
        {
            _spriteRenderers.Clear();

            if (IncludeChildRenderers)
                GetComponentsInChildren(true, _spriteRenderers);

            _radius = CalculateRadius();
            _lastLossyScale = transform.lossyScale;
            _lastRotation = transform.rotation;
            _isDirty = false;
        }

        private bool HasTransformGeometryChanged()
        {
            return transform.lossyScale != _lastLossyScale
                   || transform.rotation != _lastRotation;
        }

        private float CalculateRadius()
        {
            float radius = Collider != null
                ? RadiusFromBounds(transform, Collider.bounds)
                : 0f;

            foreach (SpriteRenderer spriteRenderer in _spriteRenderers)
            {
                if (spriteRenderer == null)
                    continue;

                radius = Mathf.Max(radius, RadiusFromBounds(transform, spriteRenderer.bounds));
            }

            return radius;
        }

        private static float RadiusFromBounds(Transform center, Bounds bounds)
        {
            Vector3 offset = bounds.center - center.position;
            offset.z = 0f;
            Vector3 extents = bounds.extents;
            return offset.magnitude + Mathf.Max(extents.x, extents.y);
        }
    }
}
