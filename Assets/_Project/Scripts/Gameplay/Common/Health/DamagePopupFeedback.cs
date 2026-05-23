using UnityEngine;
using Zenject;
using _Project.Scripts.Gameplay.Feedback;

namespace _Project.Scripts.Gameplay.Common.Health
{
    [RequireComponent(typeof(Health))]
    public class DamagePopupFeedback : MonoBehaviour
    {
        [Inject] private GameplayFeedbackAnimationConfig _animationConfig;

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

        private void ShowDamage(int damage)
        {
            if (damage <= 0 || this == null || _animationConfig == null)
                return;

            Vector3 startPosition = transform.position + _animationConfig.DamagePopupWorldOffset;
            WorldTextPopupAnimation.CreateFloatingText(
                $"{nameof(DamagePopupFeedback)}Text",
                $"-{damage}",
                startPosition,
                _animationConfig.DamagePopupTextColor,
                _animationConfig.DamagePopupFontSize,
                _animationConfig.DamagePopupSortingOrder,
                _animationConfig.DamagePopupRiseDistance,
                _animationConfig.DamagePopupDuration);
        }
    }
}
