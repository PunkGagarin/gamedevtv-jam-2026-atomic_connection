using UnityEngine;

namespace _Project.Scripts.Gameplay.Common.Physics
{
    public readonly struct SquareArea2D
    {
        private const int CLOSED_CORNER_COUNT = 5;

        private readonly Vector3 _center;
        private readonly float _halfSize;

        public SquareArea2D(Vector3 center, float halfSize)
        {
            _center = center;
            _halfSize = Mathf.Max(0f, halfSize);
        }

        public bool IsEmpty => Mathf.Approximately(_halfSize, 0f);

        public bool Contains(Vector3 point)
        {
            return Mathf.Abs(point.x - _center.x) <= _halfSize
                   && Mathf.Abs(point.y - _center.y) <= _halfSize;
        }

        public void WriteClosedCorners(Vector3[] corners)
        {
            if (corners == null || corners.Length < CLOSED_CORNER_COUNT)
                return;

            corners[0] = new Vector3(_center.x - _halfSize, _center.y - _halfSize, _center.z);
            corners[1] = new Vector3(_center.x - _halfSize, _center.y + _halfSize, _center.z);
            corners[2] = new Vector3(_center.x + _halfSize, _center.y + _halfSize, _center.z);
            corners[3] = new Vector3(_center.x + _halfSize, _center.y - _halfSize, _center.z);
            corners[4] = corners[0];
        }
    }
}
