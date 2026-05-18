using System;
using _Project.Scripts.Gameplay.Common.Time;
using _Project.Scripts.Gameplay.Currencies;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Levels
{
    public class LevelProgressService : ILevelProgressService
    {
        private float _elapsedSeconds;
        private LevelDefinition _level;
        private bool _isActive;
        private bool _completionWasRaised;

        [Inject] private ITimeService _time;
        [Inject] private ICurrencyService _currencyService;
        [Inject] private ILevelSelectionService _levelSelectionService;
        [Inject] private LevelCatalogConfig _levelCatalog;

        public event Action Completed;
        public CurrencyAmount LastCompletionReward { get; private set; }
        public float RemainingSeconds => Mathf.Max(0f, CurrentLevel.DurationSeconds - _elapsedSeconds);

        public void Start()
        {
            _level = _levelCatalog.LevelFor(_levelSelectionService.SelectedLevel);
            _elapsedSeconds = 0f;
            _isActive = true;
            _completionWasRaised = false;
            LastCompletionReward = _level.CompletionReward;
        }

        public void Update()
        {
            if (!_isActive || _completionWasRaised)
                return;

            _elapsedSeconds += _time.DeltaTime;
        }

        public void Complete()
        {
            if (!_isActive || _completionWasRaised)
                return;

            CompleteLevel();
        }

        public void Cleanup()
        {
            _level = null;
            _elapsedSeconds = 0f;
            _isActive = false;
            _completionWasRaised = false;
        }

        private LevelDefinition CurrentLevel => _level ?? _levelCatalog.LevelFor(_levelSelectionService.SelectedLevel);

        private void CompleteLevel()
        {
            _isActive = false;
            _completionWasRaised = true;
            LastCompletionReward = CurrentLevel.CompletionReward;
            _levelSelectionService.CompleteSelectedLevel();
            _currencyService.Add(LastCompletionReward);
            Completed?.Invoke();
        }
    }
}
