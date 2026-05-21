using UnityEngine;

namespace _Project.Scripts.Gameplay.Enemies.Components
{
    public class EnemyMergeLinkView : MonoBehaviour
    {
        [field: SerializeField] private LineRenderer Line { get; set; }

        private Transform _from;
        private Transform _to;
        private float _zOffset;
        private int _pointCount = 2;

        private void Awake()
        {
            if (Line == null)
                Line = GetComponent<LineRenderer>();
        }

        public void Configure(Transform from, Transform to, float width, float zOffset, int intermediatePointCount)
        {
            _from = from;
            _to = to;
            _zOffset = zOffset;
            ConfigureLine(width, intermediatePointCount);
            Tick();
        }

        public void Tick()
        {
            if (_from == null || _to == null || Line == null)
                return;

            Vector3 fromPosition = _from.position;
            Vector3 toPosition = _to.position;
            fromPosition.z += _zOffset;
            toPosition.z += _zOffset;

            for (int i = 0; i < _pointCount; i++)
            {
                float t = _pointCount > 1 ? (float)i / (_pointCount - 1) : 0f;
                Line.SetPosition(i, Vector3.Lerp(fromPosition, toPosition, t));
            }
        }

        private void ConfigureLine(float width, int intermediatePointCount)
        {
            if (Line == null)
                return;

            Line.useWorldSpace = true;
            _pointCount = Mathf.Max(2, intermediatePointCount + 2);
            Line.positionCount = _pointCount;
            float lineWidth = Mathf.Max(0f, width);
            Line.startWidth = lineWidth;
            Line.endWidth = lineWidth;
            Line.enabled = true;
        }
    }
}
