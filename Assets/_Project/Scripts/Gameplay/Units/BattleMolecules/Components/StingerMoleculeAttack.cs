using System.Collections.Generic;
using _Project.Scripts.Gameplay.Enemies;
using _Project.Scripts.Gameplay.Talents;
using _Project.Scripts.Gameplay.Units.BattleMolecules;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    public class StingerMoleculeAttack : BattleMoleculeAttack
    {
        private readonly List<EnemyHit> _enemyHits = new();

        protected override void OnDisable()
        {
            base.OnDisable();
            _enemyHits.Clear();
        }

        protected override void ResolveShot(BattleMoleculeShotRequest request)
        {
            if (request.Kind != BattleMoleculeShotKind.Stinger)
                return;

            FindEnemies(request.Origin, request.Direction, Mathf.Infinity, _enemyHits);

            int targetCount = TargetCount();
            int damagedTargets = 0;
            Vector3? lastDamagedHitPoint = null;

            for (int i = 0; i < _enemyHits.Count && damagedTargets < targetCount; i++)
            {
                EnemyUnit target = _enemyHits[i].Enemy;
                if (target == null)
                    continue;

                lastDamagedHitPoint = _enemyHits[i].Point;
                Damage(target, request.Kind, request.Origin);
                damagedTargets++;
            }

            AimLineView?.ShowShotLine(request, lastDamagedHitPoint);
        }

        private int TargetCount()
        {
            int pierce = Mathf.RoundToInt(TalentService.BonusOf(TalentType.StingerMoleculePierce));
            return Mathf.Max(1, 1 + pierce);
        }
    }
}
