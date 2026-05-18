using _Project.Scripts.Gameplay.Levels;
using TMPro;
using UnityEngine;
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

        private void Start()
        {
            PreviousButton.onClick.AddListener(SelectPrevious);
            NextButton.onClick.AddListener(SelectNext);
            _levelSelectionService.Changed += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            PreviousButton.onClick.RemoveListener(SelectPrevious);
            NextButton.onClick.RemoveListener(SelectNext);

            if (_levelSelectionService != null)
                _levelSelectionService.Changed -= Refresh;
        }

        private void SelectPrevious()
        {
            _levelSelectionService.SelectPrevious();
        }

        private void SelectNext()
        {
            _levelSelectionService.SelectNext();
        }

        private void Refresh()
        {
            LevelLabel.text = $"Level {_levelSelectionService.SelectedLevel}";
            PreviousButton.gameObject.SetActive(_levelSelectionService.CanSelectPrevious);
            NextButton.gameObject.SetActive(_levelSelectionService.CanSelectNext);
        }
    }
}
