using _Project.Scripts.Gameplay.Drag;
using _Project.Scripts.Gameplay.Units;
using _Project.Scripts.Gameplay.Units.FreeAtoms;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    [RequireComponent(typeof(OwnedAtoms))]
    [RequireComponent(typeof(BattleMoleculeCharge))]
    public class BattleMoleculeAtomReceiver : MonoBehaviour
    {
        [field: SerializeField] private OwnedAtoms OwnedAtoms { get; set; }
        [field: SerializeField] private BattleMoleculeCharge Charge { get; set; }

        private void Awake()
        {
            if (OwnedAtoms == null)
                OwnedAtoms = GetComponent<OwnedAtoms>();

            if (Charge == null)
                Charge = GetComponent<BattleMoleculeCharge>();
        }

        public bool CanAcceptDrop(IDraggable draggable)
        {
            return draggable is FreeAtom && Charge.CanReceiveAtom(OwnedAtoms.Count);
        }

        public void AcceptDrop(IDraggable draggable)
        {
            if (draggable is not FreeAtom freeAtom)
                return;

            TryAcceptAtom(freeAtom);
        }

        public bool TryAcceptAtom(FreeAtom freeAtom)
        {
            if (freeAtom == null || Charge == null || OwnedAtoms == null)
                return false;

            if (!Charge.CanReceiveAtom(OwnedAtoms.Count))
                return false;

            DisableCollider(freeAtom);

            OwnedAtoms.TakeOwnership(freeAtom, FreeAtomOwnerKind.BattleMolecule);
            Charge.RegisterAtomCount(OwnedAtoms.Count);

            return true;
        }

        private static void DisableCollider(FreeAtom freeAtom)
        {
            Collider2D col = freeAtom.GetComponent<Collider2D>();
            if (col != null)
                col.enabled = false;
        }
    }
}
