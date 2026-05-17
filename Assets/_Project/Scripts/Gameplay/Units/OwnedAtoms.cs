using System.Collections.Generic;
using _Project.Scripts.Gameplay.Units.FreeAtoms;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units
{
    public class OwnedAtoms : MonoBehaviour
    {
        private readonly List<FreeAtom> _atoms = new();

        public int Count => _atoms.Count;

        public void TakeOwnership(FreeAtom atom, FreeAtomOwnerKind ownerKind)
        {
            if (atom == null)
                return;

            if (!_atoms.Contains(atom))
            {
                _atoms.Add(atom);
                atom.OwnerChanged += OnAtomOwnerChanged;
                atom.Destroyed += OnAtomDestroyed;
            }

            atom.transform.SetParent(transform, true);
            atom.AssignOwner(ownerKind, transform);
        }

        public void TickOrbit(float angleDelta)
        {
            foreach (FreeAtom atom in _atoms)
            {
                if (atom.CanOrbit)
                    atom.OrbitMotion?.Tick(angleDelta);
            }
        }

        public void ReleaseAll()
        {
            for (int i = _atoms.Count - 1; i >= 0; i--)
            {
                FreeAtom atom = _atoms[i];
                Remove(atom);

                if (atom != null)
                    atom.RequestDespawn();
            }
        }

        private void OnDestroy()
        {
            for (int i = _atoms.Count - 1; i >= 0; i--)
                Remove(_atoms[i]);
        }

        private void OnAtomOwnerChanged(FreeAtom atom, FreeAtomOwnerKind ownerKind)
        {
            if (atom == null || atom.Owner != transform)
                Remove(atom);
        }

        private void OnAtomDestroyed(FreeAtom atom)
        {
            Remove(atom);
        }

        private void Remove(FreeAtom atom)
        {
            if (atom != null)
            {
                atom.OwnerChanged -= OnAtomOwnerChanged;
                atom.Destroyed -= OnAtomDestroyed;
            }

            _atoms.Remove(atom);
        }
    }
}
