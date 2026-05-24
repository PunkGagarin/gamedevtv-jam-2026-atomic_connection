using _Project.Scripts.Gameplay.Talents;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    public class StingerMoleculeAttack : LineShotMoleculeAttack
    {
        protected override BattleMoleculeShotKind ShotKind => BattleMoleculeShotKind.Stinger;
        protected override int BaseShotDamage => Config.StingerMoleculeShotDamage;
        protected override TalentType DamageTalentType => TalentType.StingerMoleculeDamage;
        protected override bool UsesCriticalHits => true;
        protected override TalentType CriticalChanceTalentType => TalentType.StingerMoleculeCriticalChance;
        protected override TalentType CriticalRewardTalentType => TalentType.StingerMoleculeCriticalReward;
        protected override float CriticalDamageMultiplier => Config.StingerMoleculeCriticalDamageMultiplier;

        protected override int TargetCount()
        {
            int pierce = Mathf.RoundToInt(TalentService.BonusOf(TalentType.StingerMoleculePierce));
            return Mathf.Max(1, 1 + pierce);
        }
    }
}
