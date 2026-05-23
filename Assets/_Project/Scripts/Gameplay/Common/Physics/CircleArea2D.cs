using UnityEngine;

namespace _Project.Scripts.Gameplay.Common.Physics
{
    public readonly struct CircleArea2D
    {
        private readonly Vector3 _center;
        private readonly float _radiusSqr;

        public CircleArea2D(Vector3 center, float radius)
        {
            _center = center;
            float clampedRadius = Mathf.Max(0f, radius);
            _radiusSqr = clampedRadius * clampedRadius;
        }

        public bool Contains(Vector3 point)
        {
            Vector2 offset = point - _center;
            return offset.sqrMagnitude <= _radiusSqr;
        }
    }
}
