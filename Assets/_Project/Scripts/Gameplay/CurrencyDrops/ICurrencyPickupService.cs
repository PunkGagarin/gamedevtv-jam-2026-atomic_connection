using UnityEngine;
using _Project.Scripts.Gameplay.Currencies;

namespace _Project.Scripts.Gameplay.CurrencyDrops
{
    public interface ICurrencyPickupService
    {
        void Start();
        void Spawn(CurrencyAmount amount, Vector3 worldPosition);
        void Update();
        void Cleanup();
    }
}
