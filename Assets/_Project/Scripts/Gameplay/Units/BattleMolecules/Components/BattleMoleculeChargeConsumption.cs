using _Project.Scripts.Gameplay.Units;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(OwnedAtoms))]
    [RequireComponent(typeof(BattleMoleculeCharge))]
    public class BattleMoleculeChargeConsumption : MonoBehaviour
    {
        [field: SerializeField] private OwnedAtoms OwnedAtoms { get; set; }
        [field: SerializeField] private BattleMoleculeCharge Charge { get; set; }

        public bool CanConsume => OwnedAtoms != null && Charge != null && Charge.IsCharged;

        private void Awake()
        {
            if (OwnedAtoms == null)
                OwnedAtoms = GetComponent<OwnedAtoms>();

            if (Charge == null)
                Charge = GetComponent<BattleMoleculeCharge>();
        }

        public bool TryConsume()
        {
            if (!CanConsume)
                return false;

            Charge.Spend();
            OwnedAtoms.ReleaseAll();
            return true;
        }
    }
}
