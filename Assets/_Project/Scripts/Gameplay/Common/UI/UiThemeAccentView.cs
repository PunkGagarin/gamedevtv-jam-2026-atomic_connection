using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;
using _Project.Scripts.GameplayData;

namespace _Project.Scripts.Gameplay.Common.UI
{
    public class UiThemeAccentView : MonoBehaviour
    {
        [SerializeField] private List<TextMeshProUGUI> _accentTextLabels = new();
        [SerializeField] private List<Graphic> _accentGraphics = new();

        [Inject] private UiThemeConfig _themeConfig;

        private void Start() =>
            ApplyTheme();

        public void ApplyTheme()
        {
            if (_themeConfig == null)
                return;

            foreach (TextMeshProUGUI label in _accentTextLabels)
            {
                if (label != null)
                    label.color = _themeConfig.AccentTextColor;
            }

            foreach (Graphic graphic in _accentGraphics)
            {
                if (graphic != null)
                    graphic.color = _themeConfig.AccentGraphicColor;
            }
        }
    }
}
