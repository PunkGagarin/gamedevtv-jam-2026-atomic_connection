using _Project.Scripts.Audio.Domain;
using _Project.Scripts.Gameplay.Levels;
using _Project.Scripts.Localization;
using TMPro;
using UnityEngine;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine.InputSystem;
#endif
using UnityEngine.UI;
using Zenject;

namespace _Project.Scripts.MainMenu
{
    public class MainMenuLevelSelector : MonoBehaviour
    {
        [field: SerializeField] private Button PreviousButton { get; set; }
        [field: SerializeField] private Button NextButton { get; set; }
        [field: SerializeField] private TextMeshProUGUI LevelLabel { get; set; }

        [Inject] private ILevelSelectionService _levelSelectionService;
        [Inject] private LanguageService _languageService;
        [Inject] private LocalizationTool _localizationTool;
        [Inject] private AudioService _audio;

        private void Start()
        {
            PreviousButton.onClick.AddListener(SelectPrevious);
            NextButton.onClick.AddListener(SelectNext);
            _levelSelectionService.Changed += Refresh;
            _languageService.OnSwitchLanguage += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            PreviousButton.onClick.RemoveListener(SelectPrevious);
            NextButton.onClick.RemoveListener(SelectNext);

            if (_levelSelectionService != null)
                _levelSelectionService.Changed -= Refresh;

            if (_languageService != null)
                _languageService.OnSwitchLanguage -= Refresh;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void Update()
        {
            HandleDebugLevelHotkeys();
        }
#endif

        private void SelectPrevious()
        {
            _audio.PlaySound(Sounds.buttonClick);
            _levelSelectionService.SelectPrevious();
        }

        private void SelectNext()
        {
            _audio.PlaySound(Sounds.buttonClick);
            _levelSelectionService.SelectNext();
        }

        private void Refresh()
        {
            LevelLabel.text = string.Format(
                _localizationTool.GetText("MAIN_MENU_LEVEL_FORMAT"),
                _levelSelectionService.SelectedLevel);
            PreviousButton.gameObject.SetActive(_levelSelectionService.CanSelectPrevious);
            NextButton.gameObject.SetActive(_levelSelectionService.CanSelectNext);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void HandleDebugLevelHotkeys()
        {
            Keyboard keyboard = Keyboard.current;

            if (keyboard == null || !IsDebugModifierPressed(keyboard))
                return;

            if (!TryGetPressedLevelHotkey(keyboard, out int level))
                return;

            _levelSelectionService.SelectLevelForDebug(level);
        }

        private bool IsDebugModifierPressed(Keyboard keyboard)
        {
            return (keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed)
                   && (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed);
        }

        private bool TryGetPressedLevelHotkey(Keyboard keyboard, out int level)
        {
            if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame)
            {
                level = 1;
                return true;
            }

            if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame)
            {
                level = 2;
                return true;
            }

            if (keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame)
            {
                level = 3;
                return true;
            }

            if (keyboard.digit4Key.wasPressedThisFrame || keyboard.numpad4Key.wasPressedThisFrame)
            {
                level = 4;
                return true;
            }

            if (keyboard.digit5Key.wasPressedThisFrame || keyboard.numpad5Key.wasPressedThisFrame)
            {
                level = 5;
                return true;
            }

            if (keyboard.digit6Key.wasPressedThisFrame || keyboard.numpad6Key.wasPressedThisFrame)
            {
                level = 6;
                return true;
            }

            if (keyboard.digit7Key.wasPressedThisFrame || keyboard.numpad7Key.wasPressedThisFrame)
            {
                level = 7;
                return true;
            }

            if (keyboard.digit8Key.wasPressedThisFrame || keyboard.numpad8Key.wasPressedThisFrame)
            {
                level = 8;
                return true;
            }

            if (keyboard.digit9Key.wasPressedThisFrame || keyboard.numpad9Key.wasPressedThisFrame)
            {
                level = 9;
                return true;
            }

            level = 0;
            return false;
        }
#endif
    }
}
