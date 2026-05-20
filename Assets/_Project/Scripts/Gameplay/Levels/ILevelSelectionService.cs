using System;

namespace _Project.Scripts.Gameplay.Levels
{
    public interface ILevelSelectionService
    {
        event Action Changed;
        int SelectedLevel { get; }
        int HighestUnlockedLevel { get; }
        int MaxConfiguredLevel { get; }
        bool CanSelectPrevious { get; }
        bool CanSelectNext { get; }
        void SelectPrevious();
        void SelectNext();
        bool CompleteSelectedLevel();
        void ResetProgress();
    }
}
