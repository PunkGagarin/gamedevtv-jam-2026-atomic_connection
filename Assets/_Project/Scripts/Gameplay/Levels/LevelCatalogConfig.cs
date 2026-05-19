using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Levels
{
    [CreateAssetMenu(fileName = "LevelCatalogConfig", menuName = "Game Resources/Configs/Level Catalog")]
    public class LevelCatalogConfig : ScriptableObject
    {
        [field: SerializeField] public List<LevelDefinition> Levels { get; private set; } = new();

        public int MaxLevelNumber => Levels == null || Levels.Count == 0
            ? 1
            : Levels.Max(level => level.LevelNumber);

        public LevelDefinition LevelFor(int levelNumber)
        {
            if (Levels == null || Levels.Count == 0)
                throw new InvalidOperationException($"{nameof(LevelCatalogConfig)} has no level definitions.");

            LevelDefinition exactLevel = Levels.FirstOrDefault(level => level.LevelNumber == levelNumber);

            if (exactLevel != null)
                return exactLevel;

            LevelDefinition fallbackLevel = Levels
                .Where(level => level.LevelNumber <= levelNumber)
                .OrderByDescending(level => level.LevelNumber)
                .FirstOrDefault() ?? Levels[0];

            Debug.LogWarning($"{nameof(LevelCatalogConfig)} has no data for level {levelNumber}. Using level {fallbackLevel.LevelNumber} data.");
            return fallbackLevel;
        }
    }
}
