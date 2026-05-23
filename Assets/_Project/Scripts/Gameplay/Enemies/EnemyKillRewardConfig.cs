using System;
using System.Collections.Generic;
using _Project.Scripts.Gameplay.Currencies;
using _Project.Scripts.Gameplay.Talents;
using UnityEngine;

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
        [field: SerializeField] public EnemyKillRewardBaseSource BaseSource { get; private set; }
        [field: SerializeField] public bool ApplyKillRewardMultiplierToBase { get; private set; }
        [field: SerializeField] public bool UseFlatBonusTalent { get; private set; }
        [field: SerializeField] public TalentType FlatBonusTalentType { get; private set; }
        [field: SerializeField] public bool UseExtraDropChanceTalent { get; private set; }
        [field: SerializeField] public TalentType ExtraDropChanceTalentType { get; private set; }
        [field: SerializeField, Min(0)] public int ExtraDropAmount { get; private set; } = 1;
    }
}
