using DG.Tweening;
using _Project.Scripts.Gameplay.Units;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    [RequireComponent(typeof(BattleMoleculeCharge))]
    public class BattleMoleculeChargedPulseVisual : MonoBehaviour
    {
        [field: SerializeField] private BattleMoleculeCharge Charge { get; set; }
        [field: SerializeField] private Transform PulseTarget { get; set; }
        [field: SerializeField] private InitialLocalScale PulseInitialScale { get; set; }
        [field: SerializeField, Min(0f)] private float PulseScaleMultiplier { get; set; } = 1.18f;
        [field: SerializeField, Min(0f)] private float PulseDuration { get; set; } = 0.45f;

        private Tween _pulseTween;

        private void Awake()
        {
            if (Charge == null)
                Charge = GetComponent<BattleMoleculeCharge>();

            if (PulseTarget == null)
                PulseTarget = GetComponentInChildren<SpriteRenderer>()?.transform;

            if (PulseInitialScale == null && PulseTarget != null)
                PulseInitialScale = PulseTarget.GetComponent<InitialLocalScale>();
        }

        private void OnEnable()
        {
            Charge.Charged += StartPulse;
            Charge.Spent += StopPulse;
        }

        private void OnDisable()
        {
            Charge.Charged -= StartPulse;
            Charge.Spent -= StopPulse;
            StopPulse();
        }

        private void StartPulse()
        {
            if (PulseTarget == null)
                return;

            StopPulse();

            PulseInitialScale?.ResetScale();
            _pulseTween = PulseTarget
                .DOScale(BaseScale() * Mathf.Max(0f, PulseScaleMultiplier), Mathf.Max(0f, PulseDuration))
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void StopPulse()
        {
            _pulseTween?.Kill();
            _pulseTween = null;

            if (PulseTarget != null)
                PulseInitialScale?.ResetScale();
        }

        private Vector3 BaseScale()
        {
            return PulseInitialScale != null ? PulseInitialScale.Scale : PulseTarget.localScale;
        }
    }
}
