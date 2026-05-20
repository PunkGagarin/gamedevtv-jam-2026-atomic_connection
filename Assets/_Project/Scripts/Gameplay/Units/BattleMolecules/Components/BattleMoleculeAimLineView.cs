using System.Collections.Generic;
using _Project.Scripts.Gameplay.Units.BattleMolecules;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    public class BattleMoleculeAimLineView : MonoBehaviour
    {
        private const string SHOT_LINE_OBJECT_NAME = "ShotLine";

        private static Material _lineMaterial;

        [field: SerializeField, Min(0f)] private float AimLineWidth { get; set; } = 0.08f;
        [field: SerializeField, Min(0f)] private float ShotLineWidth { get; set; } = 0.12f;
        [field: SerializeField, Min(0f)] private float ShotLineLength { get; set; } = 20f;
        [field: SerializeField, Min(0.01f)] private float ShotLineSeconds { get; set; } = 0.12f;
        [field: SerializeField] private int AimLineSortingOrder { get; set; } = 10;
        [field: SerializeField] private int ShotLineSortingOrder { get; set; } = 11;
        [field: SerializeField] private Color AimLineColor { get; set; } = Color.yellow;
        [field: SerializeField] private Color ShotLineColor { get; set; } = Color.cyan;

        private readonly List<LineRenderer> _shotLines = new();
        private readonly List<float> _shotLineTimesLeft = new();
        private LineRenderer _aimLine;
        private Vector3 _origin;

        private void Awake()
        {
            _aimLine = SetupLine(gameObject, AimLineWidth, AimLineSortingOrder, AimLineColor);
            CreateShotLine();
        }

        private void OnDisable()
        {
            for (int i = 0; i < _shotLines.Count; i++)
            {
                _shotLineTimesLeft[i] = 0f;

                if (_shotLines[i] != null)
                    _shotLines[i].enabled = false;
            }
        }

        private void Update()
        {
            for (int i = 0; i < _shotLines.Count; i++)
            {
                LineRenderer shotLine = _shotLines[i];
                if (shotLine == null || _shotLineTimesLeft[i] <= 0f)
                    continue;

                _shotLineTimesLeft[i] -= Time.deltaTime;
                if (_shotLineTimesLeft[i] <= 0f)
                {
                    shotLine.enabled = false;
                    continue;
                }

                SetLineColor(shotLine, ShotLineColor, _shotLineTimesLeft[i] / CurrentShotLineSeconds());
            }
        }

        public void Show(Vector3 origin)
        {
            _origin = origin;
            SetEnd(origin);

            if (_aimLine != null)
                _aimLine.enabled = true;
        }

        public void SetEnd(Vector3 end)
        {
            if (_aimLine == null)
                return;

            _aimLine.SetPosition(0, _origin);
            _aimLine.SetPosition(1, end);
        }

        public void SetSegment(Vector3 origin, Vector3 end)
        {
            _origin = origin;
            SetEnd(end);
        }

        public void Hide()
        {
            if (_aimLine != null)
                _aimLine.enabled = false;
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
            SetLineColor(shotLine, ShotLineColor, 1f);
            shotLine.enabled = true;
        }

        private LineRenderer SetupLine(GameObject lineObject, float width, int sortingOrder, Color color)
        {
            LineRenderer line = lineObject.GetComponent<LineRenderer>();
            if (line == null)
                line = lineObject.AddComponent<LineRenderer>();

            line.useWorldSpace = true;
            line.positionCount = 2;
            float lineWidth = Mathf.Max(0f, width);
            line.startWidth = lineWidth;
            line.endWidth = lineWidth;
            line.sortingOrder = sortingOrder;
            SetLineColor(line, color, 1f);
            line.enabled = false;

            if (line.sharedMaterial == null)
                line.sharedMaterial = GetLineMaterial();

            return line;
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
            Transform existing = transform.Find(objectName);
            if (existing != null)
                return existing.gameObject;

            GameObject lineObject = new(objectName);
            lineObject.transform.SetParent(transform, false);
            return lineObject;
        }

        private static void SetLineColor(LineRenderer line, Color color, float alpha)
        {
            color.a = alpha;
            line.startColor = color;
            line.endColor = color;
        }

        private float CurrentShotLineSeconds()
        {
            return Mathf.Max(0.01f, ShotLineSeconds);
        }

        private static Material GetLineMaterial()
        {
            if (_lineMaterial != null)
                return _lineMaterial;

            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
                return null;

            _lineMaterial = new Material(shader);
            return _lineMaterial;
        }
    }
}
