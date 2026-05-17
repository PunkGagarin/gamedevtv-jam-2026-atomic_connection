using _Project.Scripts.Gameplay.Cameras.Provider;
using _Project.Scripts.Gameplay.Common.Physics;
using _Project.Scripts.Gameplay.Common.Random;
using _Project.Scripts.Gameplay.Common.Time;
using _Project.Scripts.Gameplay.Input.Service;
using _Project.Scripts.Gameplay.Talents;
using _Project.Scripts.Gameplay.Units.FreeAtoms;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Units.AtomCores
{
    public class AtomCoreService : IAtomCoreService
    {
        private const int PHYSICS_LAYER_MASK = ~0;

        private AtomCore _core;

        [Inject] private IInputService _inputService;
        [Inject] private ICameraProvider _cameraProvider;
        [Inject] private IPhysicsService _physicsService;
        [Inject] private IRandomService _random;
        [Inject] private ITimeService _time;
        [Inject] private UnitClickConfig _config;
        [Inject] private IFreeAtomFactory _freeAtomFactory;
        [Inject] private ITalentService _talentService;

        public void Start(AtomCore core)
        {
            _core = core;
            _core?.Configure(_config);
        }

        public void Update()
        {
            if (_core == null)
                return;

            HandleCoreClick();
            _core.Tick(_time.DeltaTime);
        }

        public void Cleanup()
        {
            _core?.CleanupAtoms();
            _core = null;
        }

        private void HandleCoreClick()
        {
            if (!_inputService.GetLeftMouseButtonDown())
                return;

            Camera camera = _cameraProvider.MainCamera;
            if (camera == null)
                return;

            Vector2 worldPosition = _inputService.GetWorldMousePosition();
            Collider2D hit = _physicsService.OverlapPoint(worldPosition, PHYSICS_LAYER_MASK);

            if (hit == null || hit.gameObject != _core.gameObject)
                return;

            if (_core.RegisterAtomClick())
                CreateAtomForCore();
        }

        private void CreateAtomForCore()
        {
            FreeAtom freeAtom = _freeAtomFactory.Create(AtomSpawnPosition(), _core.transform);

            if (freeAtom != null)
                _core.TakeGeneratedAtom(freeAtom);
        }

        private Vector3 AtomSpawnPosition()
        {
            float angle = _random.Range(0f, Mathf.PI * 2f);
            float radius01 = Mathf.Sqrt(_random.Range(0f, 1f));

            return _core.GetAtomSpawnPosition(angle, radius01);
        }
    }
}
