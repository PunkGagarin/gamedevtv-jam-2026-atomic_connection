using UnityEngine;
using _Project.Scripts.Gameplay.Talents;
using _Project.Scripts.Gameplay.Units;
using _Project.Scripts.Gameplay.Units.BattleMolecules;
using _Project.Scripts.Gameplay.UI;
using _Project.Scripts.Gameplay.Units.AtomCores.Components;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    [RequireComponent(typeof(OwnedAtoms))]
    [RequireComponent(typeof(BattleMoleculeCharge))]
    public class MembraneMoleculeActivation : MonoBehaviour, IBattleMoleculeRuntimeBehavior, IBattleMoleculeAutoLoadRule
    {
        [field: SerializeField] private OwnedAtoms OwnedAtoms { get; set; }
        [field: SerializeField] private BattleMoleculeCharge Charge { get; set; }
        [field: SerializeField] private ProgressBar ProgressBar { get; set; }

        private AtomCoreShield _coreMembrane;
        private float _duration;
        private float _secondsLostPerDamage;

        private void Awake()
        {
            if (OwnedAtoms == null)
                OwnedAtoms = GetComponent<OwnedAtoms>();

            if (Charge == null)
                Charge = GetComponent<BattleMoleculeCharge>();

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

        public void Configure(BattleMoleculeRuntimeContext context)
        {
            _coreMembrane = context.Core != null ? context.Core.GetComponent<AtomCoreShield>() : null;

            float durationBonus = context.BonusOf(TalentType.MembraneMoleculeDuration);

            _duration = context.Config != null
                ? Mathf.Max(0.01f, context.Config.MembraneDurationSeconds + durationBonus)
                : 0.01f;

            _secondsLostPerDamage = context.Config != null
                ? Mathf.Max(0f, context.Config.MembraneSecondsLostPerDamage)
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

        public bool CanAutoLoad(BattleMoleculeRuntimeContext context)
        {
            return context.IsUnlocked(TalentType.MembraneMoleculeAutoLoad);
        }

        private void ActivateMembrane()
        {
            if (_coreMembrane == null)
                return;

            _coreMembrane.Activate(_duration, _secondsLostPerDamage);
            Charge.Spend();
            OwnedAtoms.ReleaseAll();
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
