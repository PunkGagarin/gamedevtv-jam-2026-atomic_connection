using _Project.Scripts.Gameplay.Units.AtomCores;
using Cinemachine;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class CameraShaker : MonoBehaviour
{
    [SerializeField]
    private CinemachineVirtualCamera camera;

    [SerializeField]
    private CinemachineBasicMultiChannelPerlin noise;

    public float Duration = 3f;

    public float minForce = 6f;
    public float maxForce = 10f;

    public float minDamage = 1f;
    public float maxDamage = 10f;

    private void Awake()
    {
        if (camera == null)
        {
            Debug.LogError("Camera ref is null");
            return;
        }

        noise = camera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        if (noise == null)
        {
            Debug.LogError("Missing CinemachineBasicMultiChannelPerlin component");
        }

        AtomCoreEventBus.OnDamageEvent += OnDamage;
    }

    void OnDestroy()
    {
        AtomCoreEventBus.OnDamageEvent -= OnDamage;
    }

    // TODO 1: вынести в конфиги параметры
    private void OnDamage(float amount)
    {
        float t = Mathf.InverseLerp(minDamage, maxDamage, amount);

        float force = Mathf.Lerp(minForce, maxForce, t);

        Shake(3f, force, Ease.OutExpo);
    }

    public void Shake(float duration, float force, Ease ease)
    {
        if (noise == null)
            return;

        // Kill previous shake tween
        DOTween.Kill(noise);

        noise.m_AmplitudeGain = force;
        noise.m_FrequencyGain = force;

        DOTween
            .To(() => noise.m_AmplitudeGain, x => noise.m_AmplitudeGain = x, 0f, duration)
            .SetEase(ease)
            .SetTarget(noise);

        DOTween
            .To(() => noise.m_FrequencyGain, x => noise.m_FrequencyGain = x, 0f, duration)
            .SetEase(ease)
            .SetTarget(noise);
    }
}
