using _Project.Scripts.Gameplay.Currencies;
using UnityEngine;

namespace _Project.Scripts.Gameplay.CurrencyDrops
{
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(CurrencyPickupAmount))]
    [RequireComponent(typeof(CurrencyPickupHitArea))]
    [RequireComponent(typeof(CurrencyPickupFeedback))]
    public class CurrencyPickup : MonoBehaviour
    {
        [field: SerializeField] private CurrencyPickupAmount AmountState { get; set; }
        [field: SerializeField] private CurrencyPickupHitArea HitArea { get; set; }
        [field: SerializeField] private CurrencyPickupFeedback Feedback { get; set; }

        public CurrencyAmount Amount => AmountState.Amount;

        private void Awake()
        {
            if (AmountState == null)
                AmountState = GetComponent<CurrencyPickupAmount>();

            if (HitArea == null)
                HitArea = GetComponent<CurrencyPickupHitArea>();

            if (Feedback == null)
                Feedback = GetComponent<CurrencyPickupFeedback>();
        }

        public void Initialize(CurrencyAmount amount, CurrencyPickupConfig config)
        {
            AmountState.Configure(amount);
            Feedback.Initialize(amount, config);
        }

        public bool IsInsidePickupArea(Vector3 areaCenter, float areaHalfSize)
        {
            return HitArea.IsInside(areaCenter, areaHalfSize);
        }

        public void PlayCollected(CurrencyAmount collectedAmount, CurrencyPickupConfig config)
        {
            HitArea.Disable();
            Feedback.PlayCollected(collectedAmount, config);
        }
    }
}
