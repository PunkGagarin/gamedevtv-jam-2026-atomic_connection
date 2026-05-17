using System;
using System.Collections.Generic;

namespace _Project.Scripts.Gameplay.Talents
{
    public interface ITalentService
    {
        event Action Changed;

        int Gold { get; }
        IReadOnlyList<TalentDefinition> Talents { get; }
        float AtomGenerationMultiplier { get; }

        int LevelOf(TalentId talentId);
        bool CanBuy(TalentId talentId);
        bool Buy(TalentId talentId);
        float BonusOf(TalentType type);
        bool IsUnlocked(TalentType type);
    }
}
