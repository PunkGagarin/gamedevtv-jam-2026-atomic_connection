using UnityEngine;
using Zenject;
using _Project.Scripts.Gameplay.Enemies;
using _Project.Scripts.Gameplay.Talents;
using _Project.Scripts.Gameplay.Units.AtomCores;
using _Project.Scripts.Gameplay.UI;
using _Project.Scripts.Gameplay.Units.AtomCores.Components;
using _Project.Scripts.Gameplay.Units.BattleMolecules;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    [RequireComponent(typeof(BattleMoleculeCharge))]
    [RequireComponent(typeof(BattleMoleculeChargeConsumption))]
    public class MembraneMoleculeActivation : MonoBehaviour
    {
        [field: SerializeField] private BattleMoleculeCharge Charge { get; set; }
        [field: SerializeField] private BattleMoleculeChargeConsumption ChargeConsumption { get; set; }
        [field: SerializeField] private ProgressBar CooldownProgressBar { get; set; }
        [field: SerializeField] private ProgressBar ShieldDurationProgressBar { get; set; }

        private AtomCoreShield _coreMembrane;
        private float _duration;
        private int _integrity;
        private float _cooldownDuration;
        private float _cooldownRemaining;
        private float _knockbackRadius;
        private float _knockbackDistance;
        private float _knockbackDuration;

        [Inject] private IEnemyService _enemyService;

        private void Awake()
        {
            if (Charge == null)
                Charge = GetComponent<BattleMoleculeCharge>();

            if (ChargeConsumption == null)
                ChargeConsumption = GetComponent<BattleMoleculeChargeConsumption>();

            HideProgressBars();
        }

        private void OnEnable()
        {
            if (Charge != null)
                Charge.Charged += TryActivateMembrane;

            SubscribeToCoreMembrane();
        }

        private void OnDisable()
        {
            if (Charge != null)
                Charge.Charged -= TryActivateMembrane;

            UnsubscribeFromCoreMembrane();
        }

        public void Configure(AtomCore core, BattleMoleculeConfig config, ITalentService talentService)
        {
            SetCoreMembrane(core != null ? core.GetComponent<AtomCoreShield>() : null);

            float durationBonus = talentService != null ? talentService.BonusOf(TalentType.MembraneMoleculeDuration) : 0f;
            int integrityBonus = talentService != null ? Mathf.RoundToInt(talentService.BonusOf(TalentType.MembraneMoleculeIntegrity)) : 0;
            float cooldownReduction = talentService != null ? Mathf.Clamp01(talentService.BonusOf(TalentType.MembraneMoleculeCooldownReduction)) : 0f;

            _duration = config != null
                ? Mathf.Max(0.01f, config.MembraneDurationSeconds + durationBonus)
                : 0.01f;

            _integrity = config != null
                ? Mathf.Max(1, config.MembraneIntegrity + integrityBonus)
                : Mathf.Max(1, integrityBonus);

            _cooldownDuration = config != null
                ? Mathf.Max(0f, config.MembraneCooldownSeconds * (1f - cooldownReduction))
                : 0f;

            _knockbackRadius = config != null
                ? Mathf.Max(0f, config.MembraneKnockbackRadius)
                : 0f;

            _knockbackDistance = config != null
                ? Mathf.Max(0f, config.MembraneKnockbackDistance)
                : 0f;

            _knockbackDuration = config != null
                ? Mathf.Max(0.01f, config.MembraneKnockbackDurationSeconds)
                : 0.01f;

        }

        public void Tick(float deltaTime)
        {
            TickCooldown(deltaTime);

            if (_coreMembrane != null && _coreMembrane.IsActive)
            {
                HideProgress(CooldownProgressBar);
                ShowProgress(ShieldDurationProgressBar, _coreMembrane.NormalizedTime);
                return;
            }

            HideProgress(ShieldDurationProgressBar);

            if (_cooldownRemaining > 0f)
            {
                ShowProgress(CooldownProgressBar, 1f - _cooldownRemaining / Mathf.Max(0.01f, _cooldownDuration));
                return;
            }

            HideProgress(CooldownProgressBar);
            TryActivateMembrane();
        }

        private void TryActivateMembrane()
        {
            if (_coreMembrane == null)
                return;

            if (_coreMembrane.IsActive || _cooldownRemaining > 0f)
                return;

            if (ChargeConsumption == null || !ChargeConsumption.TryConsume())
                return;

            _coreMembrane.Activate(_duration, _integrity);
            HideProgress(CooldownProgressBar);
            ShowProgress(ShieldDurationProgressBar, 1f);
        }

        private void TickCooldown(float deltaTime)
        {
            if (_cooldownRemaining <= 0f)
                return;

            _cooldownRemaining = Mathf.Max(0f, _cooldownRemaining - Mathf.Max(0f, deltaTime));
        }

        private void StartCooldown()
        {
            _cooldownRemaining = _cooldownDuration;
        }

        private void OnMembraneBroken()
        {
            StartCooldown();

            if (_coreMembrane == null)
                return;

            _enemyService?.PushEnemiesFrom(_coreMembrane.transform.position, _knockbackRadius, _knockbackDistance, _knockbackDuration);
        }

        private void OnMembraneExpired()
        {
            StartCooldown();
        }

        private void SetCoreMembrane(AtomCoreShield coreMembrane)
        {
            if (_coreMembrane == coreMembrane)
                return;

            UnsubscribeFromCoreMembrane();
            _coreMembrane = coreMembrane;
            SubscribeToCoreMembrane();
        }

        private void SubscribeToCoreMembrane()
        {
            if (_coreMembrane == null || !isActiveAndEnabled)
                return;

            _coreMembrane.Broken += OnMembraneBroken;
            _coreMembrane.Expired += OnMembraneExpired;
        }

        private void UnsubscribeFromCoreMembrane()
        {
            if (_coreMembrane == null)
                return;

            _coreMembrane.Broken -= OnMembraneBroken;
            _coreMembrane.Expired -= OnMembraneExpired;
        }

        private void ShowProgress(ProgressBar progressBar, float normalized)
        {
            if (progressBar == null)
                return;

            float progress = Mathf.Clamp01(normalized);
            progressBar.gameObject.SetActive(progress > 0f);
            progressBar.SetProgress(progress);
        }

        private void HideProgress(ProgressBar progressBar)
        {
            if (progressBar != null)
                progressBar.gameObject.SetActive(false);
        }

        private void HideProgressBars()
        {
            HideProgress(CooldownProgressBar);
            HideProgress(ShieldDurationProgressBar);
        }
    }
}
