using DG.Tweening;
using TMPro;
using UnityEngine;
using Zenject;
using _Project.Scripts.Gameplay.Feedback;

namespace _Project.Scripts.Gameplay.Common.Health
{
    [RequireComponent(typeof(Health))]
    public class DamagePopupView : MonoBehaviour
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
            Vector3 targetPosition = startPosition + Vector3.up * _animationConfig.DamagePopupRiseDistance;

            GameObject popup = new($"{nameof(DamagePopupView)}Text");
            Transform popupTransform = popup.transform;
            popupTransform.position = startPosition;

            TextMeshPro label = popup.AddComponent<TextMeshPro>();
            label.text = $"-{damage}";
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = _animationConfig.DamagePopupFontSize;
            label.color = _animationConfig.DamagePopupTextColor;

            MeshRenderer renderer = popup.GetComponent<MeshRenderer>();
            renderer.sortingOrder = _animationConfig.DamagePopupSortingOrder;

            Color transparent = _animationConfig.DamagePopupTextColor;
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
    }
}
