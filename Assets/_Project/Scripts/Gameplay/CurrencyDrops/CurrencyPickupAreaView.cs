using UnityEngine;

namespace _Project.Scripts.Gameplay.CurrencyDrops
{
    public class CurrencyPickupAreaView : MonoBehaviour
    {
        private const int CORNER_COUNT = 5;

        private static Material _lineMaterial;

        private readonly Vector3[] _corners = new Vector3[CORNER_COUNT];
        private LineRenderer _line;

        private void Awake()
        {
            EnsureLine();
        }

        public void Configure(float lineWidth, int sortingOrder, Color color)
        {
            EnsureLine();

            float width = Mathf.Max(0f, lineWidth);
            _line.startWidth = width;
            _line.endWidth = width;
            _line.sortingOrder = sortingOrder;
            _line.startColor = color;
            _line.endColor = color;
        }

        public void Show(Vector3 center, float halfSize)
        {
            halfSize = Mathf.Max(0f, halfSize);
            if (Mathf.Approximately(halfSize, 0f))
            {
                Hide();
                return;
            }

            EnsureLine();
            SetCorners(center, halfSize);
            _line.SetPositions(_corners);
            _line.enabled = true;
        }

        public void Hide()
        {
            if (_line != null)
                _line.enabled = false;
        }

        private void EnsureLine()
        {
            if (_line != null)
                return;

            _line = gameObject.GetComponent<LineRenderer>();
            if (_line == null)
                _line = gameObject.AddComponent<LineRenderer>();

            _line.useWorldSpace = true;
            _line.positionCount = CORNER_COUNT;
            _line.enabled = false;

            if (_line.sharedMaterial == null)
                _line.sharedMaterial = GetLineMaterial();
        }

        private void SetCorners(Vector3 center, float halfSize)
        {
            _corners[0] = new Vector3(center.x - halfSize, center.y - halfSize, center.z);
            _corners[1] = new Vector3(center.x - halfSize, center.y + halfSize, center.z);
            _corners[2] = new Vector3(center.x + halfSize, center.y + halfSize, center.z);
            _corners[3] = new Vector3(center.x + halfSize, center.y - halfSize, center.z);
            _corners[4] = _corners[0];
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
