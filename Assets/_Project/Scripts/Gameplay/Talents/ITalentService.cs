using System;
using System.Collections.Generic;
using _Project.Scripts.Gameplay.Currencies;

namespace _Project.Scripts.Gameplay.Talents
{
    public interface ITalentService
    {
        event Action Changed;

        IReadOnlyList<TalentDefinition> Talents { get; }
        float AtomGenerationMultiplier { get; }
        bool HasAvailableUpgradeNotification { get; }

        int LevelOf(TalentId talentId);
        CurrencyAmount PriceFor(TalentId talentId);
        bool CanBuy(TalentId talentId);
        bool Buy(TalentId talentId);
        void ResetProgress();
        float BonusOf(TalentType type);
        bool IsUnlocked(TalentType type);
        bool ShouldShowNotification(TalentId talentId);
    }
}
