using _Project.Scripts.Gameplay.Drag;
using _Project.Scripts.Gameplay.Enemies;
using _Project.Scripts.Gameplay.CurrencyDrops;
using _Project.Scripts.Gameplay.Level;
using _Project.Scripts.Gameplay.Levels;
using _Project.Scripts.Gameplay.Units.FreeAtoms;
using _Project.Scripts.Gameplay.Units.BattleMolecules;
using _Project.Scripts.Gameplay.Units.AtomCores;
using _Project.Scripts.Infrastructure.GameStates.StateInfrastructure;
using _Project.Scripts.Infrastructure.GameStates.StateMachine;
using _Project.Scripts.Utils.Pause;
using Zenject;

namespace _Project.Scripts.Infrastructure.GameStates.States
{
    internal class GameplayLoopState : EndOfFrameExitState
    {
        [Inject] private IEnemyService _enemyService;
        [Inject] private IMergeEnemyService _mergeEnemyService;
        [Inject] private IEnemyKillRewardService _enemyKillRewardService;
        [Inject] private IEnemyProjectileService _enemyProjectileService;
        [Inject] private IAtomCoreService _atomCoreService;
        [Inject] private IFreeAtomFactory _freeAtomFactory;
        [Inject] private IBattleMoleculeService _battleMoleculeService;
        [Inject] private IDragService _dragService;
        [Inject] private ICurrencyPickupService _currencyPickupService;
        [Inject] private ILevelProgressService _levelProgressService;
        [Inject] private IGameplayRuntimeHierarchy _runtimeHierarchy;
        [Inject] private GameStateMachine _stateMachine;
        [Inject] private PauseService _pauseService;

        private bool _terminalTransitionWasRequested;

        public override void Enter()
        {
            _terminalTransitionWasRequested = false;
            _atomCoreService.CoreDied += OnCoreDied;
            _enemyService.BossKilled += OnBossKilled;
            _levelProgressService.Completed += OnLevelCompleted;
            _atomCoreService.Start();
            _mergeEnemyService.Start();
            _enemyService.Start(_atomCoreService.CurrentCoreTransform);
            _battleMoleculeService.Start();
            _currencyPickupService.Start();
            _levelProgressService.Start();
        }

        protected override void OnUpdate()
        {
            if (_pauseService.IsPaused)
                return;

            _enemyService.Update();
            _enemyProjectileService.Update();

            if (_terminalTransitionWasRequested)
                return;

            _dragService.Update();
            _currencyPickupService.Update();
            _battleMoleculeService.Update();
            _atomCoreService.Update();

            if (_terminalTransitionWasRequested)
                return;

            _levelProgressService.Update();
        }

        protected override void ExitOnEndOfFrame()
        {
            _atomCoreService.CoreDied -= OnCoreDied;
            _enemyService.BossKilled -= OnBossKilled;
            _levelProgressService.Completed -= OnLevelCompleted;
            _dragService.CancelDrag();
            _currencyPickupService.Cleanup();
            _levelProgressService.Cleanup();
            _enemyProjectileService.Cleanup();
            _mergeEnemyService.Cleanup();
            _enemyKillRewardService.Cleanup();
            _enemyService.Cleanup();
            _atomCoreService.Cleanup();
            _battleMoleculeService.Cleanup();
            _freeAtomFactory.Cleanup();
            _runtimeHierarchy.Cleanup();
        }

        private void OnCoreDied()
        {
            if (_terminalTransitionWasRequested)
                return;

            _terminalTransitionWasRequested = true;
            _stateMachine.Enter<GameOverOrParagonState>();
        }

        private void OnBossKilled()
        {
            if (_terminalTransitionWasRequested)
                return;

            _levelProgressService.Complete();
        }

        private void OnLevelCompleted()
        {
            if (_terminalTransitionWasRequested)
                return;

            _terminalTransitionWasRequested = true;
            _stateMachine.Enter<LevelCompleteState>();
        }
    }
}
