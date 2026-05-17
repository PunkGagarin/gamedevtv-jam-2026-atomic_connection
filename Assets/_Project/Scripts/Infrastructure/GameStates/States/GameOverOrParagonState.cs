using _Project.Scripts.Gameplay.Windows;
using _Project.Scripts.Infrastructure.GameStates.StateInfrastructure;
using Zenject;

namespace _Project.Scripts.Infrastructure.GameStates.States
{
    public class GameOverOrParagonState : IState, IGameState
    {
        [Inject] private IWindowService _windowService;

        public void Enter()
        {
            _windowService.Open(WindowId.GameOverWindow);
        }

        public void Exit()
        {
            _windowService.Close(WindowId.GameOverWindow);
        }
    }
}
