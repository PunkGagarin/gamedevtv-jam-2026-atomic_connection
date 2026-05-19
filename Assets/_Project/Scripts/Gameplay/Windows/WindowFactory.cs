using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using _Project.Scripts.Gameplay.Windows.Configs;

namespace _Project.Scripts.Gameplay.Windows
{
    public class WindowFactory : IWindowFactory
    {
        private const string WINDOWS_CONFIG_PATH = "Configs/Windows/windowConfig";
        private const string MODAL_ROOT_NAME = "WindowModalRoot";
        private const string BACKDROP_NAME = "WindowBackdrop";

        [Inject] private IInstantiator _instantiator;

        private RectTransform _uiRoot;
        private WindowsConfig _config;
        private Dictionary<WindowId, GameObject> _windowPrefabsById;

        public void SetUIRoot(RectTransform uiRoot) =>
            _uiRoot = uiRoot;

        public BaseWindow CreateWindow(WindowId windowId)
        {
            if (_uiRoot == null)
                throw new InvalidOperationException("UIRoot is not set. Add UIInitializer to the scene before opening windows.");

            RectTransform modalRoot = CreateStretchRect(MODAL_ROOT_NAME, _uiRoot);
            WindowBackdrop backdrop = CreateBackdrop(modalRoot, Config.BackdropColor);
            BaseWindow window = _instantiator.InstantiatePrefabForComponent<BaseWindow>(PrefabFor(windowId), modalRoot);

            window.SetModalRoot(modalRoot.gameObject);
            window.transform.SetAsLastSibling();
            backdrop.Initialize(window);

            return window;
        }

        private static WindowBackdrop CreateBackdrop(RectTransform parent, Color backdropColor)
        {
            RectTransform backdropRect = CreateStretchRect(BACKDROP_NAME, parent);
            Image image = backdropRect.gameObject.AddComponent<Image>();
            image.color = backdropColor;
            image.raycastTarget = true;

            return backdropRect.gameObject.AddComponent<WindowBackdrop>();
        }

        private static RectTransform CreateStretchRect(string name, Transform parent)
        {
            GameObject gameObject = new(name, typeof(RectTransform));
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.SetParent(parent, false);
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.localScale = Vector3.one;

            return rectTransform;
        }

        private GameObject PrefabFor(WindowId windowId)
        {
            return WindowPrefabsById.TryGetValue(windowId, out GameObject prefab)
                ? prefab
                : throw new InvalidOperationException($"Prefab config for window {windowId} was not found.");
        }

        private WindowsConfig Config
        {
            get
            {
                if (_config != null)
                    return _config;

                _config = Resources.Load<WindowsConfig>(WINDOWS_CONFIG_PATH);

                return _config != null
                    ? _config
                    : throw new InvalidOperationException($"Windows config was not found at Resources/{WINDOWS_CONFIG_PATH}.");
            }
        }

        private Dictionary<WindowId, GameObject> WindowPrefabsById =>
            _windowPrefabsById ??= Config.WindowConfigs.ToDictionary(x => x.Id, x => x.Prefab);
    }
}
