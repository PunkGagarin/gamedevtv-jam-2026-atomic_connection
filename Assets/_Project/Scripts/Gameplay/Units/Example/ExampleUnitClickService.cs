using _Project.Scripts.Gameplay.Cameras.Provider;
using _Project.Scripts.Gameplay.Common.Physics;
using _Project.Scripts.Gameplay.Common.Random;
using _Project.Scripts.Gameplay.Input.Service;
using _Project.Scripts.Gameplay.Units;
using _Project.Scripts.Gameplay.Units.Atom;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Units.Example
{
    public class ExampleUnitClickService : IExampleUnitClickService
    {
        private const int PHYSICS_LAYER_MASK = ~0;

        private ExampleUnit _unit;
        private int _clicksRemaining;

        [Inject] private IInputService _inputService;
        [Inject] private ICameraProvider _cameraProvider;
        [Inject] private IPhysicsService _physicsService;
        [Inject] private IRandomService _random;
        [Inject] private UnitClickConfig _config;
        [Inject] private IAtomFactory _atomFactory;

        public void Start(ExampleUnit unit)
        {
            _unit = unit;
            _clicksRemaining = _config.ClicksToGenerateAtom;
            UpdateProgressBar();
        }

        public void Update()
        {
            if (_unit == null || !_inputService.GetLeftMouseButtonDown())
                return;

            Camera camera = _cameraProvider.MainCamera;
            if (camera == null)
                return;

            Vector2 worldPosition = _inputService.GetWorldMousePosition();
            Collider2D hit = _physicsService.OverlapPoint(worldPosition, PHYSICS_LAYER_MASK);

            if (hit != null && hit.gameObject == _unit.gameObject)
                HandleClick();
        }

        public void Cleanup()
        {
            _unit = null;
            _clicksRemaining = 0;
        }

        private void HandleClick()
        {
            _clicksRemaining--;
            UpdateProgressBar();

            if (_clicksRemaining > 0)
                return;

            _atomFactory.Create(_unit.transform.position + (Vector3)RandomSpawnOffset());
            _clicksRemaining = _config.ClicksToGenerateAtom;
            UpdateProgressBar();
        }

        private Vector2 RandomSpawnOffset()
        {
            float angle = _random.Range(0f, Mathf.PI * 2f);
            float radius = Mathf.Sqrt(_random.Range(0f, 1f)) * _config.SpawnRadiusOffset;

            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }

        private void UpdateProgressBar()
        {
            if (_unit == null)
                return;

            _unit.SetClickProgress(_clicksRemaining, _config.ClicksToGenerateAtom);
        }
    }
}
