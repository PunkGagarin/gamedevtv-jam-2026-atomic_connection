using _Project.Scripts.Gameplay.Drag;
using _Project.Scripts.Gameplay.Units;
using _Project.Scripts.Gameplay.Units.FreeAtoms;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    [RequireComponent(typeof(OwnedAtoms))]
    [RequireComponent(typeof(BattleMoleculeCharge))]
    [RequireComponent(typeof(BattleMoleculeBond))]
    public class BattleMoleculeAtomReceiver : MonoBehaviour
    {
        [field: SerializeField] private OwnedAtoms OwnedAtoms { get; set; }
        [field: SerializeField] private BattleMoleculeCharge Charge { get; set; }
        [field: SerializeField] private BattleMoleculeBond Bond { get; set; }

        public bool CanReceiveConnectionAtom => Bond != null
                                                && Bond.IsBonded
                                                && Charge != null
                                                && OwnedAtoms != null
                                                && Charge.CanReceiveAtom(OwnedAtoms.Count);

        public int ConnectionAtomsRemaining => Bond != null && Bond.IsBonded && Charge != null && OwnedAtoms != null
            ? Charge.RemainingAtoms(OwnedAtoms.Count)
            : 0;

        private void Awake()
        {
            if (OwnedAtoms == null)
                OwnedAtoms = GetComponent<OwnedAtoms>();

            if (Charge == null)
                Charge = GetComponent<BattleMoleculeCharge>();

            if (Bond == null)
                Bond = GetComponent<BattleMoleculeBond>();
        }

        public bool CanAcceptDrop(IDraggable draggable)
        {
            if (draggable is not FreeAtom)
                return false;

            return Bond != null && Bond.CanReceiveAtom || CanReceiveConnectionAtom;
        }

        public void AcceptDrop(IDraggable draggable)
        {
            if (draggable is not FreeAtom freeAtom)
                return;

            if (Bond != null && Bond.CanReceiveAtom)
            {
                TryAcceptBondAtom(freeAtom);
                return;
            }

            TryReceiveConnectionAtom(freeAtom);
        }

        public bool TryAcceptBondAtom(FreeAtom freeAtom)
        {
            if (freeAtom == null || Bond == null)
                return false;

            return Bond.TryAcceptAtom(freeAtom);
        }

        public bool TryReceiveConnectionAtom(FreeAtom freeAtom)
        {
            if (freeAtom == null || !CanReceiveConnectionAtom)
                return false;

            freeAtom.EndConnectionFlow();
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
