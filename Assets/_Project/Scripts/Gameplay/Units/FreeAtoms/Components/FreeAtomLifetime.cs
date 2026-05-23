using System;
using _Project.Scripts.Gameplay.Units.FreeAtoms;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.FreeAtoms.Components
{
    [DisallowMultipleComponent]
    public class FreeAtomLifetime : MonoBehaviour
    {
        [field: SerializeField] private FreeAtom Atom { get; set; }

        public event Action<FreeAtom> Destroyed;
        public event Action<FreeAtom> DespawnRequested;

        private void Awake()
        {
            if (Atom == null)
                Atom = GetComponent<FreeAtom>();
        }

        private void OnDestroy()
        {
            Destroyed?.Invoke(Atom);
        }

        public void RequestDespawn()
        {
            DespawnRequested?.Invoke(Atom);
        }
    }
}
