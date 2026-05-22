using DG.Tweening;
using UnityEngine;

public class PulseBehaviour : MonoBehaviour
{
    [field: Header("Pulse")]
    [field: SerializeField]
    private float PulseScale { get; set; } = 1.15f;

    [field: SerializeField]
    private float Duration { get; set; } = 0.5f;

    [field: SerializeField]
    private Ease Ease { get; set; } = Ease.InOutSine;

    [field: SerializeField]
    private bool PlayOnAwake { get; set; } = true;

    private Vector3 _originScale;
    private Tween _tween;

    private void Awake()
    {
        _originScale = transform.localScale;
    }

    private void Start()
    {
        if (PlayOnAwake)
            StartPulse();
    }

    public void StartPulse()
    {
        StopPulse();

        _tween = DOTween.Sequence()
            .Append(transform
                .DOScale(_originScale * Mathf.Max(0f, PulseScale), Mathf.Max(0f, Duration))
                .SetEase(Ease))
            .Append(transform
                .DOScale(_originScale, Mathf.Max(0f, Duration))
                .SetEase(Ease));
    }

    public void StopPulse()
    {
        _tween?.Kill();
        _tween = null;

        if (this != null)
            transform.localScale = _originScale;
    }

    private void OnDestroy()
    {
        _tween?.Kill();
    }
}
