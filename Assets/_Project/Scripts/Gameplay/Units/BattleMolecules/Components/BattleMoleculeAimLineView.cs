using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    public class BattleMoleculeAimLineView : MonoBehaviour
    {
        private const string SHOT_LINE_OBJECT_NAME = "ShotLine";
        private const float AIM_LINE_WIDTH = 0.08f;
        private const float SHOT_LINE_WIDTH = 0.12f;
        private const float SHOT_LINE_LENGTH = 20f;
        private const float SHOT_LINE_SECONDS = 0.12f;
        private const int AIM_LINE_SORTING_ORDER = 10;
        private const int SHOT_LINE_SORTING_ORDER = 11;

        private static Material _lineMaterial;

        [field: SerializeField] private BattleMoleculeShotQueue ShotQueue { get; set; }

        private readonly List<LineRenderer> _shotLines = new();
        private readonly List<float> _shotLineTimesLeft = new();
        private LineRenderer _aimLine;
        private Vector3 _origin;

        private void Awake()
        {
            if (ShotQueue == null)
                ShotQueue = GetComponent<BattleMoleculeShotQueue>();

            _aimLine = SetupLine(gameObject, AIM_LINE_WIDTH, AIM_LINE_SORTING_ORDER, Color.yellow);
            CreateShotLine();
        }

        private void OnEnable()
        {
            if (ShotQueue != null)
                ShotQueue.ShotRequested += ShowShotLine;
        }

        private void OnDisable()
        {
            if (ShotQueue != null)
                ShotQueue.ShotRequested -= ShowShotLine;

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

                SetLineColor(shotLine, Color.cyan, _shotLineTimesLeft[i] / SHOT_LINE_SECONDS);
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

        public void Hide()
        {
            if (_aimLine != null)
                _aimLine.enabled = false;
        }

        private void ShowShotLine(Vector3 origin, Vector3 direction)
        {
            if (direction.sqrMagnitude <= Mathf.Epsilon)
                return;

            int shotLineIndex = GetAvailableShotLineIndex();
            LineRenderer shotLine = _shotLines[shotLineIndex];
            Vector3 end = origin + direction.normalized * SHOT_LINE_LENGTH;

            shotLine.SetPosition(0, origin);
            shotLine.SetPosition(1, end);
            _shotLineTimesLeft[shotLineIndex] = SHOT_LINE_SECONDS;
            SetLineColor(shotLine, Color.cyan, 1f);
            shotLine.enabled = true;
        }

        private LineRenderer SetupLine(GameObject lineObject, float width, int sortingOrder, Color color)
        {
            LineRenderer line = lineObject.GetComponent<LineRenderer>();
            if (line == null)
                line = lineObject.AddComponent<LineRenderer>();

            line.useWorldSpace = true;
            line.positionCount = 2;
            line.startWidth = width;
            line.endWidth = width;
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
            LineRenderer shotLine = SetupLine(GetOrCreateShotLineObject(index), SHOT_LINE_WIDTH, SHOT_LINE_SORTING_ORDER, Color.cyan);
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
