using System;
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

        [SerializeField] private TextMeshProUGUI _isotopesLabel;
        [SerializeField] private TextMeshProUGUI _nucleotidesLabel;
        [SerializeField] private string _isotopesKey;
        [SerializeField] private string _nucleotidesKey;

        private int _displayedNucleotides;
        private int _displayedIsotopes;
        private bool _isInitialized;
        private Vector3 _nucleotidesBaseScale;
        private Vector3 _isotopesBaseScale;
        private Tween _nucleotidesValueTween;
        private Tween _isotopesValueTween;
        private Tween _nucleotidesPulseTween;
        private Tween _isotopesPulseTween;

        private void Awake()
        {
            ApplyTheme();

            if (_nucleotidesLabel != null)
                _nucleotidesBaseScale = _nucleotidesLabel.rectTransform.localScale;

            if (_isotopesLabel != null)
                _isotopesBaseScale = _isotopesLabel.rectTransform.localScale;

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

            _nucleotidesValueTween?.Kill();
            _isotopesValueTween?.Kill();
            _nucleotidesPulseTween?.Kill();
            _isotopesPulseTween?.Kill();
        }

        private void Refresh()
        {
            if (_currencyService == null)
                return;

            int nucleotides = _currencyService.BalanceOf(CurrencyId.Nucleotides);
            int isotopes = _currencyService.BalanceOf(CurrencyId.Isotopes);

            if (!_isInitialized)
            {
                _displayedNucleotides = nucleotides;
                _displayedIsotopes = isotopes;
                SetLabel(_nucleotidesLabel, _displayedNucleotides, Localize(_nucleotidesKey));
                SetLabel(_isotopesLabel, _displayedIsotopes, Localize(_isotopesKey));
                _isInitialized = true;
                return;
            }

            AnimateLabel(
                _nucleotidesLabel,
                _displayedNucleotides,
                nucleotides,
                value => _displayedNucleotides = value,
                ref _nucleotidesValueTween,
                ref _nucleotidesPulseTween,
                _nucleotidesBaseScale,
                Localize(_nucleotidesKey));

            AnimateLabel(
                _isotopesLabel,
                _displayedIsotopes,
                isotopes,
                value => _displayedIsotopes = value,
                ref _isotopesValueTween,
                ref _isotopesPulseTween,
                _isotopesBaseScale,
                Localize(_isotopesKey));
        }

        private void AnimateLabel(
            TextMeshProUGUI label,
            int displayedValue,
            int targetValue,
            Action<int> setDisplayedValue,
            ref Tween valueTween,
            ref Tween pulseTween,
            Vector3 baseScale,
            string suffix)
        {
            if (label == null)
                return;

            if (displayedValue == targetValue)
            {
                SetLabel(label, targetValue, suffix);
                return;
            }

            int startValue = displayedValue;
            valueTween?.Kill();
            pulseTween?.Kill();

            valueTween = DOTween
                .To(() => startValue, value =>
                {
                    setDisplayedValue(value);
                    SetLabel(label, value, suffix);
                }, targetValue, _animationConfig.CurrencyChangeDuration)
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    setDisplayedValue(targetValue);
                    SetLabel(label, targetValue, suffix);
                });

            label.rectTransform.localScale = baseScale;
            pulseTween = DOTween.Sequence()
                .Append(label.rectTransform
                    .DOScale(baseScale * _animationConfig.CurrencyPulseScale, _animationConfig.CurrencyPulseDuration)
                    .SetEase(Ease.OutSine))
                .Append(label.rectTransform
                    .DOScale(baseScale, _animationConfig.CurrencyPulseDuration)
                    .SetEase(Ease.InSine));
        }

        private void ApplyTheme()
        {
            ApplyCurrencyFontSize(_nucleotidesLabel);
            ApplyCurrencyFontSize(_isotopesLabel);
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
                label.text = $"{value}{suffix}";
        }

        private string Localize(string key) =>
            string.IsNullOrWhiteSpace(key) ? string.Empty : _localizationTool.GetText(key);
    }
}
