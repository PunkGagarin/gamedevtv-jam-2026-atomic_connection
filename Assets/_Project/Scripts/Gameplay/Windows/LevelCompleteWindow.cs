using _Project.Scripts.Gameplay.Currencies;
using _Project.Scripts.Gameplay.Levels;
using _Project.Scripts.Infrastructure.GameStates.StateMachine;
using _Project.Scripts.Infrastructure.GameStates.States;
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

        protected override void OnAwake()
        {
            Id = WindowId.LevelCompleteWindow;
        }

        protected override void Initialize()
        {
            Content.SetActive(true);
            CurrencyAmount reward = _levelProgressService.LastCompletionReward;
            RewardLabel.text = $"LEVEL COMPLETE\nReward: +{reward.Amount} {RewardName(reward.CurrencyId)}";
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
            _pauseService.SetPaused(false);
            _stateMachine.Enter<LoadMainMenuState>();
        }

        private void RestartGameplay()
        {
            _pauseService.SetPaused(false);
            _stateMachine.Enter<LoadGameplayState>();
        }

        private static string RewardName(CurrencyId currencyId)
        {
            return currencyId switch
            {
                CurrencyId.Nucleotides => "nucleotides",
                CurrencyId.Isotopes => "isotopes",
                _ => currencyId.ToString()
            };
        }
    }
}
