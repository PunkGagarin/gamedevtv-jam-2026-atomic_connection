using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Gameplay.Currencies;
using Zenject;
using _Project.Scripts.Infrastructure.SaveLoad;

namespace _Project.Scripts.Gameplay.Talents
{
    public class TalentService : ITalentService, IInitializable
    {
        [Inject] private TalentConfig _config;
        [Inject] private IProgressProvider _progressProvider;
        [Inject] private ISaveLoadService _saveLoadService;
        [Inject] private ICurrencyService _currencyService;
        [Inject] private CurrencyConfig _currencyConfig;

        public event Action Changed;

        public IReadOnlyList<TalentDefinition> Talents => _config.Talents;
        public float AtomGenerationMultiplier => 1f + BonusOf(TalentType.CoreClickReduction);
        public bool HasAvailableUpgradeNotification => _config.Talents.Any(talent => ShouldShowNotification(talent.Id));

        public void Initialize()
        {
            if (_config.ClearSavedProgressOnStartup)
            {
                ResetProgress();
                return;
            }

            if (!_saveLoadService.HasSavedProgress)
                CreateProgressWithStartingCurrencies();
        }

        public int LevelOf(TalentId talentId) =>
            _progressProvider.ProgressData.GetTalentLevel((int)talentId);

        public bool CanBuy(TalentId talentId)
        {
            TalentDefinition talent = DefinitionFor(talentId);
            int currentLevel = LevelOf(talentId);

            return currentLevel < talent.MaxLevel &&
                   _currencyService.CanSpend(talent.PriceForLevel(currentLevel)) &&
                   PrerequisitesBought(talent);
        }

        public bool Buy(TalentId talentId)
        {
            if (!CanBuy(talentId))
                return false;

            TalentDefinition talent = DefinitionFor(talentId);
            int currentLevel = LevelOf(talentId);

            if (!_currencyService.Spend(talent.PriceForLevel(currentLevel)))
                return false;

            _progressProvider.ProgressData.SetTalentLevel((int)talentId, currentLevel + 1);

            _saveLoadService.SaveProgress();
            Changed?.Invoke();

            return true;
        }

        public void ResetProgress()
        {
            _saveLoadService.DeleteAllSavedData();
            CreateProgressWithStartingCurrencies();
            Changed?.Invoke();
        }

        public float BonusOf(TalentType type) =>
            _config.Talents
                .Where(talent => talent.Type == type)
                .Sum(talent => LevelOf(talent.Id) * talent.BonusPerLevel);

        public bool IsUnlocked(TalentType type) =>
            _config.Talents.Any(talent => talent.Type == type && talent.IsUnlock && LevelOf(talent.Id) > 0);

        public bool ShouldShowNotification(TalentId talentId)
        {
            TalentDefinition talent = DefinitionFor(talentId);
            int currentLevel = LevelOf(talentId);

            return currentLevel < talent.MaxLevel &&
                   PrerequisitesBought(talent) &&
                   _currencyService.CanSpend(talent.PriceForLevel(currentLevel));
        }

        private bool PrerequisitesBought(TalentDefinition talent) =>
            talent.Prerequisites == null || talent.Prerequisites.All(prerequisite => LevelOf(prerequisite) > 0);

        private void CreateProgressWithStartingCurrencies()
        {
            _saveLoadService.CreateProgress();
            _currencyService.SetBalance(CurrencyId.Dna, _currencyConfig.StartingAmount(CurrencyId.Dna).Amount);
            _currencyService.SetBalance(CurrencyId.Isotopes, _currencyConfig.StartingAmount(CurrencyId.Isotopes).Amount);
            _currencyService.SetBalance(CurrencyId.Radicals, _currencyConfig.StartingAmount(CurrencyId.Radicals).Amount);
        }

        private TalentDefinition DefinitionFor(TalentId talentId) =>
            _config.Talents.FirstOrDefault(talent => talent.Id == talentId)
            ?? throw new InvalidOperationException($"Talent {talentId} is not present in {nameof(TalentConfig)}.");
    }
}
