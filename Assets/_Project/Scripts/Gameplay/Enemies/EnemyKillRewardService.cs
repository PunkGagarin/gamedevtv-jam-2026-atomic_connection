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
        [Inject] private ICurrencyPickupService _currencyPickupService;
        [Inject] private IRandomService _randomService;
        [Inject] private ITalentService _talentService;
        [Inject] private EnemyKillRewardConfig _config;

        public void RewardKill(EnemyUnit enemy)
        {
            if (enemy == null || _config == null)
                return;

            foreach (EnemyKillRewardRule rule in _config.RewardRules)
            {
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
                EnemyKillRewardBaseSource.EnemyNucleotideReward => enemy.NucleotideReward,
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
