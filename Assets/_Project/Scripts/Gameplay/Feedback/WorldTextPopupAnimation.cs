using System;
using UnityEngine;
using DG.Tweening;
using TMPro;

namespace _Project.Scripts.Gameplay.Feedback
{
    public static class WorldTextPopupAnimation
    {
        public static Tween PlayExisting(
            TextMeshPro label,
            string text,
            Vector3 startPosition,
            Vector3 baseScale,
            Color baseColor,
            float riseDistance,
            float duration,
            float pulseScale,
            GameObject linkObject,
            Action onComplete)
        {
            if (label == null)
                return null;

            label.gameObject.SetActive(true);
            label.text = text;
            label.color = baseColor;
            label.transform.localScale = baseScale;
            label.transform.position = startPosition;

            Color transparentText = baseColor;
            transparentText.a = 0f;
            Vector3 targetPosition = startPosition + Vector3.up * riseDistance;

            return DOTween.Sequence()
                .Append(label.transform
                    .DOScale(baseScale * pulseScale, duration)
                    .SetEase(Ease.OutBack))
                .Join(label.transform
                    .DOMove(targetPosition, duration)
                    .SetEase(Ease.OutCubic))
                .Join(label
                    .DOColor(transparentText, duration)
                    .SetEase(Ease.InQuad))
                .SetLink(linkObject != null ? linkObject : label.gameObject)
                .OnComplete(() => onComplete?.Invoke());
        }

        public static Tween CreateFloatingText(
            string objectName,
            string text,
            Vector3 startPosition,
            Color color,
            float fontSize,
            int sortingOrder,
            float riseDistance,
            float duration,
            FontStyles fontStyle = FontStyles.Normal)
        {
            GameObject popup = new(objectName);
            TextMeshPro label = popup.AddComponent<TextMeshPro>();
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = fontSize;
            label.fontStyle = fontStyle;

            MeshRenderer renderer = popup.GetComponent<MeshRenderer>();
            renderer.sortingOrder = sortingOrder;

            return PlayExisting(
                label,
                text,
                startPosition,
                popup.transform.localScale,
                color,
                riseDistance,
                duration,
                1f,
                popup,
                () =>
                {
                    if (popup != null)
                        UnityEngine.Object.Destroy(popup);
                });
        }
    }
}
