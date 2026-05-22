using _Project.Scripts.Gameplay.Cameras.Provider;
using _Project.Scripts.Gameplay.Common.Random;
using _Project.Scripts.Gameplay.Drag;
using _Project.Scripts.Gameplay.Input.Service;
using _Project.Scripts.Gameplay.Units;
using _Project.Scripts.Gameplay.Units.FreeAtoms;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace _Project.Scripts.Gameplay.Units.AtomCores.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AtomCore))]
    [RequireComponent(typeof(Collider2D))]
    public class AtomCoreClickInteraction : MonoBehaviour
    {
        private AtomCore _core;
        private Collider2D _clickCollider;
        private bool _clickWasStartedOnCore;

        [Inject]
        private IInputService _inputService;

        [Inject]
        private ICameraProvider _cameraProvider;

        [Inject]
        private IDragService _dragService;

        [Inject]
        private IRandomService _random;

        [Inject]
        private IFreeAtomFactory _freeAtomFactory;

        [SerializeField]
        private UnityEvent OnClickEvent;

        private void Awake()
        {
            if (_core == null)
                _core = GetComponent<AtomCore>();

            if (_clickCollider == null)
                _clickCollider = GetClickCollider();
        }

        public void Tick(float deltaTime)
        {
            if (_core == null)
                return;

            if (_inputService == null)
                return;

            if (_inputService.GetLeftMouseButtonDown())
                TryStartPendingClick();

            if (!_clickWasStartedOnCore || !_inputService.GetLeftMouseButtonUpRaw())
                return;

            bool shouldRegisterClick =
                _inputService.GetLeftMouseButtonUp()
                && (_dragService == null || !_dragService.DragWasStartedThisPress);

            _clickWasStartedOnCore = false;

            if (!shouldRegisterClick)
                return;

            TryRegisterGeneratedAtomClick();
        }

        private void TryStartPendingClick()
        {
            _clickWasStartedOnCore = false;

            Camera camera = _cameraProvider != null ? _cameraProvider.MainCamera : null;
            if (camera == null || _clickCollider == null)
                return;

            Vector2 worldPosition = _inputService.GetWorldMousePosition();

            if (!_clickCollider.OverlapPoint(worldPosition))
                return;

            OnClickEvent?.Invoke();

            _clickWasStartedOnCore = true;
        }

        private void TryRegisterGeneratedAtomClick()
        {
            if (_core.RegisterAtomClick())
                CreateAtomForCore();
        }

        private Collider2D GetClickCollider()
        {
            foreach (Collider2D col in GetComponents<Collider2D>())
            {
                if (!col.isTrigger)
                    return col;
            }

            return GetComponent<Collider2D>();
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
