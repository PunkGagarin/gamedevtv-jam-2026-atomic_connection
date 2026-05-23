using _Project.Scripts.Gameplay.Common.Rendering;
using _Project.Scripts.Gameplay.Units.BattleMolecules;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BattleMoleculeBond))]
    public class BattleMoleculeConnectionVisual : MonoBehaviour
    {
        [field: SerializeField] private BattleMoleculeBond Bond { get; set; }

        private LineRenderer _line;
        private Transform _core;
        private float _zOffset;
        private Color _inactiveColor = Color.cyan;
        private Color _activeColor = Color.white;
        private bool _isActiveConnection;

        private void Awake()
        {
            if (Bond == null)
                Bond = GetComponent<BattleMoleculeBond>();

            EnsureLine();
            SetVisible(false);
        }

        private void OnEnable()
        {
            if (Bond == null)
                return;

            Bond.Bonded += RefreshVisibility;
            Bond.Changed += RefreshVisibility;
        }

        private void OnDisable()
        {
            if (Bond == null)
                return;

            Bond.Bonded -= RefreshVisibility;
            Bond.Changed -= RefreshVisibility;
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

            RefreshVisibility();
            Tick();
        }

        internal void RefreshVisibility()
        {
            SetVisible(Bond != null && Bond.IsBonded);
        }

        private void SetVisible(bool isVisible)
        {
            EnsureLine();

            _line.enabled = isVisible;
        }

        public void SetActiveConnection(bool isActive)
        {
            _isActiveConnection = isActive;
            RefreshColor();
        }

        public void Tick()
        {
            if (_core == null || _line == null || !_line.enabled)
                return;

            Vector3 from = _core.position;
            Vector3 to = transform.position;
            from.z += _zOffset;
            to.z += _zOffset;

            _line.SetPosition(0, from);
            _line.SetPosition(1, to);
        }

        private void ConfigureLine(float width, int sortingOrder)
        {
            EnsureLine();

            _line.positionCount = 2;
            LineRendererUtility.Configure(
                _line,
                width,
                sortingOrder,
                _isActiveConnection ? _activeColor : _inactiveColor,
                _line.enabled);
            RefreshColor();
        }

        private void RefreshColor()
        {
            if (_line == null)
                return;

            LineRendererUtility.SetColor(_line, _isActiveConnection ? _activeColor : _inactiveColor);
        }

        private void EnsureLine()
        {
            _line = LineRendererUtility.Ensure(gameObject, _line);
        }
    }
}
