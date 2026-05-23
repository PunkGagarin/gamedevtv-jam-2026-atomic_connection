using UnityEngine;
using _Project.Scripts.Gameplay.Currencies;

namespace _Project.Scripts.Gameplay.CurrencyDrops
{
    public class CurrencyPickupAmount : MonoBehaviour
    {
        public CurrencyAmount Amount { get; private set; }

        public void Configure(CurrencyAmount amount)
        {
            Amount = amount;
        }
    }
}
