using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace _Project.Scripts.Gameplay.Talents
{
    public class TalentConnectionView : MonoBehaviour
    {
        private static readonly Color LockedColor = new(0.25f, 0.06f, 0.05f, 1f);
        private static readonly Color AvailableColor = new(0.95f, 0.82f, 0.28f, 1f);
        private static readonly Color BoughtColor = new(0.55f, 0.95f, 0.45f, 1f);

        [field: SerializeField] private RectTransform RectTransform { get; set; }
        [field: SerializeField] private Image LineImage { get; set; }

        private TalentTreeAnimationConfig _animationConfig;
        private Vector3 _baseScale;
        private Tween _pulseTween;

        public void Initialize(Vector2 from, Vector2 to, TalentTreeAnimationConfig animationConfig)
        {
            _animationConfig = animationConfig;
            Vector2 direction = to - from;
            RectTransform.pivot = new Vector2(0f, 0.5f);
            RectTransform.anchoredPosition = from;
            RectTransform.sizeDelta = new Vector2(direction.magnitude, RectTransform.sizeDelta.y);
            RectTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            _baseScale = RectTransform.localScale;
        }

        public void Refresh(bool parentBought, bool childBought)
        {
            if (LineImage == null)
                return;

            LineImage.color = childBought ? BoughtColor : parentBought ? AvailableColor : LockedColor;
        }

        public void SetVisible(bool visible)
        {
            _pulseTween?.Kill();
            RectTransform.localScale = _baseScale;
            gameObject.SetActive(visible);
        }

        public void PlayUnlockPulse(TweenCallback onComplete)
        {
            if (LineImage == null)
            {
                onComplete?.Invoke();
                return;
            }

            _pulseTween?.Kill();
            gameObject.SetActive(true);
            RectTransform.localScale = new Vector3(0f, _baseScale.y, _baseScale.z);

            _pulseTween = DOTween.Sequence()
                .Append(LineImage.DOColor(AvailableColor, _animationConfig.ConnectionPulseDuration))
                .Join(RectTransform.DOScaleX(_baseScale.x, _animationConfig.ConnectionPulseDuration).SetEase(Ease.OutCubic))
                .Join(RectTransform.DOScaleY(_baseScale.y * _animationConfig.ConnectionPulseScale, _animationConfig.ConnectionPulseDuration).SetEase(Ease.OutSine))
                .Append(RectTransform.DOScaleY(_baseScale.y, _animationConfig.ConnectionPulseDuration).SetEase(Ease.InSine))
                .OnComplete(onComplete);
        }

        private void OnDestroy()
        {
            _pulseTween?.Kill();
        }
    }
}
