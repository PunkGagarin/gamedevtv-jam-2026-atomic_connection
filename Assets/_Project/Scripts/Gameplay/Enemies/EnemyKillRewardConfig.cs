using System;
using System.Collections.Generic;
using _Project.Scripts.Gameplay.Currencies;
using _Project.Scripts.Gameplay.Talents;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.Gameplay.Enemies
{
    [CreateAssetMenu(fileName = "EnemyKillRewardConfig", menuName = "Game Resources/Configs/Enemy Kill Rewards")]
    public class EnemyKillRewardConfig : ScriptableObject
    {
        [field: SerializeField] public List<EnemyKillRewardRule> RewardRules { get; private set; } = new();
    }

    [Serializable]
    public class EnemyKillRewardRule
    {
        [field: SerializeField] public CurrencyId CurrencyId { get; private set; }
        [field: SerializeField] public List<EnemyId> AllowedEnemyIds { get; private set; } = new();
        [field: SerializeField] public EnemyKillRewardBaseSource BaseSource { get; private set; }
        [field: SerializeField, Min(0)] public int FixedAmount { get; private set; }
        [field: SerializeField] public bool ApplyKillRewardMultiplierToBase { get; private set; }
        [field: SerializeField] public bool UseFlatBonusTalent { get; private set; }
        [field: SerializeField, FormerlySerializedAs("<FlatBonusTalentType>k__BackingField")] public TalentEffectType FlatBonusTalentEffectType { get; private set; }
        [field: SerializeField] public bool UseExtraDropChanceTalent { get; private set; }
        [field: SerializeField, FormerlySerializedAs("<ExtraDropChanceTalentType>k__BackingField")] public TalentEffectType ExtraDropChanceTalentEffectType { get; private set; }
        [field: SerializeField, Min(0)] public int ExtraDropAmount { get; private set; } = 1;

        public bool Matches(EnemyId enemyId)
        {
            return AllowedEnemyIds == null || AllowedEnemyIds.Count == 0 || AllowedEnemyIds.Contains(enemyId);
        }
    }
}
