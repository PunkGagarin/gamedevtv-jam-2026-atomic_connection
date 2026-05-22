using _Project.Scripts.Audio.Domain;
using _Project.Scripts.Gameplay.Currencies;
using _Project.Scripts.Gameplay.Levels;
using _Project.Scripts.Gameplay.Talents;
using _Project.Scripts.Gameplay.Windows;
using _Project.Scripts.Infrastructure.GameStates.StateMachine;
using _Project.Scripts.Infrastructure.GameStates.States;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace _Project.Scripts.MainMenu
{
    public class MainMenuHud : MonoBehaviour
    {
        [field: SerializeField]
        private Button StartGameButton { get; set; }

        [field: SerializeField]
        private Button SettingsButton { get; set; }

        [field: SerializeField]
        private Button UpdateButton { get; set; }

        [field: SerializeField]
        private Image UpgradeNotificationImage { get; set; }

        [field: SerializeField]
        private Button ResetButton { get; set; }

        [field: SerializeField]
        private Button CreditsButton { get; set; }

        [field: SerializeField]
        private Toggle AbTestToggle { get; set; }

        [Inject] private GameStateMachine _stateMachine;
        [Inject] private AudioService _audio;
        [Inject] private IWindowService _windowService;
        [Inject] private ITalentService _talentService;
        [Inject] private ICurrencyService _currencyService;
        [Inject] private ILevelSelectionService _levelSelectionService;

        private void Awake()
        {
            StartGameButton.onClick.AddListener(StartGame);
            SettingsButton.onClick.AddListener(OpenSettings);
            UpdateButton.onClick.AddListener(OpenTalentTree);
            ResetButton.onClick.AddListener(ResetSavedData);
            CreditsButton.onClick.AddListener(OpenCredits);
        }

        private void Start()
        {
            RefreshUpgradeNotification();
        }

        private void OnEnable()
        {
            if (_talentService != null)
                _talentService.Changed += RefreshUpgradeNotification;

            if (_currencyService != null)
                _currencyService.Changed += RefreshUpgradeNotification;
        }

        private void OnDisable()
        {
            if (_talentService != null)
                _talentService.Changed -= RefreshUpgradeNotification;

            if (_currencyService != null)
                _currencyService.Changed -= RefreshUpgradeNotification;
        }

        private void OnDestroy()
        {
            StartGameButton.onClick.RemoveListener(StartGame);
            SettingsButton.onClick.RemoveListener(OpenSettings);
            UpdateButton.onClick.RemoveListener(OpenTalentTree);
            ResetButton.onClick.RemoveListener(ResetSavedData);
            CreditsButton.onClick.RemoveListener(OpenCredits);
        }

        private void RefreshUpgradeNotification()
        {
            if (UpgradeNotificationImage == null || _talentService == null)
                return;

            UpgradeNotificationImage.gameObject.SetActive(_talentService.HasAvailableUpgradeNotification);
        }

        private void StartGame()
        {
            _audio.PlaySound(Sounds.buttonClick);
            _stateMachine.Enter<LoadGameplayState>();
        }

        private void OpenSettings()
        {
            _audio.PlaySound(Sounds.buttonClick);
            _windowService.Open(WindowId.SettingsWindow);
        }

        private void OpenTalentTree()
        {
            _audio.PlaySound(Sounds.buttonClick);
            _windowService.Open(WindowId.TalentTreeWindow);
        }

        private void ResetSavedData()
        {
            _audio.PlaySound(Sounds.buttonClick);
            _talentService.ResetProgress();
            _levelSelectionService.ResetProgress();
        }

        private void OpenCredits()
        {
            _audio.PlaySound(Sounds.buttonClick);
            _windowService.Open(WindowId.CreditsWindow);
        }
    }
}
