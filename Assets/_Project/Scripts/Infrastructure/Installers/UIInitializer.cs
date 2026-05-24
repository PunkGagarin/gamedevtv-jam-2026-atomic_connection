using UnityEngine;
using Zenject;
using _Project.Scripts.Gameplay.Windows;
using _Project.Scripts.Infrastructure.UI;

namespace _Project.Scripts.Infrastructure.Installers
{
    public class UIInitializer : MonoBehaviour, IInitializable
    {
        [field: SerializeField] private RectTransform UIRoot { get; set; }

        [Inject] private IWindowFactory _windowFactory;
        [Inject] private IUiRootProvider _uiRootProvider;

        public void Initialize()
        {
            _windowFactory.SetUIRoot(UIRoot);
            _uiRootProvider.SetUIRoot(UIRoot);
        }
    }
}
