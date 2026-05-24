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

        private readonly List<LineRenderer> _shotLines = new();
        private readonly List<float> _shotLineTimesLeft = new();
        private LineRenderer _aimLine;
        private LineRenderer _aimPreviewLine;
        private Vector3 _origin;

        private void Awake()
        {
            _aimLine = SetupLine(gameObject, AimLineWidth, AimLineSortingOrder, AimLineColor);
            _aimPreviewLine = SetupLine(
                GetOrCreateLineObject(AIM_PREVIEW_LINE_OBJECT_NAME),
                AimPreviewLineWidth,
                AimPreviewLineSortingOrder,
                AimPreviewLineColor);
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

            Vector3 end = origin + direction.normalized * Mathf.Max(0f, AimPreviewLineLength);

            _aimPreviewLine.SetPosition(0, origin);
            _aimPreviewLine.SetPosition(1, end);
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

        private LineRenderer SetupLine(GameObject lineObject, float width, int sortingOrder, Color color)
        {
            LineRenderer line = LineRendererUtility.Ensure(lineObject);
            line.positionCount = 2;
            LineRendererUtility.Configure(line, width, sortingOrder, color, false);

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
