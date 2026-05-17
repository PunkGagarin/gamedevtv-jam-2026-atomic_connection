using _Project.Scripts.Gameplay.Cameras.Provider;
using _Project.Scripts.Gameplay.Drag;
using _Project.Scripts.Gameplay.Enemies;
using _Project.Scripts.Gameplay.Input.Service;
using _Project.Scripts.Gameplay.Units.Atom;
using _Project.Scripts.Gameplay.Units.BattleMolecule;
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
        [Inject] private IExampleUnitClickService _exampleUnitClickService;
        [Inject] private IAtomFactory _atomFactory;
        [Inject] private IBattleMoleculeFactory _battleMoleculeFactory;
        [Inject] private IDragService _dragService;
        [Inject] private IInputService _inputService;
        [Inject] private ICameraProvider _cameraProvider;
        [Inject] private PauseService _pauseService;

        public override void Enter()
        {
            ExampleUnit currentUnit = _exampleUnitFactory.CurrentUnit;

            _exampleUnitClickService.Start(currentUnit);
            _enemySpawner.Start(currentUnit != null ? currentUnit.transform : null);
        }

        protected override void OnUpdate()
        {
            if (_pauseService.IsPaused)
                return;

            _enemySpawner.Update();
            _exampleUnitClickService.Update();
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
            _exampleUnitClickService.Cleanup();
            _exampleUnitFactory.Cleanup();
            _battleMoleculeFactory.Cleanup();
            _atomFactory.Cleanup();
        }
    }
}
