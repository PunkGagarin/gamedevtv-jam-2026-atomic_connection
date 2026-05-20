using System.Collections.Generic;
using _Project.Scripts.Gameplay.Enemies;
using _Project.Scripts.Gameplay.Units.BattleMolecules;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    public class SwarmMoleculeAttack : BattleMoleculeAttack
    {
        private readonly List<EnemyHit> _enemyHits = new();
        private readonly HashSet<EnemyUnit> _shotSequenceHits = new();
        private int _currentShotSequenceId = -1;

        protected override void OnDisable()
        {
            base.OnDisable();
            _enemyHits.Clear();
            _shotSequenceHits.Clear();
            _currentShotSequenceId = -1;
        }

        protected override void ResolveShot(BattleMoleculeShotRequest request)
        {
            if (request.Kind != BattleMoleculeShotKind.Swarm)
                return;

            float attackRange = CurrentAttackRange();
            PrepareShotSequence(request);
            FindEnemies(request.Origin, request.Direction, attackRange, _enemyHits);
            AimLineView?.ShowShotLine(request, null, attackRange);

            foreach (EnemyHit hit in _enemyHits)
            {
                EnemyUnit target = hit.Enemy;
                if (target == null || _shotSequenceHits.Contains(target))
                    continue;

                Damage(target, request.Kind, request.Origin);
                _shotSequenceHits.Add(target);
            }
        }

        private void PrepareShotSequence(BattleMoleculeShotRequest request)
        {
            if (_currentShotSequenceId == request.ShotSequenceId)
                return;

            _currentShotSequenceId = request.ShotSequenceId;
            _shotSequenceHits.Clear();
        }

        private float CurrentAttackRange()
        {
            return Mathf.Max(0f, Config.SwarmMoleculeAttackRange);
        }
    }
}
