using _Project.Scripts.Gameplay.Cameras.Provider;
using _Project.Scripts.Gameplay.Drag;
using _Project.Scripts.Gameplay.Enemies;
using _Project.Scripts.Gameplay.Input.Service;
using _Project.Scripts.Gameplay.Units.FreeAtoms;
using _Project.Scripts.Gameplay.Units.BattleMolecules;
using _Project.Scripts.Gameplay.Units.AtomCores;
using _Project.Scripts.Utils.Pause;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Infrastructure.GameStates.States
{
    internal class GameplayState : EndOfFrameExitState
    {
        [Inject] private IEnemySpawner _enemySpawner;
        [Inject] private IAtomCoreFactory _atomCoreFactory;
        [Inject] private IAtomCoreClickService _atomCoreClickService;
        [Inject] private IFreeAtomFactory _freeAtomFactory;
        [Inject] private IBattleMoleculeFactory _battleMoleculeFactory;
        [Inject] private IDragService _dragService;
        [Inject] private IInputService _inputService;
        [Inject] private ICameraProvider _cameraProvider;
        [Inject] private PauseService _pauseService;

        public override void Enter()
        {
            AtomCore currentCore = _atomCoreFactory.CurrentCore;

            _atomCoreClickService.Start(currentCore);
            _enemySpawner.Start(currentCore != null ? currentCore.transform : null);
        }

        protected override void OnUpdate()
        {
            if (_pauseService.IsPaused)
                return;

            _enemySpawner.Update();
            _atomCoreClickService.Update();
            UpdateDrag();
        }

        private void UpdateDrag()
        {
            Camera camera = _cameraProvider.MainCamera;
            if (camera == null)
                return;

            Vector2 screenPos = _inputService.GetScreenMousePosition();

            if (!_dragService.IsDragging)
            {
                if (_inputService.GetLeftMouseButtonDown())
                    _dragService.TryStartDrag(screenPos, camera);
            }
            else
            {
                _dragService.UpdateDrag(screenPos, camera);

                if (_inputService.GetLeftMouseButtonUpRaw())
                    _dragService.EndDrag(screenPos, camera);
            }
        }

        protected override void ExitOnEndOfFrame()
        {
            _dragService.CancelDrag();
            _enemySpawner.Cleanup();
            _atomCoreClickService.Cleanup();
            _atomCoreFactory.Cleanup();
            _battleMoleculeFactory.Cleanup();
            _freeAtomFactory.Cleanup();
        }
    }
}
