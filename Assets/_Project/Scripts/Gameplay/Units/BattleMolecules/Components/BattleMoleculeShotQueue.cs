using System;
using _Project.Scripts.Gameplay.Units.BattleMolecules;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    [RequireComponent(typeof(BattleMoleculeChargeConsumption))]
    public class BattleMoleculeShotQueue : MonoBehaviour
    {
        [field: SerializeField] private BattleMoleculeChargeConsumption ChargeConsumption { get; set; }

        public event Action<BattleMoleculeShotRequest> ShotRequested;

        private void Awake()
        {
            if (ChargeConsumption == null)
                ChargeConsumption = GetComponent<BattleMoleculeChargeConsumption>();
        }

        public virtual void Configure(BattleMoleculeConfig config)
        {
        }

        public virtual bool TryRequestShot(Vector3 direction)
        {
            return TryRequestShot(direction, RequestStingerShot);
        }

        protected bool TryRequestShot(Vector3 direction, Action<Vector3, Vector3> requestShots)
        {
            if (!CanRequestShot(direction))
                return false;

            Vector3 origin = transform.position;
            Vector3 shotDirection = NormalizeShotDirection(direction);

            SpendCharge();
            requestShots?.Invoke(origin, shotDirection);

            return true;
        }

        protected bool CanRequestShot(Vector3 direction)
        {
            return ChargeConsumption != null
                   && ChargeConsumption.CanConsume
                   && direction.sqrMagnitude > Mathf.Epsilon;
        }

        protected void SpendCharge()
        {
            ChargeConsumption?.TryConsume();
        }

        protected void RequestShot(Vector3 origin, Vector3 direction, BattleMoleculeShotKind kind, int shotSequenceId = 0)
        {
            ShotRequested?.Invoke(new BattleMoleculeShotRequest(origin, direction, kind, shotSequenceId));
        }

        protected static Vector3 NormalizeShotDirection(Vector3 direction)
        {
            return direction.normalized;
        }

        private void RequestStingerShot(Vector3 origin, Vector3 direction)
        {
            RequestShot(origin, direction, BattleMoleculeShotKind.Stinger);
        }
    }
}
