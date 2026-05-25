using _Project.Scripts.Audio.Domain;
using _Project.Scripts.Infrastructure.GameStates.StateMachine;
using _Project.Scripts.Infrastructure.GameStates.States;
using _Project.Scripts.Localization;
using _Project.Scripts.Utils.Pause;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace _Project.Scripts.Gameplay.Windows
{
    public class GameCompleteWindow : BaseWindow
    {
        private const string GAME_COMPLETE_MESSAGE_KEY = "GAME_COMPLETE_MESSAGE";

        [field: SerializeField] private GameObject Content { get; set; }
        [field: SerializeField] private Button RestartButton { get; set; }
        [field: SerializeField] private Button MainMenuButton { get; set; }
        [field: SerializeField] private TextMeshProUGUI MessageLabel { get; set; }

        [Inject] private GameStateMachine _stateMachine;
        [Inject] private PauseService _pauseService;
        [Inject] private LocalizationTool _localizationTool;
        [Inject] private AudioService _audio;

        protected override void OnAwake()
        {
            Id = WindowId.GameCompleteWindow;
        }

        protected override void Initialize()
        {
            Content.SetActive(true);
            MessageLabel.text = _localizationTool.GetText(GAME_COMPLETE_MESSAGE_KEY);
        }

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
            _audio.PlaySound(Sounds.buttonClick);
            _pauseService.SetPaused(false);
            _stateMachine.Enter<LoadMainMenuState>();
        }

        private void RestartGameplay()
        {
            _audio.PlaySound(Sounds.buttonClick);
            _pauseService.SetPaused(false);
            _stateMachine.Enter<LoadGameplayState>();
        }
    }
}
