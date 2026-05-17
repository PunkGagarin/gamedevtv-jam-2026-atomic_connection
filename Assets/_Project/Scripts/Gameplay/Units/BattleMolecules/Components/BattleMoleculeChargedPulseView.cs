using DG.Tweening;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    [RequireComponent(typeof(BattleMoleculeCharge))]
    public class BattleMoleculeChargedPulseView : MonoBehaviour
    {
        private const float PULSE_SCALE = 1.18f;
        private const float PULSE_DURATION = 0.45f;

        [field: SerializeField] private BattleMoleculeCharge Charge { get; set; }
        [field: SerializeField] private Transform PulseTarget { get; set; }

        private Vector3 _baseScale;
        private Tween _pulseTween;

        private void Awake()
        {
            if (Charge == null)
                Charge = GetComponent<BattleMoleculeCharge>();

            if (PulseTarget == null)
                PulseTarget = GetComponentInChildren<SpriteRenderer>()?.transform;

            if (PulseTarget != null)
                _baseScale = PulseTarget.localScale;
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

            PulseTarget.localScale = _baseScale;
            _pulseTween = PulseTarget
                .DOScale(_baseScale * PULSE_SCALE, PULSE_DURATION)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void StopPulse()
        {
            _pulseTween?.Kill();
            _pulseTween = null;

            if (PulseTarget != null)
                PulseTarget.localScale = _baseScale;
        }
    }
}
