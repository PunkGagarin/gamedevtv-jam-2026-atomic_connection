using _Project.Scripts.Gameplay.Drag;
using _Project.Scripts.Gameplay.Units.FreeAtoms;
using _Project.Scripts.Gameplay.Units.FreeAtoms.Components;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    [RequireComponent(typeof(BattleMoleculeBond))]
    [RequireComponent(typeof(BattleMoleculeConnectionAtomReceiver))]
    public class BattleMoleculeAtomReceiver : MonoBehaviour
    {
        [field: SerializeField] private BattleMoleculeBond Bond { get; set; }
        [field: SerializeField] private BattleMoleculeConnectionAtomReceiver ConnectionReceiver { get; set; }

        private void Awake()
        {
            if (Bond == null)
                Bond = GetComponent<BattleMoleculeBond>();

            if (ConnectionReceiver == null)
                ConnectionReceiver = GetComponent<BattleMoleculeConnectionAtomReceiver>();
        }

        internal bool CanAcceptDrop(IDraggable draggable)
        {
            if (!TryGetFreeAtom(draggable, out _))
                return false;

            return Bond.CanReceiveAtom || ConnectionReceiver.CanReceiveAtom;
        }

        internal void AcceptDrop(IDraggable draggable)
        {
            if (!TryGetFreeAtom(draggable, out FreeAtom freeAtom))
                return;

            if (Bond.CanReceiveAtom)
            {
                Bond.TryAcceptAtom(freeAtom);
                return;
            }

            ConnectionReceiver.TryReceive(freeAtom);
        }

        private static bool TryGetFreeAtom(IDraggable draggable, out FreeAtom freeAtom)
        {
            freeAtom = draggable switch
            {
                FreeAtomDrag drag => drag.Atom,
                _ => null
            };

            return freeAtom != null;
        }
    }
}
