using _Project.Scripts.Infrastructure.GameStates.StateMachine;
using _Project.Scripts.Infrastructure.GameStates.States;
using _Project.Scripts.Utils.Pause;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace _Project.Scripts.Gameplay.Windows
{
    public class GameOverWindow : BaseWindow
    {
        [field: SerializeField] private GameObject Content { get; set; }
        [field: SerializeField] private Button RestartButton { get; set; }
        [field: SerializeField] private Button MainMenuButton { get; set; }

        [Inject] private GameStateMachine _stateMachine;
        [Inject] private PauseService _pauseService;

        protected override void OnAwake()
        {
            Id = WindowId.GameOverWindow;
        }

        protected override void Initialize() =>
            Content.SetActive(true);

        protected override void SubscribeUpdates()
        {
            RestartButton.onClick.AddListener(RestartGameplay);
            MainMenuButton.onClick.AddListener(OpenMainMenu);
        }

        protected override void UnsubscribeUpdates()
        {
            RestartButton.onClick.RemoveListener(RestartGameplay);
            MainMenuButton.onClick.RemoveListener(OpenMainMenu);
        }

        private void OpenMainMenu()
        {
            _pauseService.SetPaused(false);
            _stateMachine.Enter<LoadMainMenuState>();
        }

        private void RestartGameplay()
        {
            _pauseService.SetPaused(false);
            _stateMachine.Enter<LoadGameplayState>();
        }
    }
}
