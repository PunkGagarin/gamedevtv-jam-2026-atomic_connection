using UnityEngine;
using _Project.Scripts.Gameplay.Talents;
using _Project.Scripts.Gameplay.Units.AtomCores;
using _Project.Scripts.Gameplay.Units.BattleMolecules;
using _Project.Scripts.Gameplay.UI;
using _Project.Scripts.Gameplay.Units.AtomCores.Components;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    [RequireComponent(typeof(BattleMoleculeCharge))]
    [RequireComponent(typeof(BattleMoleculeChargeConsumption))]
    public class MembraneMoleculeActivation : MonoBehaviour
    {
        [field: SerializeField] private BattleMoleculeCharge Charge { get; set; }
        [field: SerializeField] private BattleMoleculeChargeConsumption ChargeConsumption { get; set; }
        [field: SerializeField] private ProgressBar ProgressBar { get; set; }

        private AtomCoreShield _coreMembrane;
        private float _duration;
        private float _secondsLostPerDamage;

        private void Awake()
        {
            if (Charge == null)
                Charge = GetComponent<BattleMoleculeCharge>();

            if (ChargeConsumption == null)
                ChargeConsumption = GetComponent<BattleMoleculeChargeConsumption>();

            HideProgress();
        }

        private void OnEnable()
        {
            if (Charge != null)
                Charge.Charged += ActivateMembrane;
        }

        private void OnDisable()
        {
            if (Charge != null)
                Charge.Charged -= ActivateMembrane;
        }

        public void Configure(AtomCore core, BattleMoleculeConfig config, ITalentService talentService)
        {
            _coreMembrane = core != null ? core.GetComponent<AtomCoreShield>() : null;

            float durationBonus = talentService != null ? talentService.BonusOf(TalentType.MembraneMoleculeDuration) : 0f;

            _duration = config != null
                ? Mathf.Max(0.01f, config.MembraneDurationSeconds + durationBonus)
                : 0.01f;

            _secondsLostPerDamage = config != null
                ? Mathf.Max(0f, config.MembraneSecondsLostPerDamage)
                : 0f;
        }

        public void Tick(float deltaTime)
        {
            if (_coreMembrane == null || !_coreMembrane.IsActive)
            {
                HideProgress();
                return;
            }

            ShowProgress(_coreMembrane.NormalizedTime);
        }

        private void ActivateMembrane()
        {
            if (_coreMembrane == null)
                return;

            if (ChargeConsumption == null || !ChargeConsumption.TryConsume())
                return;

            _coreMembrane.Activate(_duration, _secondsLostPerDamage);
            ShowProgress(1f);
        }

        private void ShowProgress(float normalized)
        {
            if (ProgressBar == null)
                return;

            float progress = Mathf.Clamp01(normalized);
            ProgressBar.gameObject.SetActive(progress > 0f);
            ProgressBar.SetProgress(progress);
        }

        private void HideProgress()
        {
            if (ProgressBar != null)
                ProgressBar.gameObject.SetActive(false);
        }
    }
}
