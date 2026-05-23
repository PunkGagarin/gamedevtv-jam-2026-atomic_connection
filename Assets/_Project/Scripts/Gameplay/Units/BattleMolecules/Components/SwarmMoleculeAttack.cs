using System.Collections.Generic;
using _Project.Scripts.Gameplay.Enemies;
using _Project.Scripts.Gameplay.Talents;
using _Project.Scripts.Gameplay.Units.BattleMolecules;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    public class SwarmMoleculeAttack : BattleMoleculeAttack
    {
        private readonly HashSet<EnemyUnit> _shotSequenceHits = new();
        private int _currentShotSequenceId = -1;

        protected override int BaseShotDamage => Config.SwarmMoleculeShotDamage;
        protected override TalentType DamageTalentType => TalentType.SwarmMoleculeDamage;

        protected override void OnDisable()
        {
            base.OnDisable();
            _shotSequenceHits.Clear();
            _currentShotSequenceId = -1;
        }

        protected override void ResolveShot(BattleMoleculeShotRequest request)
        {
            if (request.Kind != BattleMoleculeShotKind.Swarm)
                return;

            float attackRange = CurrentAttackRange();
            PrepareShotSequence(request);
            FindEnemies(request.Origin, request.Direction, attackRange, EnemyHits);
            AimLine?.ShowShotLine(request, null, attackRange);

            DamageEnemies(request.Origin, int.MaxValue, _shotSequenceHits, out _);
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
            float rangeBonus = TalentService != null ? TalentService.BonusOf(TalentType.SwarmMoleculeAttackRange) : 0f;
            return Mathf.Max(0f, Config.SwarmMoleculeAttackRange + rangeBonus);
        }
    }
}
