using System;
using System.Collections.Generic;

namespace _Project.Scripts.Gameplay.Talents
{
    public interface ITalentService
    {
        event Action Changed;

        IReadOnlyList<TalentDefinition> Talents { get; }
        float AtomGenerationMultiplier { get; }
        bool HasAvailableUpgradeNotification { get; }

        int LevelOf(TalentId talentId);
        bool CanBuy(TalentId talentId);
        bool Buy(TalentId talentId);
        void ResetProgress();
        float BonusOf(TalentEffectType effectType);
        bool IsUnlocked(TalentEffectType effectType);
        bool ShouldShowNotification(TalentId talentId);
    }
}
