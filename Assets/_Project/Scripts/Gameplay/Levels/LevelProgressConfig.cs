using _Project.Scripts.Gameplay.Currencies;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Levels
{
    [CreateAssetMenu(fileName = "LevelProgressConfig", menuName = "Game Resources/Configs/Level Progress")]
    public class LevelProgressConfig : ScriptableObject
    {
        [field: SerializeField, Min(0.1f)] public float DurationSeconds { get; private set; } = 30f;
        [field: SerializeField] private CurrencyId RewardCurrency { get; set; } = CurrencyId.Isotopes;
        [field: SerializeField, Min(0)] private int RewardAmount { get; set; } = 1;

        public CurrencyAmount CompletionReward => new(RewardCurrency, RewardAmount);
    }
}
