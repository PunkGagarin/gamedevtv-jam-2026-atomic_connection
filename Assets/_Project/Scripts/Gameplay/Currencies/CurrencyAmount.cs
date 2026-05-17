using System;

namespace _Project.Scripts.Gameplay.Currencies
{
    [Serializable]
    public struct CurrencyAmount
    {
        public CurrencyId CurrencyId;
        public int Amount;

        public CurrencyAmount(CurrencyId currencyId, int amount)
        {
            CurrencyId = currencyId;
            Amount = amount;
        }
    }
}
