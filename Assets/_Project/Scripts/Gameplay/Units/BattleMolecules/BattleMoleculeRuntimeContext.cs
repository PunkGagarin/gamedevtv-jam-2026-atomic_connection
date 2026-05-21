using _Project.Scripts.Gameplay.Talents;
using _Project.Scripts.Gameplay.Units.AtomCores;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules
{
    public readonly struct BattleMoleculeRuntimeContext
    {
        private readonly ITalentService _talentService;

        public BattleMoleculeRuntimeContext(
            AtomCore core,
            BattleMoleculeConfig config,
            ITalentService talentService)
        {
            Core = core;
            Config = config;
            _talentService = talentService;
        }

        public AtomCore Core { get; }
        public BattleMoleculeConfig Config { get; }

        public float BonusOf(TalentType talentType)
        {
            return _talentService != null ? _talentService.BonusOf(talentType) : 0f;
        }

        public bool IsUnlocked(TalentType talentType)
        {
            return _talentService != null && _talentService.IsUnlocked(talentType);
        }
    }
}
