using System.Collections.Generic;
using _Project.Scripts.Gameplay.Currencies;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Talents
{
    [CreateAssetMenu(fileName = "TalentDefinition", menuName = "Game Resources/Talents/Talent Definition")]
    public class TalentDefinition : ScriptableObject
    {
        [field: SerializeField] public TalentId Id { get; private set; }
        [field: SerializeField] public TalentType Type { get; private set; }
        [field: SerializeField] public string Title { get; private set; }
        [field: SerializeField, TextArea] public string Description { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField, Min(1)] public int MaxLevel { get; private set; } = 1;
        [field: SerializeField] public CurrencyId CostCurrency { get; private set; } = CurrencyId.Nucleotides;
        [field: SerializeField] public List<int> CostsByLevel { get; private set; } = new();
        [field: SerializeField] public List<TalentId> Prerequisites { get; private set; } = new();
        [field: SerializeField] public Vector2 GraphPosition { get; private set; }
        [field: SerializeField] public float BonusPerLevel { get; private set; }
        [field: SerializeField] public bool IsUnlock { get; private set; }

        public int CostForLevel(int currentLevel)
        {
            if (CostsByLevel == null || CostsByLevel.Count == 0)
                return 0;

            int index = Mathf.Clamp(currentLevel, 0, CostsByLevel.Count - 1);
            return CostsByLevel[index];
        }

        public CurrencyAmount PriceForLevel(int currentLevel)
        {
            return new CurrencyAmount(CostCurrency, CostForLevel(currentLevel));
        }
    }
}
