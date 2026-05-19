using _Project.Scripts.Gameplay.Cameras.Provider;
using _Project.Scripts.Gameplay.Common.Random;
using _Project.Scripts.Gameplay.Drag;
using _Project.Scripts.Gameplay.Input.Service;
using _Project.Scripts.Gameplay.Talents;
using _Project.Scripts.Gameplay.Units.FreeAtoms;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Units.AtomCores.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AtomCore))]
    [RequireComponent(typeof(Collider2D))]
    public class AtomCoreClickInteraction : MonoBehaviour
    {
        private const float AUTO_CLICK_INTERVAL_SECONDS = 0.35f;

        private AtomCore _core;
        private Collider2D _clickCollider;
        private bool _clickWasStartedOnCore;
        private float _autoClickTimer;

        [Inject] private IInputService _inputService;
        [Inject] private ICameraProvider _cameraProvider;
        [Inject] private IDragService _dragService;
        [Inject] private IRandomService _random;
        [Inject] private IFreeAtomFactory _freeAtomFactory;
        [Inject] private ITalentService _talentService;

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

            TickAutoClick(deltaTime);

            if (_inputService.GetLeftMouseButtonDown())
                TryStartPendingClick();

            if (!_clickWasStartedOnCore || !_inputService.GetLeftMouseButtonUpRaw())
                return;

            bool shouldRegisterClick = _inputService.GetLeftMouseButtonUp() &&
                                       (_dragService == null || !_dragService.DragWasStartedThisPress);

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

            _clickWasStartedOnCore = true;
        }

        private void TickAutoClick(float deltaTime)
        {
            if (deltaTime <= 0f || !CanAutoClick())
            {
                _autoClickTimer = 0f;
                return;
            }

            _autoClickTimer += deltaTime;
            if (_autoClickTimer < AUTO_CLICK_INTERVAL_SECONDS)
                return;

            _autoClickTimer = 0f;
            TryRegisterGeneratedAtomClick();
        }

        private bool CanAutoClick()
        {
            if (_talentService == null || !_talentService.IsUnlocked(TalentType.AtomAutoClick))
                return false;

            if (_inputService.GetLeftMouseButtonRaw())
                return false;

            if (_dragService != null && _dragService.IsDragActive)
                return false;

            return IsMouseOverCore();
        }

        private bool IsMouseOverCore()
        {
            Camera camera = _cameraProvider != null ? _cameraProvider.MainCamera : null;
            if (camera == null || _clickCollider == null)
                return false;

            Vector2 worldPosition = _inputService.GetWorldMousePosition();
            return _clickCollider.OverlapPoint(worldPosition);
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
