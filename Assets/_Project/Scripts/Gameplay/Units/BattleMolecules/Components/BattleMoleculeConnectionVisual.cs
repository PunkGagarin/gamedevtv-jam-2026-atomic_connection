using _Project.Scripts.Gameplay.Units.BattleMolecules;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BattleMoleculeBond))]
    public class BattleMoleculeConnectionVisual : MonoBehaviour
    {
        [field: SerializeField] private BattleMoleculeBond Bond { get; set; }
        [field: SerializeField] private BattleMoleculeConnectionLineVisual LineVisualPrefab { get; set; }

        private BattleMoleculeConnectionLineVisual _lineVisual;
        private Transform _core;
        private float _zOffset;
        private float _lineWidth;
        private int _sortingOrder;
        private Color _inactiveColor = Color.cyan;
        private Color _activeColor = Color.white;
        private bool _isActiveConnection;
        private bool _missingLineVisualPrefabLogged;

        private void Awake()
        {
            if (Bond == null)
                Bond = GetComponent<BattleMoleculeBond>();

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
                _lineWidth = config.ConnectionLineWidth;
                _sortingOrder = config.ConnectionLineSortingOrder;
                _inactiveColor = config.ConnectionInactiveColor;
                _activeColor = config.ConnectionActiveColor;
                ConfigureLine();
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
            if (EnsureLineVisual())
                _lineVisual.SetVisible(isVisible);
        }

        public void SetActiveConnection(bool isActive)
        {
            _isActiveConnection = isActive;
            RefreshColor();
        }

        public void Tick()
        {
            if (_core == null || !EnsureLineVisual() || !_lineVisual.IsVisible)
                return;

            Vector3 from = _core.position;
            Vector3 to = transform.position;
            from.z += _zOffset;
            to.z += _zOffset;

            _lineVisual.SetEndpoints(from, to);
        }

        private void ConfigureLine()
        {
            if (EnsureLineVisual())
                _lineVisual.Configure(_lineWidth, _sortingOrder, CurrentColor());
        }

        private void RefreshColor()
        {
            if (EnsureLineVisual())
                _lineVisual.SetColor(CurrentColor());
        }

        private Color CurrentColor()
        {
            return _isActiveConnection ? _activeColor : _inactiveColor;
        }

        private bool EnsureLineVisual()
        {
            if (_lineVisual != null)
                return true;

            if (LineVisualPrefab == null)
            {
                if (!_missingLineVisualPrefabLogged)
                {
                    Debug.LogError($"{nameof(BattleMoleculeConnectionVisual)} on '{name}' is missing a line visual prefab.", this);
                    _missingLineVisualPrefabLogged = true;
                }

                return false;
            }

            _lineVisual = Instantiate(LineVisualPrefab, transform);
            _lineVisual.name = LineVisualPrefab.name;
            return _lineVisual != null;
        }
    }
}
