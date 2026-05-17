using _Project.Scripts.Gameplay.Drag;
using _Project.Scripts.Gameplay.Enemies;
using _Project.Scripts.Gameplay.Units.FreeAtoms;
using _Project.Scripts.Gameplay.Units.BattleMolecules;
using _Project.Scripts.Gameplay.Units.AtomCores;
using _Project.Scripts.Infrastructure.GameStates.StateInfrastructure;
using _Project.Scripts.Utils.Pause;
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
            _dragService.Update();
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
