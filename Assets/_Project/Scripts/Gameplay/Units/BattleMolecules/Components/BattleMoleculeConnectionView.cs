using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    public class BattleMoleculeConnectionView : MonoBehaviour
    {
        private static Material _defaultLineMaterial;

        [field: SerializeField] private LineRenderer Line { get; set; }

        private Transform _core;
        private float _zOffset;
        private Color _inactiveColor = Color.cyan;
        private Color _activeColor = Color.white;
        private bool _isActiveConnection;

        private static Material DefaultLineMaterial
        {
            get
            {
                if (_defaultLineMaterial != null)
                    return _defaultLineMaterial;

                Shader shader = Shader.Find("Sprites/Default");
                if (shader == null)
                    return null;

                _defaultLineMaterial = new Material(shader)
                {
                    hideFlags = HideFlags.HideAndDontSave
                };

                return _defaultLineMaterial;
            }
        }

        private void Awake()
        {
            EnsureLine();
            SetVisible(false);
        }

        public void Configure(Transform core, BattleMoleculeConfig config)
        {
            _core = core;

            if (config != null)
            {
                _zOffset = config.ConnectionLineZOffset;
                _inactiveColor = config.ConnectionInactiveColor;
                _activeColor = config.ConnectionActiveColor;
                ConfigureLine(config.ConnectionLineWidth, config.ConnectionLineSortingOrder);
            }

            Tick();
        }

        public void SetVisible(bool isVisible)
        {
            EnsureLine();

            if (Line != null)
                Line.enabled = isVisible;
        }

        public void SetActiveConnection(bool isActive)
        {
            _isActiveConnection = isActive;
            RefreshColor();
        }

        public void Tick()
        {
            if (_core == null || Line == null || !Line.enabled)
                return;

            Vector3 from = _core.position;
            Vector3 to = transform.position;
            from.z += _zOffset;
            to.z += _zOffset;

            Line.SetPosition(0, from);
            Line.SetPosition(1, to);
        }

        private void ConfigureLine(float width, int sortingOrder)
        {
            EnsureLine();

            if (Line == null)
                return;

            Line.useWorldSpace = true;
            Line.positionCount = 2;
            Line.startWidth = Mathf.Max(0f, width);
            Line.endWidth = Mathf.Max(0f, width);
            Line.sortingOrder = sortingOrder;

            if (Line.sharedMaterial == null)
                Line.sharedMaterial = DefaultLineMaterial;

            RefreshColor();
        }

        private void RefreshColor()
        {
            if (Line == null)
                return;

            Color color = _isActiveConnection ? _activeColor : _inactiveColor;
            Line.startColor = color;
            Line.endColor = color;
        }

        private void EnsureLine()
        {
            if (Line == null)
                Line = GetComponent<LineRenderer>();

            if (Line == null)
                Line = gameObject.AddComponent<LineRenderer>();
        }
    }
}
