using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;
using _Project.Scripts.Infrastructure.SaveLoad;

namespace _Project.Scripts.Gameplay.Talents
{
    public class TalentService : ITalentService, IInitializable
    {
        [Inject] private TalentConfig _config;
        [Inject] private IProgressProvider _progressProvider;
        [Inject] private ISaveLoadService _saveLoadService;

        public event Action Changed;

        public int Gold => _progressProvider.ProgressData.Gold;
        public IReadOnlyList<TalentDefinition> Talents => _config.Talents;
        public float AtomGenerationMultiplier => 1f + BonusOf(TalentType.AtomGenerationSpeed);

        public void Initialize()
        {
            if (_config.ClearSavedProgressOnStartup)
            {
                ResetProgress();
                return;
            }

            if (!_saveLoadService.HasSavedProgress)
                CreateProgressWithStartingGold();
        }

        public int LevelOf(TalentId talentId) =>
            _progressProvider.ProgressData.GetTalentLevel((int)talentId);

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

            _progressProvider.ProgressData.Gold -= talent.CostForLevel(currentLevel);
            _progressProvider.ProgressData.SetTalentLevel((int)talentId, currentLevel + 1);

            _saveLoadService.SaveProgress();
            Changed?.Invoke();

            return true;
        }

        public void ResetProgress()
        {
            _saveLoadService.DeleteAllSavedData();
            CreateProgressWithStartingGold();
            Changed?.Invoke();
        }

        public float BonusOf(TalentType type) =>
            _config.Talents
                .Where(talent => talent.Type == type)
                .Sum(talent => LevelOf(talent.Id) * talent.BonusPerLevel);

        public bool IsUnlocked(TalentType type) =>
            _config.Talents.Any(talent => talent.Type == type && talent.IsUnlock && LevelOf(talent.Id) > 0);

        private bool PrerequisitesBought(TalentDefinition talent) =>
            talent.Prerequisites == null || talent.Prerequisites.All(prerequisite => LevelOf(prerequisite) > 0);

        private void CreateProgressWithStartingGold()
        {
            _saveLoadService.CreateProgress();
            _progressProvider.ProgressData.Gold = _config.TestStartingGold;
            _saveLoadService.SaveProgress();
        }

        private TalentDefinition DefinitionFor(TalentId talentId) =>
            _config.Talents.FirstOrDefault(talent => talent.Id == talentId)
            ?? throw new InvalidOperationException($"Talent {talentId} is not present in {nameof(TalentConfig)}.");
    }
}
