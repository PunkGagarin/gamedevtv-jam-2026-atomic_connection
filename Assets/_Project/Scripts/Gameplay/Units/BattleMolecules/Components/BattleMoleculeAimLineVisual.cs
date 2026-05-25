using System.Collections.Generic;
using _Project.Scripts.Gameplay.Common.Rendering;
using _Project.Scripts.Gameplay.Units.BattleMolecules;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    public class BattleMoleculeAimLineVisual : MonoBehaviour
    {
        private const string AIM_PREVIEW_LINE_OBJECT_NAME = "AimPreviewLine";
        private const string SHOT_LINE_OBJECT_NAME = "ShotLine";

        [field: SerializeField] private bool DragLineEnabled { get; set; } = true;
        [field: SerializeField, Min(0f)] private float AimLineWidth { get; set; } = 0.08f;
        [field: SerializeField, Min(0f)] private float AimPreviewLineWidth { get; set; } = 0.06f;
        [field: SerializeField, Min(0f)] private float AimPreviewLineLength { get; set; } = 20f;
        [field: SerializeField, Min(0f)] private float ShotLineWidth { get; set; } = 0.12f;
        [field: SerializeField, Min(0f)] private float ShotLineLength { get; set; } = 20f;
        [field: SerializeField, Min(0.01f)] private float ShotLineSeconds { get; set; } = 0.12f;
        [field: SerializeField] private int AimLineSortingOrder { get; set; } = 10;
        [field: SerializeField] private int AimPreviewLineSortingOrder { get; set; } = 12;
        [field: SerializeField] private int ShotLineSortingOrder { get; set; } = 11;
        [field: SerializeField] private Color AimLineColor { get; set; } = Color.yellow;
        [field: SerializeField] private Color AimPreviewLineColor { get; set; } = Color.green;
        [field: SerializeField] private Color ShotLineColor { get; set; } = Color.cyan;

        [field: SerializeField, Min(2)] private int PreviewWobbleSegmentCount { get; set; } = 14;
        [field: SerializeField, Min(0f)] private float PreviewWobbleAmplitude { get; set; } = 1.5f;
        [field: SerializeField, Min(0f)] private float PreviewWobbleFrequency { get; set; } = 3f;
        [field: SerializeField, Min(0.01f)] private float PreviewWobbleDuration { get; set; } = 0.4f;
        [field: SerializeField, Min(0f)] private float PreviewWobbleAnimationSpeed { get; set; } = 4f;

        [field: SerializeField] private AnimationCurve WidthCurve { get; set; } = AnimationCurve.Linear(0f, 1f, 1f, 1f);

        private readonly List<LineRenderer> _shotLines = new();
        private readonly List<float> _shotLineTimesLeft = new();
        private LineRenderer _aimLine;
        private LineRenderer _aimPreviewLine;
        private Vector3 _origin;
        private float _previewWobbleTimeLeft;
        private int _previewPointCount;

        private void Awake()
        {
            _previewPointCount = PreviewWobbleSegmentCount + 2;
            _aimLine = SetupLine(gameObject, AimLineWidth, AimLineSortingOrder, AimLineColor, 2);
            _aimPreviewLine = SetupLine(
                GetOrCreateLineObject(AIM_PREVIEW_LINE_OBJECT_NAME),
                AimPreviewLineWidth,
                AimPreviewLineSortingOrder,
                AimPreviewLineColor,
                _previewPointCount);
            CreateShotLine();
        }

        private void OnDisable()
        {
            Hide();

            for (int i = 0; i < _shotLines.Count; i++)
            {
                _shotLineTimesLeft[i] = 0f;

                if (_shotLines[i] != null)
                    _shotLines[i].enabled = false;
            }

            HideAimPreview();
        }

        public void Tick(float deltaTime)
        {
            for (int i = 0; i < _shotLines.Count; i++)
            {
                LineRenderer shotLine = _shotLines[i];
                if (shotLine == null || _shotLineTimesLeft[i] <= 0f)
                    continue;

                _shotLineTimesLeft[i] -= deltaTime;
                if (_shotLineTimesLeft[i] <= 0f)
                {
                    shotLine.enabled = false;
                    continue;
                }

                LineRendererUtility.SetColor(shotLine, ShotLineColor, _shotLineTimesLeft[i] / CurrentShotLineSeconds());
            }

            if (_previewWobbleTimeLeft > 0f)
            {
                _previewWobbleTimeLeft -= deltaTime;
                if (_previewWobbleTimeLeft < 0f)
                    _previewWobbleTimeLeft = 0f;
            }
        }

        public void Show(Vector3 origin)
        {
            _origin = origin;

            if (!DragLineEnabled)
            {
                Hide();
                return;
            }

            SetEnd(origin);

            if (_aimLine != null)
                _aimLine.enabled = true;
        }

        public void SetSegment(Vector3 origin, Vector3 end)
        {
            _origin = origin;
            if (!DragLineEnabled)
                return;

            SetEnd(end);
        }

        public void Hide()
        {
            if (_aimLine != null)
                _aimLine.enabled = false;
        }

        public void ShowAimPreview(Vector3 origin, Vector3 direction)
        {
            if (_aimPreviewLine == null)
                return;

            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                HideAimPreview();
                return;
            }

            bool wasHidden = !_aimPreviewLine.enabled;
            if (wasHidden)
                _previewWobbleTimeLeft = PreviewWobbleDuration;

            Vector3 end = origin + direction.normalized * Mathf.Max(0f, AimPreviewLineLength);
            SetPreviewEndpoints(origin, end);
            _aimPreviewLine.enabled = true;
        }

        public void HideAimPreview()
        {
            if (_aimPreviewLine != null)
                _aimPreviewLine.enabled = false;
        }

        public void ShowShotLine(BattleMoleculeShotRequest request, Vector3? hitPoint, float? lineLength = null)
        {
            Vector3 origin = request.Origin;
            Vector3 direction = request.Direction;

            if (direction.sqrMagnitude <= Mathf.Epsilon)
                return;

            int shotLineIndex = GetAvailableShotLineIndex();
            LineRenderer shotLine = _shotLines[shotLineIndex];
            Vector3 end = hitPoint ?? origin + direction.normalized * Mathf.Max(0f, lineLength ?? ShotLineLength);

            shotLine.SetPosition(0, origin);
            shotLine.SetPosition(1, end);
            _shotLineTimesLeft[shotLineIndex] = CurrentShotLineSeconds();
            LineRendererUtility.SetColor(shotLine, ShotLineColor);
            shotLine.enabled = true;
        }

        private void SetPreviewEndpoints(Vector3 from, Vector3 to)
        {
            if (_aimPreviewLine == null)
                return;

            Vector3 direction = to - from;
            float distance = direction.magnitude;

            if (distance < 0.001f || _previewWobbleTimeLeft <= 0f || PreviewWobbleAmplitude <= 0f)
            {
                if (_aimPreviewLine.positionCount != 2)
                    _aimPreviewLine.positionCount = 2;

                _aimPreviewLine.SetPosition(0, from);
                _aimPreviewLine.SetPosition(1, to);
                return;
            }

            int pointCount = _previewPointCount;
            if (_aimPreviewLine.positionCount != pointCount)
                _aimPreviewLine.positionCount = pointCount;

            Vector3 normalizedDir = direction / distance;
            Vector3 perpendicular = Vector3.Cross(normalizedDir, Vector3.forward).normalized;

            if (perpendicular.sqrMagnitude < 0.001f)
                perpendicular = Vector3.up;

            float amplitudeFactor = _previewWobbleTimeLeft / PreviewWobbleDuration;
            float currentAmplitude = PreviewWobbleAmplitude * amplitudeFactor;
            float timeOffset = Time.time * PreviewWobbleAnimationSpeed;

            for (int i = 0; i < pointCount; i++)
            {
                float t = (float)i / (pointCount - 1);
                Vector3 basePoint = Vector3.Lerp(from, to, t);

                float weight = Mathf.Sin(t * Mathf.PI);
                float sineOffset = Mathf.Sin(t * Mathf.PI * 2f * PreviewWobbleFrequency + timeOffset) * currentAmplitude * weight;
                Vector3 point = basePoint + perpendicular * sineOffset;

                _aimPreviewLine.SetPosition(i, point);
            }
        }

        private LineRenderer SetupLine(GameObject lineObject, float width, int sortingOrder, Color color, int pointCount = 2)
        {
            LineRenderer line = LineRendererUtility.Ensure(lineObject);
            line.positionCount = pointCount;
            LineRendererUtility.Configure(line, width, sortingOrder, color, false);

            line.widthCurve = WidthCurve;
            line.widthMultiplier = Mathf.Max(0f, width);

            return line;
        }

        private void SetEnd(Vector3 end)
        {
            if (_aimLine == null)
                return;

            _aimLine.SetPosition(0, _origin);
            _aimLine.SetPosition(1, end);
        }

        private int GetAvailableShotLineIndex()
        {
            for (int i = 0; i < _shotLines.Count; i++)
            {
                if (_shotLines[i] != null && !_shotLines[i].enabled)
                    return i;
            }

            return CreateShotLine();
        }

        private int CreateShotLine()
        {
            int index = _shotLines.Count;
            LineRenderer shotLine = SetupLine(GetOrCreateShotLineObject(index), ShotLineWidth, ShotLineSortingOrder, ShotLineColor);
            _shotLines.Add(shotLine);
            _shotLineTimesLeft.Add(0f);
            return index;
        }

        private GameObject GetOrCreateShotLineObject(int index)
        {
            string objectName = index == 0 ? SHOT_LINE_OBJECT_NAME : $"{SHOT_LINE_OBJECT_NAME}_{index}";
            return GetOrCreateLineObject(objectName);
        }

        private GameObject GetOrCreateLineObject(string objectName)
        {
            Transform existing = transform.Find(objectName);
            if (existing != null)
                return existing.gameObject;

            GameObject lineObject = new(objectName);
            lineObject.transform.SetParent(transform, false);
            return lineObject;
        }

        private float CurrentShotLineSeconds()
        {
            return Mathf.Max(0.01f, ShotLineSeconds);
        }
    }
}
