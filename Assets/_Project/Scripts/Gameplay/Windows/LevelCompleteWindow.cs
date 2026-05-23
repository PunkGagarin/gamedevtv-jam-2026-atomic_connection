using _Project.Scripts.Audio.Domain;
using _Project.Scripts.Gameplay.Currencies;
using _Project.Scripts.Gameplay.Levels;
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
    public class LevelCompleteWindow : BaseWindow
    {
        [field: SerializeField] private GameObject Content { get; set; }
        [field: SerializeField] private Button RestartButton { get; set; }
        [field: SerializeField] private Button MainMenuButton { get; set; }
        [field: SerializeField] private TextMeshProUGUI RewardLabel { get; set; }

        [Inject] private GameStateMachine _stateMachine;
        [Inject] private PauseService _pauseService;
        [Inject] private ILevelProgressService _levelProgressService;
        [Inject] private LocalizationTool _localizationTool;
        [Inject] private AudioService _audio;

        protected override void OnAwake()
        {
            Id = WindowId.LevelCompleteWindow;
        }

        protected override void Initialize()
        {
            Content.SetActive(true);
            CurrencyAmount reward = _levelProgressService.LastCompletionReward;
            string title = _levelProgressService.LastCompletedLevelWasFinal
                ? _localizationTool.GetText("LEVEL_COMPLETE_FINAL_TITLE")
                : _localizationTool.GetText("LEVEL_COMPLETE_TITLE");
            string rewardText = _levelProgressService.LastCompletionWasFirstClear
                ? string.Format(
                    _localizationTool.GetText("LEVEL_COMPLETE_REWARD_FORMAT"),
                    reward.Amount,
                    RewardName(reward.CurrencyId))
                : _localizationTool.GetText("LEVEL_COMPLETE_REWARD_CLAIMED");

            RewardLabel.text = $"{title}\n{rewardText}";
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

        private string RewardName(CurrencyId currencyId)
        {
            string key = currencyId switch
            {
                CurrencyId.Dna => "CURRENCY_DNA",
                CurrencyId.Isotopes => "CURRENCY_ISOTOPES",
                _ => null
            };

            return key == null ? currencyId.ToString() : _localizationTool.GetText(key);
        }
    }
}
