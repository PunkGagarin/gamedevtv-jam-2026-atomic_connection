using System;
using _Project.Scripts.Infrastructure.SaveLoad;
using _Project.Scripts.Localization;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Currencies
{
    public class CurrencyService : ICurrencyService
    {
        [Inject] private IProgressProvider _progressProvider;
        [Inject] private ISaveLoadService _saveLoadService;
        [Inject] private LocalizationTool _localizationTool;

        public event Action Changed;

        public int BalanceOf(CurrencyId currencyId)
        {
            return _progressProvider.ProgressData.GetCurrencyAmount(currencyId);
        }

        public bool CanSpend(CurrencyAmount price)
        {
            return price.Amount <= 0 || BalanceOf(price.CurrencyId) >= price.Amount;
        }

        public bool Spend(CurrencyAmount price)
        {
            if (!CanSpend(price))
                return false;

            if (price.Amount <= 0)
                return true;

            SetBalance(price.CurrencyId, BalanceOf(price.CurrencyId) - price.Amount);
            return true;
        }

        public void Add(CurrencyAmount reward)
        {
            if (reward.Amount <= 0)
                return;

            SetBalance(reward.CurrencyId, BalanceOf(reward.CurrencyId) + reward.Amount);
        }

        public void SetBalance(CurrencyId currencyId, int amount)
        {
            _progressProvider.ProgressData.SetCurrencyAmount(currencyId, Mathf.Max(0, amount));
            _saveLoadService.SaveProgress();
            Changed?.Invoke();
        }

        public string Format(CurrencyAmount amount)
        {
            return $"{amount.Amount} {NameOf(amount.CurrencyId)}";
        }

        private string NameOf(CurrencyId currencyId)
        {
            string key = currencyId switch
            {
                CurrencyId.Nucleotides => "CURRENCY_NUCLEOTIDES_SHORT",
                CurrencyId.Isotopes => "CURRENCY_ISOTOPES_SHORT",
                CurrencyId.Radicals => "CURRENCY_RADICALS_SHORT",
                _ => null
            };

            return key == null ? currencyId.ToString() : _localizationTool.GetText(key);
        }
    }
}
