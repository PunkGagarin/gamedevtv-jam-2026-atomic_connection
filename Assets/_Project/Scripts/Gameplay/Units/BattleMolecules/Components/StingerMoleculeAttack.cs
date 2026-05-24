using _Project.Scripts.Gameplay.Talents;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    public class StingerMoleculeAttack : LineShotMoleculeAttack
    {
        protected override BattleMoleculeShotKind ShotKind => BattleMoleculeShotKind.Stinger;
        protected override int BaseShotDamage => Config.StingerMoleculeShotDamage;
        protected override TalentEffectType DamageTalentEffectType => TalentEffectType.StingerMoleculeDamage;
        protected override bool UsesCriticalHits => true;
        protected override TalentEffectType CriticalChanceTalentEffectType => TalentEffectType.StingerMoleculeCriticalChance;
        protected override TalentEffectType CriticalRewardTalentEffectType => TalentEffectType.StingerMoleculeCriticalReward;
        protected override float CriticalDamageMultiplier => Config.StingerMoleculeCriticalDamageMultiplier;

        protected override int TargetCount()
        {
            int pierce = Mathf.RoundToInt(TalentService.BonusOf(TalentEffectType.StingerMoleculePierce));
            return Mathf.Max(1, 1 + pierce);
        }
    }
}
