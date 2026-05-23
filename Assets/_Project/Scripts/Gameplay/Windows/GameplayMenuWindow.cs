using _Project.Scripts.Audio.Domain;
using _Project.Scripts.Infrastructure.GameStates.StateMachine;
using _Project.Scripts.Infrastructure.GameStates.States;
using _Project.Scripts.Utils.Pause;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace _Project.Scripts.Gameplay.Windows
{
    public class GameplayMenuWindow : BaseWindow
    {
        [field: SerializeField] private GameObject Content { get; set; }

        [field: SerializeField]
        private Button RestartButton { get; set; }

        [field: SerializeField]
        private Button MainMenuButton { get; set; }

        [field: SerializeField]
        private Button CloseButton { get; set; }

        [Inject] private GameStateMachine _stateMachine;
        [Inject] private PauseService _pauseService;
        [Inject] private IWindowService _windowService;
        [Inject] private AudioService _audio;

        protected override void OnAwake()
        {
            Id = WindowId.GameplayMenuWindow;
        }

        protected override void Initialize() =>
            Content.SetActive(true);

        public override void OnBackdropClicked() =>
            Close();

        protected override void SubscribeUpdates()
        {
            RestartButton.onClick.AddListener(RestartGameplay);
            MainMenuButton.onClick.AddListener(OpenMainMenu);
            CloseButton.onClick.AddListener(Close);
        }

        protected override void UnsubscribeUpdates()
        {
            RestartButton.onClick.RemoveListener(RestartGameplay);
            MainMenuButton.onClick.RemoveListener(OpenMainMenu);
            CloseButton.onClick.RemoveListener(Close);
        }

        private void OpenMainMenu()
        {
            _audio.PlaySound(Sounds.buttonClick);
            _pauseService.SetPaused(false);
            _windowService.Close(Id);
            _stateMachine.Enter<LoadMainMenuState>();
        }

        private void RestartGameplay()
        {
            _audio.PlaySound(Sounds.buttonClick);
            _pauseService.SetPaused(false);
            _windowService.Close(Id);
            _stateMachine.Enter<LoadGameplayState>();
        }

        private void Close()
        {
            _audio.PlaySound(Sounds.buttonClick);
            _pauseService.SetPaused(false);
            _windowService.Close(Id);
        }
    }
}
