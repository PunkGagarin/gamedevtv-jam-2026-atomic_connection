using UnityEngine;
using DG.Tweening;
using TMPro;
using _Project.Scripts.Gameplay.Currencies;

namespace _Project.Scripts.Gameplay.CurrencyDrops
{
    [RequireComponent(typeof(Collider2D))]
    public class CurrencyPickupView : MonoBehaviour
    {
        [field: SerializeField] private SpriteRenderer Icon { get; set; }
        [field: SerializeField] private TextMeshPro CollectPopupLabel { get; set; }

        private CurrencyAmount _amount;
        private Vector3 _baseIconScale;
        private Vector3 _baseCollectPopupScale;
        private Color _baseCollectPopupColor;
        private Collider2D _pickupCollider;
        private Tween _idleTween;
        private Tween _collectTween;

        public CurrencyAmount Amount => _amount;

        private void Awake()
        {
            _pickupCollider = GetComponent<Collider2D>();
        }

        public void Initialize(CurrencyAmount amount, CurrencyPickupConfig config)
        {
            _amount = amount;

            if (Icon != null)
            {
                _baseIconScale = Icon.transform.localScale;
                Sprite icon = config.IconFor(amount.CurrencyId);
                if (icon != null)
                    Icon.sprite = icon;

                Icon.gameObject.SetActive(true);
                PlayIdleAnimation(config);
            }

            if (CollectPopupLabel != null)
            {
                _baseCollectPopupScale = CollectPopupLabel.transform.localScale;
                _baseCollectPopupColor = CollectPopupLabel.color;
                CollectPopupLabel.gameObject.SetActive(false);
            }
        }

        public bool IsInsidePickupArea(Vector3 areaCenter, float areaHalfSize)
        {
            Vector3 pickupCenter = _pickupCollider != null
                ? _pickupCollider.bounds.center
                : transform.position;

            float halfSize = Mathf.Max(0f, areaHalfSize);
            return Mathf.Abs(pickupCenter.x - areaCenter.x) <= halfSize
                   && Mathf.Abs(pickupCenter.y - areaCenter.y) <= halfSize;
        }

        public void PlayCollected(CurrencyPickupConfig config)
        {
            DisableCollecting();
            _idleTween?.Kill();

            if (!config.ShowCollectPopup || CollectPopupLabel == null)
            {
                PlayCollectIconAnimation(config);
                return;
            }

            if (Icon != null)
                Icon.gameObject.SetActive(false);

            ConfigureCollectPopup(config);
            PlayCollectPopupAnimation(config);
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

        private void ConfigureCollectPopup(CurrencyPickupConfig config)
        {
            CollectPopupLabel.gameObject.SetActive(true);
            CollectPopupLabel.text = $"+{_amount.Amount}";
            CollectPopupLabel.color = _baseCollectPopupColor;
            CollectPopupLabel.transform.localScale = _baseCollectPopupScale;
            CollectPopupLabel.transform.position = transform.position + config.CollectPopupWorldOffset;
        }

        private void PlayCollectPopupAnimation(CurrencyPickupConfig config)
        {
            Vector3 startPosition = CollectPopupLabel.transform.position;
            Vector3 targetPosition = startPosition + Vector3.up * config.CollectPopupRiseDistance;
            Color transparentText = _baseCollectPopupColor;
            transparentText.a = 0f;

            _collectTween?.Kill();
            _collectTween = DOTween.Sequence()
                .Append(CollectPopupLabel.transform
                    .DOScale(_baseCollectPopupScale * config.CollectPopupPulseScale, config.CollectPopupDuration)
                    .SetEase(Ease.OutBack))
                .Join(CollectPopupLabel.transform
                    .DOMove(targetPosition, config.CollectPopupDuration)
                    .SetEase(Ease.OutCubic))
                .Join(CollectPopupLabel
                    .DOColor(transparentText, config.CollectPopupDuration)
                    .SetEase(Ease.InQuad))
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

        private void DisableCollecting()
        {
            if (_pickupCollider != null)
                _pickupCollider.enabled = false;
        }

        private void DestroySelf()
        {
            if (this != null)
                Destroy(gameObject);
        }

        private void OnDestroy()
        {
            _idleTween?.Kill();
            _collectTween?.Kill();
        }
    }
}
