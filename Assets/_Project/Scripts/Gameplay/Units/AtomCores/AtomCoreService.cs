using System;
using _Project.Scripts.Gameplay.Cameras.Provider;
using _Project.Scripts.Gameplay.Common.Random;
using _Project.Scripts.Gameplay.Common.Time;
using _Project.Scripts.Gameplay.Drag;
using _Project.Scripts.Gameplay.Input.Service;
using _Project.Scripts.Gameplay.Talents;
using _Project.Scripts.Gameplay.Units.FreeAtoms;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Units.AtomCores
{
    public class AtomCoreService : IAtomCoreService, IAtomCoreCreator
    {
        private AtomCore _core;
        private bool _isStarted;
        private bool _clickWasStartedOnCore;
        private float _autoClickTimer;

        [Inject] private ICameraProvider _cameraProvider;
        [Inject] private IInputService _inputService;
        [Inject] private IDragService _dragService;
        [Inject] private IRandomService _random;
        [Inject] private ITimeService _time;
        [Inject] private AtomCoreConfig _config;
        [Inject] private ITalentService _talentService;
        [Inject] private IAtomCoreFactory _atomCoreFactory;
        [Inject] private IFreeAtomFactory _freeAtomFactory;

        public Transform CurrentCoreTransform => _core != null ? _core.transform : null;
        public event Action CoreDied;

        public void Create(Vector3 at)
        {
            if (_core != null)
                Cleanup();

            _core = _atomCoreFactory.Create(at);

            if (_core != null)
                ApplyTalentBonuses();
        }

        public void Start()
        {
            if (_core == null || _isStarted)
                return;

            _core.Died += OnCoreDied;
            _talentService.Changed += OnTalentChanged;
            _autoClickTimer = 0f;
            _isStarted = true;
        }

        public void Update()
        {
            if (_core == null)
                return;

            TickAutoClick();
            TickCoreClickInput();
            _core.Tick(_time.DeltaTime);
        }

        public void Cleanup()
        {
            if (_core != null && _isStarted)
            {
                _core.Died -= OnCoreDied;
                _talentService.Changed -= OnTalentChanged;
            }

            _core?.CleanupAtoms();
            _isStarted = false;
            _clickWasStartedOnCore = false;
            _autoClickTimer = 0f;
            _atomCoreFactory.Cleanup();
            _core = null;
        }

        private void TickCoreClickInput()
        {
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

        private void TickAutoClick()
        {
            float deltaTime = _time.DeltaTime;
            if (deltaTime <= 0f || !CanAutoClick())
            {
                _autoClickTimer = 0f;
                return;
            }

            _autoClickTimer += deltaTime;
            if (_autoClickTimer < Mathf.Max(0.01f, _config.AutoClickIntervalSeconds))
                return;

            _autoClickTimer = 0f;
            TryRegisterGeneratedAtomClick();
        }

        private bool CanAutoClick()
        {
            if (_talentService == null || !_talentService.IsUnlocked(TalentType.AutoClick))
                return false;

            if (!IsPointerOverCore())
                return false;

            if (_inputService.GetLeftMouseButtonRaw())
                return false;

            if (_dragService != null && _dragService.IsDragActive)
                return false;

            return true;
        }

        private bool IsPointerOverCore()
        {
            Camera camera = _cameraProvider.MainCamera;
            return camera != null && _core.ContainsPoint(_inputService.GetWorldMousePosition());
        }

        private void TryStartPendingClick()
        {
            _clickWasStartedOnCore = false;

            Camera camera = _cameraProvider.MainCamera;
            if (camera == null)
                return;

            if (_core.ContainsPoint(_inputService.GetWorldMousePosition()))
                _clickWasStartedOnCore = true;
        }

        private void CreateAtomForCore()
        {
            Vector2 offset = RandomGeometry.PointInCircle(_random, _config.SpawnRadiusOffset);
            Vector3 spawnPosition = _core.transform.position + new Vector3(offset.x, offset.y, 0f);
            FreeAtom freeAtom = _freeAtomFactory.Create(spawnPosition, _core.transform);
            _core.TakeGeneratedAtom(freeAtom);
        }

        private void TryRegisterGeneratedAtomClick()
        {
            if (_core.RegisterAtomClick())
                CreateAtomForCore();
        }

        private void OnTalentChanged()
        {
            if (_core == null)
                return;

            ApplyTalentBonuses();
        }

        private void ApplyTalentBonuses()
        {
            int adjustedClicks = Mathf.Max(1, Mathf.RoundToInt(
                _config.ClicksToGenerateFreeAtom / _talentService.AtomGenerationMultiplier));

            int adjustedHealth = Mathf.Max(1, Mathf.RoundToInt(
                _config.CoreMaxHealth + _talentService.BonusOf(TalentType.CoreHealth)));

            _core.Configure(_config, adjustedClicks, adjustedHealth);
        }
        
        private void OnCoreDied()
        {
            CoreDied?.Invoke();
        }
    }
}
