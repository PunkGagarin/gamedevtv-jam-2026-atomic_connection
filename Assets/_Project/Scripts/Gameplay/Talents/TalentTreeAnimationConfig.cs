using UnityEngine;

namespace _Project.Scripts.Gameplay.Talents
{
    [CreateAssetMenu(fileName = "TalentTreeAnimationConfig", menuName = "Game Resources/Configs/Talent Tree Animation")]
    public sealed class TalentTreeAnimationConfig : ScriptableObject
    {
        [field: Header("Node Hover")]
        [field: SerializeField, Min(0f)] public float NodeHoverScale { get; private set; } = 1.035f;
        [field: SerializeField, Min(0f)] public float NodeHoverDuration { get; private set; } = 0.12f;

        [field: Header("Node Reveal")]
        [field: SerializeField, Min(0f)] public float NodeRevealStartScale { get; private set; } = 0.72f;
        [field: SerializeField, Min(0f)] public float NodeRevealPulseScale { get; private set; } = 1.12f;
        [field: SerializeField, Min(0f)] public float NodeRevealDuration { get; private set; } = 0.22f;
        [field: SerializeField, Min(0f)] public float NodeRevealFadeDuration { get; private set; } = 0.154f;
        [field: SerializeField, Min(0f)] public float NodeRevealSettleDuration { get; private set; } = 0.18f;

        [field: Header("Node Unlock Pulse")]
        [field: SerializeField, Min(0f)] public float NodeUnlockPulseScale { get; private set; } = 1.12f;
        [field: SerializeField, Min(0f)] public float NodeUnlockPulseDuration { get; private set; } = 0.18f;

        [field: Header("Node Shake")]
        [field: SerializeField, Min(0f)] public float NodeShakeDuration { get; private set; } = 0.18f;
        [field: SerializeField, Min(0f)] public float NodeShakeStrength { get; private set; } = 8f;
        [field: SerializeField, Min(1)] public int NodeShakeVibrato { get; private set; } = 12;
        [field: SerializeField, Min(0f)] public float NodeShakeRandomness { get; private set; } = 65f;

        [field: Header("Tooltip")]
        [field: SerializeField] public Vector2 TooltipOffset { get; private set; } = new(0f, 94f);
        [field: SerializeField, Min(0f)] public float TooltipStartScaleX { get; private set; } = 1f;
        [field: SerializeField, Min(0f)] public float TooltipStartScaleY { get; private set; } = 0.82f;
        [field: SerializeField, Min(0f)] public float TooltipRevealDuration { get; private set; } = 0.14f;

        [field: Header("Connection Pulse")]
        [field: SerializeField, Min(0f)] public float ConnectionPulseDuration { get; private set; } = 0.16f;
        [field: SerializeField, Min(0f)] public float ConnectionPulseScale { get; private set; } = 1.45f;
    }
}
