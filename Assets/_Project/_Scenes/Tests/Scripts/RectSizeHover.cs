using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class RectSizeHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [field: SerializeField] private RectTransform Target { get; set; }
    [field: SerializeField] private float HoverScale { get; set; } = 1.1f;

    [Header("Enter")]
    [field: SerializeField] private float EnterDuration { get; set; } = 0.2f;
    [field: SerializeField] private Ease EnterEase { get; set; } = Ease.OutQuad;

    [Header("Exit")]
    [field: SerializeField] private float ExitDuration { get; set; } = 0.15f;
    [field: SerializeField] private Ease ExitEase { get; set; } = Ease.OutQuad;

    private Tweener _enterTween;
    private Tweener _exitTween;
    private Vector3 _originalScale;

    private void Awake()
    {
        if (Target == null)
            Target = (RectTransform)transform;

        _originalScale = Target.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _exitTween?.Kill();
        _enterTween?.Kill();
        _enterTween = Target.DOScale(_originalScale * HoverScale, EnterDuration)
            .SetEase(EnterEase);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _enterTween?.Kill();
        _exitTween?.Kill();
        _exitTween = Target.DOScale(_originalScale, ExitDuration)
            .SetEase(ExitEase);
    }

    private void OnDisable()
    {
        _enterTween?.Kill();
        _enterTween = null;
        _exitTween?.Kill();
        _exitTween = null;

        if (Target != null)
            Target.localScale = _originalScale;
    }
}
