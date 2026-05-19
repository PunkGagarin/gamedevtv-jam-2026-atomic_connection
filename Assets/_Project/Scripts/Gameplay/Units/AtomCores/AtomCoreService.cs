using System;
using _Project.Scripts.Gameplay.Common.Time;
using _Project.Scripts.Gameplay.Talents;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Units.AtomCores
{
    public class AtomCoreService : IAtomCoreService, IAtomCoreCreator
    {
        private AtomCore _core;
        private bool _isStarted;

        [Inject] private ITimeService _time;
        [Inject] private AtomCoreConfig _config;
        [Inject] private ITalentService _talentService;
        [Inject] private IAtomCoreFactory _atomCoreFactory;

        public Transform CurrentCoreTransform => _core != null ? _core.transform : null;
        public event Action CoreDied;

        public void Create(Vector3 at)
        {
            if (_core != null)
                Cleanup();

            _core = _atomCoreFactory.Create(at);

            if (_core != null)
                ApplyTalentBonuses();
        }

        public void Start()
        {
            if (_core == null || _isStarted)
                return;

            _core.Died += OnCoreDied;
            _talentService.Changed += OnTalentChanged;
            _isStarted = true;
        }

        public void Update()
        {
            if (_core == null)
                return;

            _core.Tick(_time.DeltaTime);
        }

        public void Cleanup()
        {
            if (_core != null && _isStarted)
            {
                _core.Died -= OnCoreDied;
                _talentService.Changed -= OnTalentChanged;
            }

            _core?.CleanupAtoms();
            _isStarted = false;
            _atomCoreFactory.Cleanup();
            _core = null;
        }

        private void OnTalentChanged()
        {
            if (_core == null)
                return;

            ApplyTalentBonuses();
        }

        private void ApplyTalentBonuses()
        {
            int adjustedClicks = Mathf.Max(1, Mathf.RoundToInt(
                _config.ClicksToGenerateFreeAtom / _talentService.AtomGenerationMultiplier));

            int adjustedHealth = Mathf.Max(1, Mathf.RoundToInt(
                _config.CoreMaxHealth + _talentService.BonusOf(TalentType.CoreHealth)));

            _core.Configure(_config, adjustedClicks, adjustedHealth);
        }
        
        private void OnCoreDied()
        {
            CoreDied?.Invoke();
        }
    }
}
