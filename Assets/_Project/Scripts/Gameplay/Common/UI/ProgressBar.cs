using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Gameplay.UI
{
    public class ProgressBar : MonoBehaviour
    {
        [field: Header("Progress Bar")]
        [field: SerializeField] private Image Fill { get; set; }
        [field: SerializeField, Min(0f)] private float Duration { get; set; } = 0.3f;
        [field: SerializeField] private Ease Ease { get; set; } = Ease.OutQuad;

        private Tween _tween;

        public void SetProgress(float value)
        {
            if (Fill == null)
                return;

            value = Mathf.Clamp01(value);

            _tween?.Kill();

            float current = Fill.rectTransform.anchorMax.x;
            if (Mathf.Approximately(current, value))
                return;

            _tween = DOTween
                .To(() => Fill.rectTransform.anchorMax.x,
                    x =>
                    {
                        Vector2 anchorMax = Fill.rectTransform.anchorMax;
                        anchorMax.x = x;
                        Fill.rectTransform.anchorMax = anchorMax;
                    },
                    value, Duration)
                .SetEase(Ease)
                .SetTarget(Fill);
        }

        private void OnDestroy()
        {
            _tween?.Kill();
        }
    }
}
