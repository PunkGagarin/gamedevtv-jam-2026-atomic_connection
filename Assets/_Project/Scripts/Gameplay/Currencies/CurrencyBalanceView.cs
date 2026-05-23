using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Zenject;
using _Project.Scripts.Gameplay.Feedback;
using _Project.Scripts.GameplayData;
using _Project.Scripts.Localization;

namespace _Project.Scripts.Gameplay.Currencies
{
    public class CurrencyBalanceView : MonoBehaviour
    {
        [Inject] private GameplayFeedbackAnimationConfig _animationConfig;
        [Inject] private UiThemeConfig _themeConfig;
        [Inject] private ICurrencyService _currencyService;
        [Inject] private LanguageService _languageService;
        [Inject] private LocalizationTool _localizationTool;

        [field: SerializeField] private List<CurrencyBalanceBinding> Balances { get; set; } = new();

        private readonly Dictionary<CurrencyId, int> _displayedValues = new();
        private readonly Dictionary<CurrencyId, Vector3> _baseScales = new();
        private readonly Dictionary<CurrencyId, Tween> _valueTweens = new();
        private readonly Dictionary<CurrencyId, Tween> _pulseTweens = new();
        private bool _isInitialized;

        private void Awake()
        {
            ApplyTheme();

            foreach (CurrencyBalanceBinding binding in Balances)
            {
                if (binding.Label != null)
                    _baseScales[binding.CurrencyId] = binding.Label.rectTransform.localScale;
            }

            _currencyService.Changed += Refresh;
            _languageService.OnSwitchLanguage += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            if (_currencyService != null)
                _currencyService.Changed -= Refresh;

            if (_languageService != null)
                _languageService.OnSwitchLanguage -= Refresh;

            foreach (Tween tween in _valueTweens.Values)
                tween?.Kill();

            foreach (Tween tween in _pulseTweens.Values)
                tween?.Kill();
        }

        private void Refresh()
        {
            if (_currencyService == null)
                return;

            if (!_isInitialized)
            {
                foreach (CurrencyBalanceBinding binding in Balances)
                {
                    int value = _currencyService.BalanceOf(binding.CurrencyId);
                    _displayedValues[binding.CurrencyId] = value;
                    SetLabel(binding.Label, value, Localize(binding.BalanceSuffixKey));
                }

                _isInitialized = true;
                return;
            }

            foreach (CurrencyBalanceBinding binding in Balances)
                RefreshBinding(binding);
        }

        private void RefreshBinding(CurrencyBalanceBinding binding)
        {
            int displayedValue = _displayedValues.TryGetValue(binding.CurrencyId, out int value) ? value : 0;
            int targetValue = _currencyService.BalanceOf(binding.CurrencyId);
            Vector3 baseScale = _baseScales.TryGetValue(binding.CurrencyId, out Vector3 scale) ? scale : Vector3.one;

            AnimateLabel(binding, displayedValue, targetValue, baseScale);
        }

        private void AnimateLabel(CurrencyBalanceBinding binding, int displayedValue, int targetValue, Vector3 baseScale)
        {
            TextMeshProUGUI label = binding.Label;
            if (label == null)
                return;

            string suffix = Localize(binding.BalanceSuffixKey);

            if (displayedValue == targetValue)
            {
                SetLabel(label, targetValue, suffix);
                return;
            }

            int startValue = displayedValue;
            if (_valueTweens.TryGetValue(binding.CurrencyId, out Tween valueTween))
                valueTween?.Kill();

            if (_pulseTweens.TryGetValue(binding.CurrencyId, out Tween pulseTween))
                pulseTween?.Kill();

            _valueTweens[binding.CurrencyId] = DOTween
                .To(() => startValue, value =>
                {
                    _displayedValues[binding.CurrencyId] = value;
                    SetLabel(label, value, suffix);
                }, targetValue, _animationConfig.CurrencyChangeDuration)
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    _displayedValues[binding.CurrencyId] = targetValue;
                    SetLabel(label, targetValue, suffix);
                });

            label.rectTransform.localScale = baseScale;
            _pulseTweens[binding.CurrencyId] = DOTween.Sequence()
                .Append(label.rectTransform
                    .DOScale(baseScale * _animationConfig.CurrencyPulseScale, _animationConfig.CurrencyPulseDuration)
                    .SetEase(Ease.OutSine))
                .Append(label.rectTransform
                    .DOScale(baseScale, _animationConfig.CurrencyPulseDuration)
                    .SetEase(Ease.InSine));
        }

        private void ApplyTheme()
        {
            foreach (CurrencyBalanceBinding binding in Balances)
                ApplyCurrencyFontSize(binding.Label);
        }

        private void ApplyCurrencyFontSize(TextMeshProUGUI label)
        {
            if (label == null || _themeConfig == null)
                return;

            label.fontSize = _themeConfig.CurrencyTextFontSize;
            label.fontSizeMax = _themeConfig.CurrencyTextFontSize;
        }

        private static void SetLabel(TextMeshProUGUI label, int value, string suffix)
        {
            if (label != null)
                label.text = $"{value} {suffix}";
        }

        private string Localize(string key) =>
            string.IsNullOrWhiteSpace(key) ? string.Empty : _localizationTool.GetText(key);

        [Serializable]
        private class CurrencyBalanceBinding
        {
            [field: SerializeField] public CurrencyId CurrencyId { get; private set; }
            [field: SerializeField] public TextMeshProUGUI Label { get; private set; }
            [field: SerializeField] public string BalanceSuffixKey { get; private set; }
        }
    }
}
