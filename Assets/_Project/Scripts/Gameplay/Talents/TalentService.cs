using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Talents
{
    public class TalentService : ITalentService, IInitializable
    {
        private const string GOLD_KEY = "AtomicConnection.Talents.Gold";
        private const string TALENT_LEVEL_KEY_PREFIX = "AtomicConnection.Talents.Level.";

        [Inject] private TalentConfig _config;

        private readonly Dictionary<TalentId, int> _levelsById = new();

        public event Action Changed;

        public int Gold { get; private set; }
        public IReadOnlyList<TalentDefinition> Talents => _config.Talents;
        public float AtomGenerationMultiplier => 1f + BonusOf(TalentType.AtomGenerationSpeed);

        public void Initialize()
        {
            if (_config.ClearSavedProgressOnStartup)
                ClearSavedProgress();

            Gold = PlayerPrefs.HasKey(GOLD_KEY)
                ? PlayerPrefs.GetInt(GOLD_KEY)
                : _config.TestStartingGold;

            foreach (TalentDefinition talent in _config.Talents)
                _levelsById[talent.Id] = PlayerPrefs.GetInt(LevelKey(talent.Id), 0);
        }

        public int LevelOf(TalentId talentId) =>
            _levelsById.TryGetValue(talentId, out int level) ? level : 0;

        public bool CanBuy(TalentId talentId)
        {
            TalentDefinition talent = DefinitionFor(talentId);
            int currentLevel = LevelOf(talentId);

            return currentLevel < talent.MaxLevel &&
                   Gold >= talent.CostForLevel(currentLevel) &&
                   PrerequisitesBought(talent);
        }

        public bool Buy(TalentId talentId)
        {
            if (!CanBuy(talentId))
                return false;

            TalentDefinition talent = DefinitionFor(talentId);
            int currentLevel = LevelOf(talentId);

            Gold -= talent.CostForLevel(currentLevel);
            _levelsById[talentId] = currentLevel + 1;

            Save();
            Changed?.Invoke();

            return true;
        }

        public float BonusOf(TalentType type) =>
            _config.Talents
                .Where(talent => talent.Type == type)
                .Sum(talent => LevelOf(talent.Id) * talent.BonusPerLevel);

        public bool IsUnlocked(TalentType type) =>
            _config.Talents.Any(talent => talent.Type == type && talent.IsUnlock && LevelOf(talent.Id) > 0);

        private bool PrerequisitesBought(TalentDefinition talent) =>
            talent.Prerequisites == null || talent.Prerequisites.All(prerequisite => LevelOf(prerequisite) > 0);

        private TalentDefinition DefinitionFor(TalentId talentId) =>
            _config.Talents.FirstOrDefault(talent => talent.Id == talentId)
            ?? throw new InvalidOperationException($"Talent {talentId} is not present in {nameof(TalentConfig)}.");

        private void Save()
        {
            PlayerPrefs.SetInt(GOLD_KEY, Gold);

            foreach (KeyValuePair<TalentId, int> levelById in _levelsById)
                PlayerPrefs.SetInt(LevelKey(levelById.Key), levelById.Value);

            PlayerPrefs.Save();
        }

        private void ClearSavedProgress()
        {
            PlayerPrefs.DeleteKey(GOLD_KEY);

            foreach (TalentDefinition talent in _config.Talents)
                PlayerPrefs.DeleteKey(LevelKey(talent.Id));

            PlayerPrefs.Save();
        }

        private string LevelKey(TalentId talentId) =>
            $"{TALENT_LEVEL_KEY_PREFIX}{talentId}";
    }
}
