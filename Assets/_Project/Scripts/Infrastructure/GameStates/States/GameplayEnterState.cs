using _Project.Scripts.Gameplay.Common.Movement;
using _Project.Scripts.Gameplay.Level;
using _Project.Scripts.Gameplay.Talents;
using _Project.Scripts.Gameplay.Units.AtomCores;
using _Project.Scripts.Gameplay.Units.BattleMolecules;
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
            CreateStingerMolecule();
            CreateMembraneMoleculeIfUnlocked();
            CreateSwarmMoleculeIfUnlocked();
            _stateMachine.Enter<GameplayLoopState>();
        }

        private void CreateAtomCore()
        {
            _atomCoreCreator.Create(_levelStartPointProvider.StartPoint);
        }

        private void CreateStingerMolecule()
        {
            _battleMoleculeFactory.CreateStinger(
                MoleculeSpawnPosition(_battleMoleculeConfig.StingerMoleculeSpawnOffset),
                _battleMoleculeConfig);
        }

        private void CreateSwarmMoleculeIfUnlocked()
        {
            if (!_talentService.IsUnlocked(TalentType.SwarmMolecule))
                return;

            _battleMoleculeFactory.CreateSwarm(
                MoleculeSpawnPosition(_battleMoleculeConfig.SwarmMoleculeSpawnOffset),
                _battleMoleculeConfig);
        }

        private void CreateMembraneMoleculeIfUnlocked()
        {
            if (!_talentService.IsUnlocked(TalentType.MembraneMolecule))
                return;

            _battleMoleculeFactory.CreateMembrane(
                MoleculeSpawnPosition(_battleMoleculeConfig.MembraneMoleculeSpawnOffset),
                _battleMoleculeConfig);
        }

        private Vector3 MoleculeSpawnPosition(Vector2 direction)
        {
            Vector3 startPoint = _levelStartPointProvider.StartPoint;
            float angle = direction.sqrMagnitude > Mathf.Epsilon
                ? Mathf.Atan2(direction.y, direction.x)
                : 0f;

            return OrbitMath.PositionOnCircle(startPoint, _battleMoleculeConfig.CoreOrbitRadius, angle, startPoint.z);
        }

        public void Exit()
        {
        }
    }
}
