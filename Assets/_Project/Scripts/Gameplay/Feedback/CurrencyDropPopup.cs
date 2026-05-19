using UnityEngine;
using DG.Tweening;
using TMPro;
using _Project.Scripts.Gameplay.Currencies;

namespace _Project.Scripts.Gameplay.Feedback
{
    public class CurrencyDropPopup : MonoBehaviour
    {
        [field: SerializeField] private TextMeshPro Label { get; set; }
        [field: SerializeField] private SpriteRenderer Icon { get; set; }

        public void Play(CurrencyAmount amount, GameplayFeedbackAnimationConfig config)
        {
            if (amount.CurrencyId != CurrencyId.Isotopes || amount.Amount <= 0 || config == null)
            {
                Destroy(gameObject);
                return;
            }

            Configure(amount, config);
            PlayAnimation(config);
        }

        private void Configure(CurrencyAmount amount, GameplayFeedbackAnimationConfig config)
        {
            transform.localScale = Vector3.one * config.CurrencyDropPopupStartScale;

            if (Label != null)
            {
                Label.text = $"+{amount.Amount}";
                Label.alignment = TextAlignmentOptions.Center;
                Label.fontSize = config.CurrencyDropPopupFontSize;
                Label.color = config.CurrencyDropPopupTextColor;

                if (Label.TryGetComponent(out MeshRenderer labelRenderer))
                    labelRenderer.sortingOrder = config.CurrencyDropPopupSortingOrder + 1;
            }

            if (Icon != null)
            {
                Icon.sprite = config.IsotopeDropIcon;
                Icon.color = config.CurrencyDropPopupIconColor;
                Icon.sortingOrder = config.CurrencyDropPopupSortingOrder;
                Icon.gameObject.SetActive(config.IsotopeDropIcon != null);
            }
        }

        private void PlayAnimation(GameplayFeedbackAnimationConfig config)
        {
            Vector3 startPosition = transform.position;
            Vector3 targetPosition = startPosition + Vector3.up * config.CurrencyDropPopupRiseDistance;
            Vector3 currentPosition = startPosition;
            Vector3 currentScale = transform.localScale;
            Color currentTextColor = Label != null ? Label.color : Color.clear;
            Color currentIconColor = Icon != null ? Icon.color : Color.clear;

            Color transparentText = config.CurrencyDropPopupTextColor;
            transparentText.a = 0f;

            Color transparentIcon = config.CurrencyDropPopupIconColor;
            transparentIcon.a = 0f;

            DOTween.Sequence()
                .Append(DOTween
                    .To(
                        () => currentScale,
                        scale =>
                        {
                            currentScale = scale;

                            if (this != null)
                                transform.localScale = scale;
                        },
                        Vector3.one * config.CurrencyDropPopupPulseScale,
                        config.CurrencyDropPopupDuration * 0.35f)
                    .SetEase(Ease.OutBack))
                .Join(DOTween
                    .To(
                        () => currentPosition,
                        position =>
                        {
                            currentPosition = position;

                            if (this != null)
                                transform.position = position;
                        },
                        targetPosition,
                        config.CurrencyDropPopupDuration)
                    .SetEase(Ease.OutCubic))
                .Join(DOTween
                    .To(
                        () => currentTextColor,
                        color =>
                        {
                            currentTextColor = color;

                            if (Label != null)
                                Label.color = color;
                        },
                        transparentText,
                        config.CurrencyDropPopupDuration)
                    .SetEase(Ease.InQuad))
                .Join(DOTween
                    .To(
                        () => currentIconColor,
                        color =>
                        {
                            currentIconColor = color;

                            if (Icon != null)
                                Icon.color = color;
                        },
                        transparentIcon,
                        config.CurrencyDropPopupDuration)
                    .SetEase(Ease.InQuad))
                .SetLink(gameObject)
                .OnComplete(() =>
                {
                    if (this != null)
                        Destroy(gameObject);
                });
        }
    }
}
