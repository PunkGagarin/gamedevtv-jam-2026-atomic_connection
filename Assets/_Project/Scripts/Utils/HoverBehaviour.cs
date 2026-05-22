using DG.Tweening;
using UnityEngine;

public class HoverBehaviour : MonoBehaviour
{
    [field: Header("Hover")]
    [field: SerializeField]
    private float Scale { get; set; } = 1.1f;

    [field: SerializeField]
    private float Duration { get; set; } = 0.2f;

    [field: SerializeField]
    private Ease Ease { get; set; } = Ease.OutQuad;

    private Vector3 _originalScale;
    private Tween _tween;

    public bool CanHover => this.enabled;

    private void Awake()
    {
        _originalScale = transform.localScale;
    }

    public void HoverStart()
    {
        _tween?.Kill();
        _tween = transform.DOScale(_originalScale * Scale, Duration).SetEase(Ease);
    }

    public void HoverEnd()
    {
        _tween?.Kill();
        _tween = transform.DOScale(_originalScale, Duration).SetEase(Ease);
    }

    private void OnDestroy()
    {
        _tween?.Kill();
    }

    private void OnMouseEnter()
    {
        if (!CanHover)
            return;

        HoverStart();
    }

    private void OnMouseExit()
    {
        if (!CanHover)
            return;

        HoverEnd();
    }
}
