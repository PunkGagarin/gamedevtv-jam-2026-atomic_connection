using System;

namespace _Project.Scripts.Gameplay.Currencies
{
    public interface ICurrencyService
    {
        event Action Changed;

        int BalanceOf(CurrencyId currencyId);
        bool CanSpend(CurrencyAmount price);
        bool Spend(CurrencyAmount price);
        void Add(CurrencyAmount reward);
        void SetBalance(CurrencyId currencyId, int amount);
    }
}
