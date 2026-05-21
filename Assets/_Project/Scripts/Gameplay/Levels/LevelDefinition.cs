using System.Collections.Generic;
using _Project.Scripts.Gameplay.Currencies;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Levels
{
    [CreateAssetMenu(fileName = "LevelDefinition", menuName = "Game Resources/Configs/Level Definition")]
    public class LevelDefinition : ScriptableObject
    {
        [field: SerializeField, Min(1)] public int LevelNumber { get; private set; } = 1;
        [field: SerializeField, Min(0.1f)] public float DurationSeconds { get; private set; } = 30f;
        [field: SerializeField, Min(0f)] public float OffscreenSpawnPadding { get; private set; } = 1f;
        [field: SerializeField] private CurrencyId RewardCurrency { get; set; } = CurrencyId.Isotopes;
        [field: SerializeField, Min(0)] private int RewardAmount { get; set; } = 1;
        [field: SerializeField] public List<LevelWaveDefinition> Waves { get; private set; } = new();

        public CurrencyAmount CompletionReward => new(RewardCurrency, RewardAmount);
    }
}
