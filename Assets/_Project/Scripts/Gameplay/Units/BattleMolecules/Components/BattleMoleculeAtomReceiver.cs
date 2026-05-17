using _Project.Scripts.Gameplay.Drag;
using _Project.Scripts.Gameplay.Units;
using _Project.Scripts.Gameplay.Units.FreeAtoms;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    [RequireComponent(typeof(OwnedAtoms))]
    [RequireComponent(typeof(BattleMoleculeCharge))]
    public class BattleMoleculeAtomReceiver : MonoBehaviour, IDropTarget
    {
        [field: SerializeField] private OwnedAtoms OwnedAtoms { get; set; }
        [field: SerializeField] private BattleMoleculeCharge Charge { get; set; }

        private float _atomsPosCircleRadius;

        private void Awake()
        {
            if (OwnedAtoms == null)
                OwnedAtoms = GetComponent<OwnedAtoms>();

            if (Charge == null)
                Charge = GetComponent<BattleMoleculeCharge>();
        }

        public void Configure(float atomsPosCircleRadius)
        {
            _atomsPosCircleRadius = atomsPosCircleRadius;
        }

        public bool CanAcceptDrop(IDraggable draggable)
        {
            return draggable is FreeAtom && Charge.CanReceiveAtom(OwnedAtoms.Count);
        }

        public void OnDropAccepted(IDraggable draggable)
        {
            if (draggable is not FreeAtom freeAtom)
                return;

            DisableCollider(freeAtom);

            float angle = GetCircleAngle(OwnedAtoms.Count, Charge.AtomsRequired);
            OwnedAtoms.TakeOwnership(freeAtom, FreeAtomOwnerKind.BattleMolecule);
            freeAtom.OrbitMotion?.Configure(transform, _atomsPosCircleRadius, angle);
            Charge.RegisterAtomCount(OwnedAtoms.Count);
        }

        public void OnDropRejected(IDraggable draggable)
        {
        }

        private static void DisableCollider(FreeAtom freeAtom)
        {
            Collider2D col = freeAtom.GetComponent<Collider2D>();
            if (col != null)
                col.enabled = false;
        }

        private static float GetCircleAngle(int index, int total)
        {
            if (total <= 0)
                return 0;

            return (index / (float)total) * Mathf.PI * 2f;
        }
    }
}
