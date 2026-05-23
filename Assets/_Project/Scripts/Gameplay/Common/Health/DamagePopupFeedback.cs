using UnityEngine;
using Zenject;
using TMPro;
using _Project.Scripts.Gameplay.Feedback;
using _Project.Scripts.Localization;

namespace _Project.Scripts.Gameplay.Common.Health
{
    [RequireComponent(typeof(Health))]
    public class DamagePopupFeedback : MonoBehaviour
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
            WorldTextPopupAnimation.CreateFloatingText(
                $"{nameof(DamagePopupFeedback)}Text",
                DamageText(damage, isCritical),
                startPosition,
                DamageTextColor(isCritical),
                DamageFontSize(isCritical),
                _animationConfig.DamagePopupSortingOrder,
                _animationConfig.DamagePopupRiseDistance,
                _animationConfig.DamagePopupDuration,
                isCritical ? FontStyles.Bold : FontStyles.Normal);
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
