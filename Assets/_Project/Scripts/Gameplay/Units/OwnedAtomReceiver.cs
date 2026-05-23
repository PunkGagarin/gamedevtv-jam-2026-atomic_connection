using _Project.Scripts.Gameplay.Units.FreeAtoms;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(OwnedAtoms))]
    public class OwnedAtomReceiver : MonoBehaviour
    {
        [field: SerializeField] private OwnedAtoms OwnedAtoms { get; set; }
        [field: SerializeField] private FreeAtomOwnerKind OwnerKind { get; set; }

        private void Awake()
        {
            if (OwnedAtoms == null)
                OwnedAtoms = GetComponent<OwnedAtoms>();
        }

        public void Configure(FreeAtomOwnerKind ownerKind)
        {
            OwnerKind = ownerKind;
        }

        public bool TryTake(FreeAtom atom)
        {
            if (atom == null || OwnedAtoms == null)
                return false;

            OwnedAtoms.TakeOwnership(atom, OwnerKind);
            return true;
        }

        public void ReleaseAll()
        {
            OwnedAtoms?.ReleaseAll();
        }
    }
}
