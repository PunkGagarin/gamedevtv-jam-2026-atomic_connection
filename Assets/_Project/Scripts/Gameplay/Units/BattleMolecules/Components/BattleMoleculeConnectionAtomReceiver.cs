using _Project.Scripts.Gameplay.Units;
using _Project.Scripts.Gameplay.Units.FreeAtoms;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(OwnedAtoms))]
    [RequireComponent(typeof(OwnedAtomReceiver))]
    [RequireComponent(typeof(BattleMoleculeCharge))]
    [RequireComponent(typeof(BattleMoleculeBond))]
    public class BattleMoleculeConnectionAtomReceiver : MonoBehaviour
    {
        [field: SerializeField] private OwnedAtoms OwnedAtoms { get; set; }
        [field: SerializeField] private OwnedAtomReceiver AtomReceiver { get; set; }
        [field: SerializeField] private BattleMoleculeCharge Charge { get; set; }
        [field: SerializeField] private BattleMoleculeBond Bond { get; set; }

        public bool CanReceiveAtom => Bond.IsBonded
                                      && Charge.CanReceiveAtom(OwnedAtoms.Count);

        public int RemainingAtoms => Bond.IsBonded
            ? Charge.RemainingAtoms(OwnedAtoms.Count)
            : 0;

        private void Awake()
        {
            if (OwnedAtoms == null)
                OwnedAtoms = GetComponent<OwnedAtoms>();

            if (AtomReceiver == null)
                AtomReceiver = GetComponent<OwnedAtomReceiver>();

            if (Charge == null)
                Charge = GetComponent<BattleMoleculeCharge>();

            if (Bond == null)
                Bond = GetComponent<BattleMoleculeBond>();
        }

        public bool TryReceive(FreeAtom atom)
        {
            if (atom == null || !CanReceiveAtom)
                return false;

            atom.EndConnectionFlow();
            atom.SetCollisionEnabled(false);

            if (!AtomReceiver.TryTake(atom))
                return false;

            Charge.RegisterAtomCount(OwnedAtoms.Count);
            return true;
        }
    }
}
