using UnityEngine;

namespace _Project.Scripts.Gameplay.Currencies
{
    [CreateAssetMenu(fileName = "CurrencyConfig", menuName = "Game Resources/Configs/Currencies")]
    public class CurrencyConfig : ScriptableObject
    {
        [field: SerializeField, Min(0)] public int StartingDna { get; private set; } = 500;
        [field: SerializeField, Min(0)] public int StartingIsotopes { get; private set; } = 3;
        [field: SerializeField, Min(0)] public int StartingRadicals { get; private set; }

        public CurrencyAmount StartingAmount(CurrencyId currencyId)
        {
            return currencyId switch
            {
                CurrencyId.Dna => new CurrencyAmount(currencyId, StartingDna),
                CurrencyId.Isotopes => new CurrencyAmount(currencyId, StartingIsotopes),
                CurrencyId.Radicals => new CurrencyAmount(currencyId, StartingRadicals),
                _ => new CurrencyAmount(currencyId, 0)
            };
        }
    }
}
