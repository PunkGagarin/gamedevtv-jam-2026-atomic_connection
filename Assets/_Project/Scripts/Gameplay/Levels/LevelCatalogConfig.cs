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

        public int MaxLevelNumber => ValidLevels()
            .Select(level => level.LevelNumber)
            .DefaultIfEmpty(1)
            .Max();

        public LevelDefinition LevelFor(int levelNumber)
        {
            List<LevelDefinition> levels = ValidLevels().ToList();

            if (levels.Count == 0)
                throw new InvalidOperationException($"{nameof(LevelCatalogConfig)} has no level definitions.");

            LevelDefinition exactLevel = levels.FirstOrDefault(level => level.LevelNumber == levelNumber);

            if (exactLevel != null)
                return exactLevel;

            LevelDefinition fallbackLevel = levels
                .Where(level => level.LevelNumber <= levelNumber)
                .OrderByDescending(level => level.LevelNumber)
                .FirstOrDefault() ?? levels[0];

            Debug.LogWarning($"{nameof(LevelCatalogConfig)} has no data for level {levelNumber}. Using level {fallbackLevel.LevelNumber} data.");
            return fallbackLevel;
        }

        private IEnumerable<LevelDefinition> ValidLevels() =>
            Levels?.Where(level => level != null) ?? Enumerable.Empty<LevelDefinition>();
    }
}
