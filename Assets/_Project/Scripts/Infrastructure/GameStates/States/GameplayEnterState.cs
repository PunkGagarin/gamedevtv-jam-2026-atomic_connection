using _Project.Scripts.Gameplay.Level;
using _Project.Scripts.Gameplay.Units.BattleMolecules;
using _Project.Scripts.Gameplay.Units.AtomCores;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Infrastructure.GameStates.States
{
    public class GameplayEnterState : IState, IGameState
    {
        [Inject] private GameStateMachine _stateMachine;
        [Inject] private ILevelStartPointProvider _levelStartPointProvider;
        [Inject] private IAtomCoreFactory _atomCoreFactory;
        [Inject] private IBattleMoleculeFactory _battleMoleculeFactory;
        [Inject] private BattleMoleculeConfig _battleMoleculeConfig;

        public void Enter()
        {
            CreateAtomCore();
            CreateBattleMolecule();
            _stateMachine.Enter<GameplayState>();
        }

        private void CreateAtomCore()
        {
            _atomCoreFactory.Create(_levelStartPointProvider.StartPoint);
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
