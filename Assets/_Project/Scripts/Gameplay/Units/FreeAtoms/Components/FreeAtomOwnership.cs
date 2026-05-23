using System;
using _Project.Scripts.Gameplay.Common.Movement;
using _Project.Scripts.Gameplay.Units.FreeAtoms;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.FreeAtoms.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(OrbitMotion))]
    public class FreeAtomOwnership : MonoBehaviour
    {
        [field: SerializeField] private FreeAtom Atom { get; set; }
        [field: SerializeField] private OrbitMotion OrbitMotion { get; set; }

        public FreeAtomOwnerKind OwnerKind { get; private set; }
        public Transform Owner { get; private set; }

        public event Action<FreeAtom, FreeAtomOwnerKind> OwnerChanged;

        private void Awake()
        {
            if (Atom == null)
                Atom = GetComponent<FreeAtom>();

            if (OrbitMotion == null)
                OrbitMotion = GetComponent<OrbitMotion>();
        }

        public void AssignOwner(FreeAtomOwnerKind ownerKind, Transform owner)
        {
            OwnerKind = ownerKind;
            Owner = owner;
            OwnerChanged?.Invoke(Atom, OwnerKind);
        }

        public void ClearOwner()
        {
            OwnerKind = FreeAtomOwnerKind.None;
            Owner = null;
            OrbitMotion?.Clear();
            OwnerChanged?.Invoke(Atom, OwnerKind);
        }
    }
}
