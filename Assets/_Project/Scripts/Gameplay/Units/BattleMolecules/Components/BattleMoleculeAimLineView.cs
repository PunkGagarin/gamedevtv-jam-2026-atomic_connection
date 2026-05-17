using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    public class BattleMoleculeAimLineView : MonoBehaviour
    {
        private const float LINE_WIDTH = 0.08f;
        private const int LINE_SORTING_ORDER = 10;

        private static Material _lineMaterial;

        private LineRenderer _line;
        private Vector3 _origin;

        private void Awake()
        {
            SetupLine();
        }

        public void Show(Vector3 origin)
        {
            _origin = origin;
            SetEnd(origin);

            if (_line != null)
                _line.enabled = true;
        }

        public void SetEnd(Vector3 end)
        {
            if (_line == null)
                return;

            _line.SetPosition(0, _origin);
            _line.SetPosition(1, end);
        }

        public void Hide()
        {
            if (_line != null)
                _line.enabled = false;
        }

        private void SetupLine()
        {
            _line = GetComponent<LineRenderer>();
            if (_line == null)
                _line = gameObject.AddComponent<LineRenderer>();

            _line.useWorldSpace = true;
            _line.positionCount = 2;
            _line.startWidth = LINE_WIDTH;
            _line.endWidth = LINE_WIDTH;
            _line.sortingOrder = LINE_SORTING_ORDER;
            _line.startColor = Color.yellow;
            _line.endColor = Color.yellow;
            _line.enabled = false;

            if (_line.sharedMaterial == null)
                _line.sharedMaterial = GetLineMaterial();
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
