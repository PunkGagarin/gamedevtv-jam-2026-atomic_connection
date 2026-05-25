using _Project.Scripts.Gameplay.Levels;
using _Project.Scripts.Gameplay.Windows;
using _Project.Scripts.Infrastructure.GameStates.StateInfrastructure;
using Zenject;

namespace _Project.Scripts.Infrastructure.GameStates.States
{
    public class LevelCompleteState : IState, IGameState
    {
        [Inject] private ILevelProgressService _levelProgressService;
        [Inject] private IWindowService _windowService;

        private WindowId _openedWindowId;

        public void Enter()
        {
            _openedWindowId = _levelProgressService.LastCompletedLevelWasFinal
                ? WindowId.GameCompleteWindow
                : WindowId.LevelCompleteWindow;

            _windowService.Open(_openedWindowId);
        }

        public void Exit()
        {
            _windowService.Close(_openedWindowId);
        }
    }
}
