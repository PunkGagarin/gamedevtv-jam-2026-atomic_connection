using System;
using _Project.Scripts.Infrastructure.SaveLoad;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Levels
{
    public class LevelSelectionService : ILevelSelectionService
    {
        [Inject] private IProgressProvider _progressProvider;
        [Inject] private ISaveLoadService _saveLoadService;
        [Inject] private LevelCatalogConfig _levelCatalog;

        public event Action Changed;

        public int SelectedLevel
        {
            get
            {
                EnsureSelectedLevel();
                return _progressProvider.ProgressData.SelectedLevel;
            }
        }

        public int HighestUnlockedLevel => Mathf.Min(MaxConfiguredLevel, Mathf.Max(1, _progressProvider.ProgressData.CompletedLevelCount + 1));
        public int MaxConfiguredLevel => Mathf.Max(1, _levelCatalog.MaxLevelNumber);
        public bool CanSelectPrevious => SelectedLevel > 1;
        public bool CanSelectNext => SelectedLevel < HighestUnlockedLevel;

        public void SelectPrevious()
        {
            if (!CanSelectPrevious)
                return;

            SetSelectedLevel(SelectedLevel - 1);
        }

        public void SelectNext()
        {
            if (!CanSelectNext)
                return;

            SetSelectedLevel(SelectedLevel + 1);
        }

        public bool CompleteSelectedLevel()
        {
            ProgressData progressData = _progressProvider.ProgressData;

            if (SelectedLevel <= progressData.CompletedLevelCount)
                return false;

            progressData.CompletedLevelCount = SelectedLevel;
            SetSelectedLevel(HighestUnlockedLevel);
            return true;
        }

        public void ResetProgress()
        {
            ProgressData progressData = _progressProvider.ProgressData;
            progressData.CompletedLevelCount = 0;
            progressData.SelectedLevel = 1;
            _saveLoadService.SaveProgress();
            Changed?.Invoke();
        }

        private void EnsureSelectedLevel()
        {
            ProgressData progressData = _progressProvider.ProgressData;
            int clampedSelectedLevel = progressData.SelectedLevel <= 0
                ? HighestUnlockedLevel
                : Mathf.Clamp(progressData.SelectedLevel, 1, HighestUnlockedLevel);

            if (progressData.SelectedLevel == clampedSelectedLevel)
                return;

            progressData.SelectedLevel = clampedSelectedLevel;
            _saveLoadService.SaveProgress();
        }

        private void SetSelectedLevel(int level)
        {
            _progressProvider.ProgressData.SelectedLevel = Mathf.Clamp(level, 1, HighestUnlockedLevel);
            _saveLoadService.SaveProgress();
            Changed?.Invoke();
        }
    }
}
