using System;
using _Project.Scripts.Gameplay.Units;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    [RequireComponent(typeof(OwnedAtoms))]
    [RequireComponent(typeof(BattleMoleculeCharge))]
    public class BattleMoleculeShotQueue : MonoBehaviour
    {
        [field: SerializeField] private OwnedAtoms OwnedAtoms { get; set; }
        [field: SerializeField] private BattleMoleculeCharge Charge { get; set; }

        public event Action<Vector3, Vector3> ShotRequested;

        private void Awake()
        {
            if (OwnedAtoms == null)
                OwnedAtoms = GetComponent<OwnedAtoms>();

            if (Charge == null)
                Charge = GetComponent<BattleMoleculeCharge>();
        }

        public bool TryRequestShot(Vector3 direction)
        {
            if (!Charge.IsCharged || direction.sqrMagnitude <= Mathf.Epsilon)
                return false;

            Vector3 origin = transform.position;
            Vector3 shotDirection = direction.normalized;

            Charge.Spend();
            OwnedAtoms.ReleaseAll();
            ShotRequested?.Invoke(origin, shotDirection);

            return true;
        }
    }
}
