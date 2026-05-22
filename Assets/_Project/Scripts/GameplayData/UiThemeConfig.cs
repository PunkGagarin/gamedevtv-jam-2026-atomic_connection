using UnityEngine;

namespace _Project.Scripts.GameplayData
{
    [CreateAssetMenu(fileName = "UiThemeConfig", menuName = "Game Resources/Configs/UI Theme")]
    public class UiThemeConfig : ScriptableObject
    {
        [field: Header("Text")]
        [field: SerializeField] public Color AccentTextColor { get; private set; } = new(0.5518868f, 1f, 0.878471f, 1f);
        [field: SerializeField, Min(0f)] public float CurrencyTextFontSize { get; private set; } = 44.75f;

        [field: Header("Graphics")]
        [field: SerializeField] public Color AccentGraphicColor { get; private set; } = new(0.5518868f, 1f, 0.878471f, 1f);
    }
}
