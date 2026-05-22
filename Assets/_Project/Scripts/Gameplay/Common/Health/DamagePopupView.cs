using DG.Tweening;
using TMPro;
using UnityEngine;
using Zenject;
using _Project.Scripts.Gameplay.Feedback;
using _Project.Scripts.Localization;

namespace _Project.Scripts.Gameplay.Common.Health
{
    [RequireComponent(typeof(Health))]
    public class DamagePopupView : MonoBehaviour
    {
        private const string CRITICAL_DAMAGE_POPUP_KEY = "DAMAGE_POPUP_CRITICAL";

        [Inject] private GameplayFeedbackAnimationConfig _animationConfig;
        [Inject] private LocalizationTool _localizationTool;

        [field: SerializeField] private Health Health { get; set; }

        private void Awake()
        {
            if (Health == null)
                Health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            if (Health != null)
                Health.Damaged += ShowDamage;
        }

        private void OnDisable()
        {
            if (Health != null)
                Health.Damaged -= ShowDamage;
        }

        private void ShowDamage(int damage, bool isCritical)
        {
            if (damage <= 0 || this == null || _animationConfig == null)
                return;

            Vector3 startPosition = transform.position + _animationConfig.DamagePopupWorldOffset;
            Vector3 targetPosition = startPosition + Vector3.up * _animationConfig.DamagePopupRiseDistance;

            GameObject popup = new($"{nameof(DamagePopupView)}Text");
            Transform popupTransform = popup.transform;
            popupTransform.position = startPosition;

            TextMeshPro label = popup.AddComponent<TextMeshPro>();
            label.text = DamageText(damage, isCritical);
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = DamageFontSize(isCritical);
            label.color = DamageTextColor(isCritical);
            label.fontStyle = isCritical ? FontStyles.Bold : FontStyles.Normal;

            MeshRenderer renderer = popup.GetComponent<MeshRenderer>();
            renderer.sortingOrder = _animationConfig.DamagePopupSortingOrder;

            Color transparent = label.color;
            transparent.a = 0f;
            Vector3 currentPosition = startPosition;
            Color currentColor = label.color;

            DOTween.Sequence()
                .Append(DOTween
                    .To(
                        () => currentPosition,
                        position =>
                        {
                            currentPosition = position;

                            if (popupTransform != null)
                                popupTransform.position = position;
                        },
                        targetPosition,
                        _animationConfig.DamagePopupDuration)
                    .SetEase(Ease.OutCubic))
                .Join(DOTween
                    .To(
                        () => currentColor,
                        color =>
                        {
                            currentColor = color;

                            if (label != null)
                                label.color = color;
                        },
                        transparent,
                        _animationConfig.DamagePopupDuration)
                    .SetEase(Ease.InQuad))
                .SetLink(popup)
                .OnComplete(() =>
                {
                    if (popup != null)
                        Destroy(popup);
                });
        }

        private float DamageFontSize(bool isCritical)
        {
            if (!isCritical)
                return _animationConfig.DamagePopupFontSize;

            return _animationConfig.DamagePopupFontSize * _animationConfig.CriticalDamagePopupFontSizeMultiplier;
        }

        private Color DamageTextColor(bool isCritical)
        {
            return isCritical
                ? _animationConfig.CriticalDamagePopupTextColor
                : _animationConfig.DamagePopupTextColor;
        }

        private string DamageText(int damage, bool isCritical)
        {
            string damageText = $"-{damage}";

            if (!isCritical || _localizationTool == null)
                return damageText;

            string criticalText = _localizationTool.GetText(CRITICAL_DAMAGE_POPUP_KEY);
            return string.IsNullOrWhiteSpace(criticalText) || criticalText == "Undefined"
                ? damageText
                : $"{criticalText}\n{damageText}";
        }
    }
}
