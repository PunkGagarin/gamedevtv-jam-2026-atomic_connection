using _Project.Scripts.Gameplay.Talents;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    public class StingerMoleculeAttack : BattleMoleculeAttack
    {
        protected override int BaseShotDamage => Config.StingerMoleculeShotDamage;
        protected override TalentType DamageTalentType => TalentType.StingerMoleculeDamage;

        protected override void ResolveShot(BattleMoleculeShotRequest request)
        {
            if (request.Kind != BattleMoleculeShotKind.Stinger)
                return;

            FindEnemies(request.Origin, request.Direction, Mathf.Infinity, EnemyHits);

            int targetCount = TargetCount();
            int damagedTargets = DamageEnemies(request.Origin, targetCount, null, out Vector3? lastDamagedHitPoint);

            AimLine?.ShowShotLine(request, ShotLineHitPoint(targetCount, damagedTargets, lastDamagedHitPoint));
        }

        private int TargetCount()
        {
            int pierce = Mathf.RoundToInt(TalentService.BonusOf(TalentType.StingerMoleculePierce));
            return Mathf.Max(1, 1 + pierce);
        }

        private static Vector3? ShotLineHitPoint(int targetCount, int damagedTargets, Vector3? lastDamagedHitPoint)
        {
            bool hasUnusedPierce = targetCount > 1 && damagedTargets < targetCount;
            return hasUnusedPierce ? null : lastDamagedHitPoint;
        }
    }
}
