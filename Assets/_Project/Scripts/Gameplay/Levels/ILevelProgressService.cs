using System;
using _Project.Scripts.Gameplay.Currencies;

namespace _Project.Scripts.Gameplay.Levels
{
    public interface ILevelProgressService
    {
        event Action Completed;
        CurrencyAmount LastCompletionReward { get; }
        float RemainingSeconds { get; }
        void Start();
        void Update();
        void Complete();
        void Cleanup();
    }
}
