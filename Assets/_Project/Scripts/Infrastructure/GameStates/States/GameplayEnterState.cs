using _Project.Scripts.Gameplay.Level;
using _Project.Scripts.Gameplay.Units.BattleMolecule;
using _Project.Scripts.Gameplay.Units.Example;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Infrastructure.GameStates.States
{
    public class GameplayEnterState : IState, IGameState
    {
        [Inject] private GameStateMachine _stateMachine;
        [Inject] private ILevelStartPointProvider _levelStartPointProvider;
        [Inject] private IExampleUnitFactory _exampleUnitFactory;
        [Inject] private IBattleMoleculeFactory _battleMoleculeFactory;
        [Inject] private BattleMoleculeConfig _battleMoleculeConfig;

        public void Enter()
        {
            CreateExampleUnit();
            CreateBattleMolecule();
            _stateMachine.Enter<GameplayState>();
        }

        private void CreateExampleUnit()
        {
            _exampleUnitFactory.Create(_levelStartPointProvider.StartPoint);
        }

        private void CreateBattleMolecule()
        {
            Vector3 offset = _battleMoleculeConfig.SpawnOffset;
            _battleMoleculeFactory.Create(_levelStartPointProvider.StartPoint + offset);
        }

        public void Exit()
        {
        }
    }
}
