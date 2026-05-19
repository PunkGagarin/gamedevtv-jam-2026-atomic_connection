using UnityEngine;

namespace _Project.Scripts.Gameplay.Feedback
{
    [CreateAssetMenu(fileName = "GameplayFeedbackAnimationConfig", menuName = "Game Resources/Configs/Gameplay Feedback Animation")]
    public sealed class GameplayFeedbackAnimationConfig : ScriptableObject
    {
        [field: Header("Currency Balance")]
        [field: SerializeField, Min(0f)] public float CurrencyChangeDuration { get; private set; } = 0.22f;
        [field: SerializeField, Min(1f)] public float CurrencyPulseScale { get; private set; } = 1.12f;
        [field: SerializeField, Min(0f)] public float CurrencyPulseDuration { get; private set; } = 0.12f;

        [field: Header("Damage Popup")]
        [field: SerializeField] public Vector3 DamagePopupWorldOffset { get; private set; } = new(0f, 0.75f, 0f);
        [field: SerializeField] public Color DamagePopupTextColor { get; private set; } = new(1f, 0.25f, 0.2f, 1f);
        [field: SerializeField, Min(0.1f)] public float DamagePopupFontSize { get; private set; } = 4f;
        [field: SerializeField, Min(0f)] public float DamagePopupRiseDistance { get; private set; } = 0.65f;
        [field: SerializeField, Min(0f)] public float DamagePopupDuration { get; private set; } = 0.55f;
        [field: SerializeField] public int DamagePopupSortingOrder { get; private set; } = 50;

        [field: Header("Currency Drop Popup")]
        [field: SerializeField] public Sprite IsotopeDropIcon { get; private set; }
        [field: SerializeField] public Vector3 CurrencyDropPopupWorldOffset { get; private set; } = new(0f, 1.05f, 0f);
        [field: SerializeField] public Color CurrencyDropPopupTextColor { get; private set; } = new(0.55f, 1f, 1f, 1f);
        [field: SerializeField] public Color CurrencyDropPopupIconColor { get; private set; } = Color.white;
        [field: SerializeField, Min(0.1f)] public float CurrencyDropPopupFontSize { get; private set; } = 0.5f;
        [field: SerializeField, Min(0f)] public float CurrencyDropPopupRiseDistance { get; private set; } = 0.75f;
        [field: SerializeField, Min(0f)] public float CurrencyDropPopupDuration { get; private set; } = 0.7f;
        [field: SerializeField, Min(0f)] public float CurrencyDropPopupStartScale { get; private set; } = 0.75f;
        [field: SerializeField, Min(0f)] public float CurrencyDropPopupPulseScale { get; private set; } = 1.15f;
        [field: SerializeField] public int CurrencyDropPopupSortingOrder { get; private set; } = 60;
    }
}
