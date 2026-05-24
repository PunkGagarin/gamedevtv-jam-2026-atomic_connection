using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    public abstract class LineShotMoleculeAttack : BattleMoleculeAttack
    {
        protected abstract BattleMoleculeShotKind ShotKind { get; }

        protected override void ResolveShot(BattleMoleculeShotRequest request)
        {
            if (request.Kind != ShotKind)
                return;

            FindEnemies(request.Origin, request.Direction, Mathf.Infinity, EnemyHits);

            int targetCount = TargetCount();
            int damagedTargets = DamageEnemies(request.Origin, targetCount, null, out Vector3? lastDamagedHitPoint);

            AimLine?.ShowShotLine(request, ShotLineHitPoint(targetCount, damagedTargets, lastDamagedHitPoint));
        }

        protected virtual int TargetCount()
        {
            return 1;
        }

        private static Vector3? ShotLineHitPoint(int targetCount, int damagedTargets, Vector3? lastDamagedHitPoint)
        {
            bool hasUnusedPierce = targetCount > 1 && damagedTargets < targetCount;
            return hasUnusedPierce ? null : lastDamagedHitPoint;
        }
    }
}
