using _Project.Scripts.Gameplay.Level;
using _Project.Scripts.Gameplay.Talents;
using _Project.Scripts.Gameplay.Units.BattleMolecules;
using _Project.Scripts.Gameplay.Units.AtomCores;
using _Project.Scripts.Infrastructure.GameStates.StateInfrastructure;
using _Project.Scripts.Infrastructure.GameStates.StateMachine;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Infrastructure.GameStates.States
{
    public class GameplayEnterState : IState, IGameState
    {
        [Inject] private GameStateMachine _stateMachine;
        [Inject] private ILevelStartPointProvider _levelStartPointProvider;
        [Inject] private IAtomCoreCreator _atomCoreCreator;
        [Inject] private IBattleMoleculeFactory _battleMoleculeFactory;
        [Inject] private BattleMoleculeConfig _battleMoleculeConfig;
        [Inject] private ITalentService _talentService;

        public void Enter()
        {
            CreateAtomCore();
            CreateBattleMolecule();
            CreateMassBattleMoleculeIfUnlocked();
            _stateMachine.Enter<GameplayLoopState>();
        }

        private void CreateAtomCore()
        {
            _atomCoreCreator.Create(_levelStartPointProvider.StartPoint);
        }

        private void CreateBattleMolecule()
        {
            Vector3 offset = _battleMoleculeConfig.SpawnOffset;
            _battleMoleculeFactory.Create(_levelStartPointProvider.StartPoint + offset, _battleMoleculeConfig);
        }

        private void CreateMassBattleMoleculeIfUnlocked()
        {
            if (!_talentService.IsUnlocked(TalentType.MassMolecule))
                return;

            Vector3 offset = _battleMoleculeConfig.MassMoleculeSpawnOffset;
            _battleMoleculeFactory.CreateMass(_levelStartPointProvider.StartPoint + offset, _battleMoleculeConfig);
        }

        public void Exit()
        {
        }
    }
}
