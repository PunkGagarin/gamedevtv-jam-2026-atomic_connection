using UnityEngine;
using UnityEngine.UI;
using Zenject;
using _Project.Scripts.Gameplay.Windows;

namespace _Project.Scripts.Gameplay.Talents
{
    public class TalentTreeTestButton : MonoBehaviour
    {
        [field: SerializeField] private Button Button { get; set; }

        [Inject] private IWindowService _windowService;

        private void Awake()
        {
            Button.onClick.AddListener(OpenTalentTree);
        }

        private void OnDestroy()
        {
            Button.onClick.RemoveListener(OpenTalentTree);
        }

        private void OpenTalentTree()
        {
            _windowService.Open(WindowId.TalentTreeWindow);
        }
    }
}
