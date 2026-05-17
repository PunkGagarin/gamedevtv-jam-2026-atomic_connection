using _Project.Scripts.Audio.Domain;
using _Project.Scripts.Infrastructure.GameStates.StateInfrastructure;
using _Project.Scripts.Infrastructure.GameStates.StateMachine;
using Zenject;

namespace _Project.Scripts.Infrastructure.GameStates.States
{
    public class BootstrapState : IState, IGameState
    {
        [Inject] private GameStateMachine _gameStateMachine;
        [Inject] private AudioService _audioService;

        public void Enter()
        {
            // show curtain
            // some resource loading
            // asset provider loading etc
            // init all project context systems

            _audioService.Init();
            _gameStateMachine.Enter<LoadMainMenuState>();
        }

        public void Exit()
        {
        }
    }
}
