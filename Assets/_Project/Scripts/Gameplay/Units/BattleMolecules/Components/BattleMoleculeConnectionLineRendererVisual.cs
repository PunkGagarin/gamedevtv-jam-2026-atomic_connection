using UnityEngine;

using _Project.Scripts.Gameplay.Common.Rendering;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(LineRenderer))]
    public class BattleMoleculeConnectionLineRendererVisual : BattleMoleculeConnectionLineVisual
    {
        [field: SerializeField] private LineRenderer Line { get; set; }

        [field: SerializeField] private int SegmentCount { get; set; } = 12;
        [field: SerializeField] private float Amplitude { get; set; } = 0.3f;
        [field: SerializeField] private float Frequency { get; set; } = 2f;
        [field: SerializeField] private float AnimationSpeed { get; set; } = 1.5f;
        [field: SerializeField] private AnimationCurve WidthCurve { get; set; } = AnimationCurve.Linear(0f, 1f, 1f, 1f);

        internal override bool IsVisible => Line != null && Line.enabled;

        private void Awake()
        {
            Line = LineRendererUtility.Ensure(gameObject, Line);
            SetVisible(false);
        }

        internal override void Configure(float width, int sortingOrder, Color color)
        {
            if (Line == null)
                return;

            int pointCount = SegmentCount + 2;
            Line.positionCount = pointCount;
            LineRendererUtility.Configure(Line, width, sortingOrder, color, Line.enabled);

            Line.widthCurve = WidthCurve;
            Line.widthMultiplier = Mathf.Max(0f, width);
        }

        internal override void SetEndpoints(Vector3 from, Vector3 to)
        {
            if (Line == null)
                return;

            int pointCount = SegmentCount + 2;
            if (Line.positionCount != pointCount)
                Line.positionCount = pointCount;

            Vector3 direction = to - from;
            float distance = direction.magnitude;

            if (distance < 0.001f)
            {
                for (int i = 0; i < pointCount; i++)
                    Line.SetPosition(i, from);
                return;
            }

            Vector3 normalizedDir = direction / distance;
            Vector3 perpendicular = Vector3.Cross(normalizedDir, Vector3.forward).normalized;

            if (perpendicular.sqrMagnitude < 0.001f)
                perpendicular = Vector3.up;

            float timeOffset = Time.time * AnimationSpeed;

            for (int i = 0; i < pointCount; i++)
            {
                float t = (float)i / (pointCount - 1);
                Vector3 basePoint = Vector3.Lerp(from, to, t);

                float weight = Mathf.Sin(t * Mathf.PI);
                float sineOffset = Mathf.Sin(t * Mathf.PI * 2f * Frequency + timeOffset) * Amplitude * weight;
                Vector3 point = basePoint + perpendicular * sineOffset;

                Line.SetPosition(i, point);
            }
        }

        internal override void SetColor(Color color)
        {
            LineRendererUtility.SetColor(Line, color);
        }

        internal override void SetVisible(bool isVisible)
        {
            if (Line != null)
                Line.enabled = isVisible;
        }
    }
}
