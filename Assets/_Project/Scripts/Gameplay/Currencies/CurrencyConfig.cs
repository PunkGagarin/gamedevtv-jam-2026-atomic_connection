using UnityEngine;

namespace _Project.Scripts.Gameplay.Currencies
{
    [CreateAssetMenu(fileName = "CurrencyConfig", menuName = "Game Resources/Configs/Currencies")]
    public class CurrencyConfig : ScriptableObject
    {
        [field: SerializeField, Min(0)] public int StartingNucleotides { get; private set; } = 500;
        [field: SerializeField, Min(0)] public int StartingIsotopes { get; private set; } = 3;
        [field: SerializeField, Range(0, 100)] public int RegularEnemyIsotopeDropChancePercent { get; private set; } = 10;

        public CurrencyAmount StartingAmount(CurrencyId currencyId)
        {
            return currencyId switch
            {
                CurrencyId.Nucleotides => new CurrencyAmount(currencyId, StartingNucleotides),
                CurrencyId.Isotopes => new CurrencyAmount(currencyId, StartingIsotopes),
                _ => new CurrencyAmount(currencyId, 0)
            };
        }
    }
}
