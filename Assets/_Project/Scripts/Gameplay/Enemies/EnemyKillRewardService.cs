using System.Collections.Generic;
using _Project.Scripts.Gameplay.Common.Random;
using _Project.Scripts.Gameplay.Currencies;
using _Project.Scripts.Gameplay.CurrencyDrops;
using _Project.Scripts.Gameplay.Talents;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Enemies
{
    public class EnemyKillRewardService : IEnemyKillRewardService
    {
        private readonly List<EnemyUnit> _trackedEnemies = new();

        [Inject] private ICurrencyPickupService _currencyPickupService;
        [Inject] private IRandomService _randomService;
        [Inject] private ITalentService _talentService;
        [Inject] private EnemyKillRewardConfig _config;

        public void RegisterEnemy(EnemyUnit enemy)
        {
            if (enemy == null || _trackedEnemies.Contains(enemy))
                return;

            enemy.Killed += OnEnemyKilled;
            _trackedEnemies.Add(enemy);
        }

        public void UnregisterEnemy(EnemyUnit enemy)
        {
            if (enemy == null)
                return;

            enemy.Killed -= OnEnemyKilled;
            _trackedEnemies.Remove(enemy);
        }

        public void Cleanup()
        {
            foreach (EnemyUnit enemy in _trackedEnemies)
                enemy.Killed -= OnEnemyKilled;

            _trackedEnemies.Clear();
        }

        private void OnEnemyKilled(EnemyUnit enemy)
        {
            if (enemy == null || enemy.Id == EnemyId.Boss)
                return;

            RewardKill(enemy);
        }

        private void RewardKill(EnemyUnit enemy)
        {
            if (enemy == null || _config == null)
                return;

            foreach (EnemyKillRewardRule rule in _config.RewardRules)
            {
                if (rule == null || !rule.Matches(enemy.Id))
                    continue;

                int amount = RewardAmount(enemy, rule);
                if (amount <= 0)
                    continue;

                _currencyPickupService.Spawn(new CurrencyAmount(rule.CurrencyId, amount), enemy.transform.position);
            }
        }

        private int RewardAmount(EnemyUnit enemy, EnemyKillRewardRule rule)
        {
            int amount = BaseReward(enemy, rule);

            if (rule.UseFlatBonusTalent)
                amount += Mathf.Max(0, Mathf.RoundToInt(_talentService.BonusOf(rule.FlatBonusTalentType)));

            if (RollExtraDrop(rule))
                amount += Mathf.Max(0, rule.ExtraDropAmount);

            return Mathf.Max(0, amount);
        }

        private int BaseReward(EnemyUnit enemy, EnemyKillRewardRule rule)
        {
            int amount = rule.BaseSource switch
            {
                EnemyKillRewardBaseSource.EnemyDnaReward => enemy.DnaReward,
                EnemyKillRewardBaseSource.FixedAmount => rule.FixedAmount,
                _ => 0
            };

            if (rule.ApplyKillRewardMultiplierToBase)
                amount = Mathf.RoundToInt(amount * enemy.KillRewardMultiplier);

            return Mathf.Max(0, amount);
        }

        private bool RollExtraDrop(EnemyKillRewardRule rule)
        {
            if (!rule.UseExtraDropChanceTalent || rule.ExtraDropAmount <= 0)
                return false;

            float chance = Mathf.Clamp01(_talentService.BonusOf(rule.ExtraDropChanceTalentType));
            return chance >= 1f || chance > 0f && _randomService.Range(0f, 1f) < chance;
        }
    }
}
