using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Zenject;
using _Project.Scripts.Gameplay.Feedback;

namespace _Project.Scripts.Gameplay.Currencies
{
    public class CurrencyBalanceView : MonoBehaviour
    {
        [Inject] private GameplayFeedbackAnimationConfig _animationConfig;
        [Inject] private ICurrencyService _currencyService;

        [SerializeField] private TextMeshProUGUI _isotopesLabel;
        [SerializeField] private TextMeshProUGUI _nucleotidesLabel;

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
            if (_nucleotidesLabel != null)
                _nucleotidesBaseScale = _nucleotidesLabel.rectTransform.localScale;

            if (_isotopesLabel != null)
                _isotopesBaseScale = _isotopesLabel.rectTransform.localScale;

            _currencyService.Changed += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            if (_currencyService != null)
                _currencyService.Changed -= Refresh;

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
                SetLabel(_nucleotidesLabel, _displayedNucleotides);
                SetLabel(_isotopesLabel, _displayedIsotopes);
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
                _nucleotidesBaseScale);

            AnimateLabel(
                _isotopesLabel,
                _displayedIsotopes,
                isotopes,
                value => _displayedIsotopes = value,
                ref _isotopesValueTween,
                ref _isotopesPulseTween,
                _isotopesBaseScale);
        }

        private void AnimateLabel(
            TextMeshProUGUI label,
            int displayedValue,
            int targetValue,
            Action<int> setDisplayedValue,
            ref Tween valueTween,
            ref Tween pulseTween,
            Vector3 baseScale)
        {
            if (label == null || displayedValue == targetValue)
                return;

            int startValue = displayedValue;
            valueTween?.Kill();
            pulseTween?.Kill();

            valueTween = DOTween
                .To(() => startValue, value =>
                {
                    setDisplayedValue(value);
                    SetLabel(label, value);
                }, targetValue, _animationConfig.CurrencyChangeDuration)
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    setDisplayedValue(targetValue);
                    SetLabel(label, targetValue);
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

        private static void SetLabel(TextMeshProUGUI label, int value)
        {
            if (label != null)
                label.text = value.ToString();
        }
    }
}
