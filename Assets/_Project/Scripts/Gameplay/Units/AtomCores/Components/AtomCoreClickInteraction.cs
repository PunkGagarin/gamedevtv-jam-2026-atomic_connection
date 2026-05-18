using _Project.Scripts.Gameplay.Cameras.Provider;
using _Project.Scripts.Gameplay.Common.Physics;
using _Project.Scripts.Gameplay.Common.Random;
using _Project.Scripts.Gameplay.Input.Service;
using _Project.Scripts.Gameplay.Units.FreeAtoms;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Units.AtomCores.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AtomCore))]
    public class AtomCoreClickInteraction : MonoBehaviour
    {
        private const int PHYSICS_LAYER_MASK = ~0;

        private AtomCore _core;

        [Inject] private IInputService _inputService;
        [Inject] private ICameraProvider _cameraProvider;
        [Inject] private IPhysicsService _physicsService;
        [Inject] private IRandomService _random;
        [Inject] private IFreeAtomFactory _freeAtomFactory;

        private void Awake()
        {
            if (_core == null)
                _core = GetComponent<AtomCore>();
        }

        public void Tick()
        {
            if (_core == null)
                return;

            if (_inputService == null || !_inputService.GetLeftMouseButtonDown())
                return;

            Camera camera = _cameraProvider != null ? _cameraProvider.MainCamera : null;
            if (camera == null || _physicsService == null)
                return;

            Vector2 worldPosition = _inputService.GetWorldMousePosition();
            Collider2D hit = _physicsService.OverlapPoint(worldPosition, PHYSICS_LAYER_MASK);

            if (hit == null || hit.gameObject != gameObject)
                return;

            if (_core.RegisterAtomClick())
                CreateAtomForCore();
        }

        private void CreateAtomForCore()
        {
            if (_freeAtomFactory == null)
                return;

            FreeAtom freeAtom = _freeAtomFactory.Create(AtomSpawnPosition(), transform);

            if (freeAtom != null)
                _core.TakeGeneratedAtom(freeAtom);
        }

        private Vector3 AtomSpawnPosition()
        {
            if (_random == null)
                return transform.position;

            float angle = _random.Range(0f, Mathf.PI * 2f);
            float radius01 = Mathf.Sqrt(_random.Range(0f, 1f));

            return _core.GetAtomSpawnPosition(angle, radius01);
        }
    }
}
