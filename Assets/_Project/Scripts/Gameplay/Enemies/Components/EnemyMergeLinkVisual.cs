using _Project.Scripts.Gameplay.Common.Rendering;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Enemies.Components
{
    public class EnemyMergeLinkVisual : MonoBehaviour
    {
        [field: SerializeField] private LineRenderer Line { get; set; }
        [field: SerializeField] private Color LineColor { get; set; } = new(0.1f, 0.95f, 1f, 1f);

        [Header("Sine Wave")]
        [field: SerializeField] private float WaveAmplitude { get; set; } = 0.3f;
        [field: SerializeField, Range(0.5f, 8f)] private float WaveFrequency { get; set; } = 2f;
        [field: SerializeField] private float WaveSpeed { get; set; } = 1.5f;

        private Transform _from;
        private Transform _to;
        private float _zOffset;
        private int _pointCount = 2;

        private void Awake()
        {
            Line = LineRendererUtility.Ensure(gameObject, Line);
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

            Vector3 direction = (toPosition - fromPosition).normalized;
            Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0f);

            for (int i = 0; i < _pointCount; i++)
            {
                float t = _pointCount > 1 ? (float)i / (_pointCount - 1) : 0f;
                Vector3 basePosition = Vector3.Lerp(fromPosition, toPosition, t);

                if (i > 0 && i < _pointCount - 1)
                {
                    float sineOffset = WaveAmplitude * Mathf.Sin(t * WaveFrequency * Mathf.PI * 2f + Time.time * WaveSpeed);
                    basePosition += perpendicular * sineOffset;
                }

                Line.SetPosition(i, basePosition);
            }
        }

        private void ConfigureLine(float width, int intermediatePointCount)
        {
            if (Line == null)
                return;

            _pointCount = Mathf.Max(2, intermediatePointCount + 2);
            Line.positionCount = _pointCount;
            LineRendererUtility.Configure(Line, width, Line.sortingOrder, LineColor, true);
        }
    }
}
