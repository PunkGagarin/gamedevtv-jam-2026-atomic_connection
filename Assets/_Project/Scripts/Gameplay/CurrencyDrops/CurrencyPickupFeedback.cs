using UnityEngine;
using DG.Tweening;
using TMPro;
using _Project.Scripts.Gameplay.Currencies;
using _Project.Scripts.Gameplay.Feedback;

namespace _Project.Scripts.Gameplay.CurrencyDrops
{
    public class CurrencyPickupFeedback : MonoBehaviour
    {
        [field: SerializeField] private SpriteRenderer Icon { get; set; }
        [field: SerializeField] private TextMeshPro CollectPopupLabel { get; set; }

        private Vector3 _baseIconScale;
        private Vector3 _baseCollectPopupScale;
        private Color _baseCollectPopupColor;
        private Tween _idleTween;
        private Tween _collectTween;

        private void Awake()
        {
            if (Icon != null)
                _baseIconScale = Icon.transform.localScale;

            if (CollectPopupLabel != null)
            {
                _baseCollectPopupScale = CollectPopupLabel.transform.localScale;
                _baseCollectPopupColor = CollectPopupLabel.color;
            }
        }

        public void Initialize(CurrencyAmount amount, CurrencyPickupConfig config)
        {
            _idleTween?.Kill();
            _collectTween?.Kill();

            if (Icon != null)
            {
                Sprite icon = config.IconFor(amount.CurrencyId);
                if (icon != null)
                    Icon.sprite = icon;

                Icon.transform.localScale = _baseIconScale;
                Icon.gameObject.SetActive(true);
                PlayIdleAnimation(config);
            }

            if (CollectPopupLabel != null)
            {
                CollectPopupLabel.transform.localScale = _baseCollectPopupScale;
                CollectPopupLabel.color = _baseCollectPopupColor;
                CollectPopupLabel.gameObject.SetActive(false);
            }
        }

        public void PlayCollected(CurrencyAmount amount, CurrencyPickupConfig config)
        {
            _idleTween?.Kill();

            if (!config.ShowCollectPopup || CollectPopupLabel == null)
            {
                PlayCollectIconAnimation(config);
                return;
            }

            if (Icon != null)
                Icon.gameObject.SetActive(false);

            _collectTween?.Kill();
            _collectTween = WorldTextPopupAnimation.PlayExisting(
                CollectPopupLabel,
                $"+{amount.Amount}",
                transform.position + config.CollectPopupWorldOffset,
                _baseCollectPopupScale,
                _baseCollectPopupColor,
                config.CollectPopupRiseDistance,
                config.CollectPopupDuration,
                config.CollectPopupPulseScale,
                gameObject,
                DestroySelf);
        }

        public void PlayVictoryCollected(Vector3 targetWorldPosition, CurrencyPickupConfig config)
        {
            _idleTween?.Kill();
            _collectTween?.Kill();

            if (CollectPopupLabel != null)
                CollectPopupLabel.gameObject.SetActive(false);

            if (Icon == null)
            {
                Destroy(gameObject);
                return;
            }

            float duration = Mathf.Max(0f, config.VictoryAutoCollectDuration);
            if (Mathf.Approximately(duration, 0f))
            {
                DestroySelf();
                return;
            }

            Icon.gameObject.SetActive(true);
            Icon.transform.localScale = _baseIconScale;

            _collectTween = DOTween.Sequence()
                .Append(transform
                    .DOMove(targetWorldPosition, duration)
                    .SetEase(Ease.InCubic))
                .Join(Icon.transform
                    .DOScale(_baseIconScale * Mathf.Max(0f, config.VictoryAutoCollectEndScaleMultiplier), duration)
                    .SetEase(Ease.InBack))
                .SetLink(gameObject)
                .OnComplete(DestroySelf);
        }

        private void PlayCollectIconAnimation(CurrencyPickupConfig config)
        {
            if (Icon == null)
            {
                Destroy(gameObject);
                return;
            }

            _collectTween?.Kill();
            Icon.gameObject.SetActive(true);
            Icon.transform.localScale = _baseIconScale;
            _collectTween = Icon.transform
                .DOScale(_baseIconScale * config.CollectIconScaleMultiplier, config.CollectIconScaleDuration)
                .SetEase(Ease.OutBack)
                .SetLink(gameObject)
                .OnComplete(DestroySelf);
        }

        private void PlayIdleAnimation(CurrencyPickupConfig config)
        {
            _idleTween?.Kill();
            Icon.transform.localScale = _baseIconScale;

            if (!config.EnableIdleScaleAnimation)
                return;

            _idleTween = Icon.transform
                .DOScale(_baseIconScale * config.IdleScaleMultiplier, config.IdleScaleDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(gameObject);
        }

        private void DestroySelf()
        {
            if (this != null)
                Destroy(gameObject);
        }

        private void OnDisable()
        {
            KillTweens();
        }

        private void OnDestroy()
        {
            KillTweens();
        }

        private void KillTweens()
        {
            _idleTween?.Kill();
            _collectTween?.Kill();
        }
    }
}
