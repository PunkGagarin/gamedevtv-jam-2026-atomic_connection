using _Project.Scripts.Audio.Domain;
using _Project.Scripts.Utils.Pause;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace _Project.Scripts.Gameplay.Windows
{
    public class GameplayMenuButton : MonoBehaviour
    {
        [field: SerializeField] private Button Button { get; set; }

        [Inject] private PauseService _pauseService;
        [Inject] private IWindowService _windowService;
        [Inject] private AudioService _audio;

        private void Awake() =>
            Button.onClick.AddListener(OpenGameplayMenu);

        private void OnDestroy() =>
            Button.onClick.RemoveListener(OpenGameplayMenu);

        private void OpenGameplayMenu()
        {
            _audio.PlaySound(Sounds.buttonClick);
            _pauseService.SetPaused(true);
            _windowService.Open(WindowId.GameplayMenuWindow);
        }
    }
}
