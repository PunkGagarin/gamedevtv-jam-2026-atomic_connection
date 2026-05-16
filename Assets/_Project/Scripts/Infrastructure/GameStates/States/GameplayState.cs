using _Project.Scripts.Gameplay.Cameras.Provider;
using _Project.Scripts.Gameplay.Drag;
using _Project.Scripts.Gameplay.Enemies;
using _Project.Scripts.Gameplay.Input.Service;
using _Project.Scripts.Gameplay.Units.Atom;
using _Project.Scripts.Gameplay.Units.Example;
using _Project.Scripts.Utils.Pause;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Infrastructure.GameStates.States
{
    internal class GameplayState : EndOfFrameExitState
    {
        [Inject] private IEnemySpawner _enemySpawner;
        [Inject] private IExampleUnitFactory _exampleUnitFactory;
        [Inject] private IAtomFactory _atomFactory;
        [Inject] private IDragService _dragService;
        [Inject] private IInputService _inputService;
        [Inject] private ICameraProvider _cameraProvider;
        [Inject] private PauseService _pauseService;

        private bool _wasDragging;

        public override void Enter()
        {
            _enemySpawner.Start(_exampleUnitFactory.CurrentUnit != null
                ? _exampleUnitFactory.CurrentUnit.transform
                : null);
        }

        protected override void OnUpdate()
        {
            if (_pauseService.IsPaused)
                return;

            _enemySpawner.Update();
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

                if (_inputService.GetLeftMouseButtonUp())
                {
                    _dragService.EndDrag(screenPos, camera);
                    _wasDragging = true;
                }
                else
                {
                    _wasDragging = false;
                }
            }
        }

        protected override void ExitOnEndOfFrame()
        {
            _dragService.CancelDrag();
            _enemySpawner.Cleanup();
            _exampleUnitFactory.Cleanup();
            _atomFactory.Cleanup();
        }
    }
}
