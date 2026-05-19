using System;
using _Project.Scripts.Gameplay.Units;
using _Project.Scripts.Gameplay.Units.BattleMolecules;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    [RequireComponent(typeof(OwnedAtoms))]
    [RequireComponent(typeof(BattleMoleculeCharge))]
    public class BattleMoleculeShotQueue : MonoBehaviour
    {
        [field: SerializeField] private OwnedAtoms OwnedAtoms { get; set; }
        [field: SerializeField] private BattleMoleculeCharge Charge { get; set; }

        public event Action<BattleMoleculeShotRequest> ShotRequested;

        private void Awake()
        {
            if (OwnedAtoms == null)
                OwnedAtoms = GetComponent<OwnedAtoms>();

            if (Charge == null)
                Charge = GetComponent<BattleMoleculeCharge>();
        }

        public virtual void Configure(BattleMoleculeConfig config)
        {
        }

        public virtual bool TryRequestShot(Vector3 direction)
        {
            if (!CanRequestShot(direction))
                return false;

            Vector3 origin = transform.position;
            Vector3 shotDirection = NormalizeShotDirection(direction);

            SpendCharge();
            RequestShot(origin, shotDirection, BattleMoleculeShotKind.Regular);

            return true;
        }

        protected bool CanRequestShot(Vector3 direction)
        {
            return Charge != null
                   && OwnedAtoms != null
                   && Charge.IsCharged
                   && direction.sqrMagnitude > Mathf.Epsilon;
        }

        protected void SpendCharge()
        {
            Charge.Spend();
            OwnedAtoms.ReleaseAll();
        }

        protected void RequestShot(Vector3 origin, Vector3 direction, BattleMoleculeShotKind kind)
        {
            ShotRequested?.Invoke(new BattleMoleculeShotRequest(origin, direction, kind));
        }

        protected static Vector3 NormalizeShotDirection(Vector3 direction)
        {
            return direction.normalized;
        }
    }
}
