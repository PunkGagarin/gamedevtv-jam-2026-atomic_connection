using UnityEngine;
using _Project.Scripts.Gameplay.Units;
using _Project.Scripts.Gameplay.UI;
using _Project.Scripts.Gameplay.Units.AtomCores;
using _Project.Scripts.Gameplay.Units.AtomCores.Components;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    [RequireComponent(typeof(OwnedAtoms))]
    [RequireComponent(typeof(BattleMoleculeCharge))]
    public class ShieldMoleculeActivation : MonoBehaviour
    {
        [field: SerializeField] private OwnedAtoms OwnedAtoms { get; set; }
        [field: SerializeField] private BattleMoleculeCharge Charge { get; set; }
        [field: SerializeField] private ProgressBar ProgressBar { get; set; }

        private AtomCoreShield _coreShield;
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
                Charge.Charged += ActivateShield;
        }

        private void OnDisable()
        {
            if (Charge != null)
                Charge.Charged -= ActivateShield;
        }

        public void Configure(AtomCore core, float duration, float secondsLostPerDamage)
        {
            _coreShield = core != null ? core.GetComponent<AtomCoreShield>() : null;
            _duration = Mathf.Max(0.01f, duration);
            _secondsLostPerDamage = Mathf.Max(0f, secondsLostPerDamage);
        }

        public void Tick()
        {
            if (_coreShield == null || !_coreShield.IsActive)
            {
                HideProgress();
                return;
            }

            ShowProgress(_coreShield.NormalizedTime);
        }

        private void ActivateShield()
        {
            if (_coreShield == null)
                return;

            _coreShield.Activate(_duration, _secondsLostPerDamage);
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
