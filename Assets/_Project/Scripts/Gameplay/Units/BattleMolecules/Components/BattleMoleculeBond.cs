using System;
using _Project.Scripts.Gameplay.Common.Progress;
using UnityEngine;

using _Project.Scripts.Gameplay.Units.FreeAtoms;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    public class BattleMoleculeBond : MonoBehaviour
    {
        private readonly CompletionThreshold _atoms = new();

        public bool IsBonded { get; private set; }
        public bool CanReceiveAtom => !IsBonded && _atoms.Current < _atoms.Required;

        public event Action Bonded;
        public event Action Changed;

        public void Configure(int atomsRequired)
        {
            _atoms.Configure(atomsRequired);
            IsBonded = _atoms.IsComplete;
            Changed?.Invoke();
        }

        public bool TryAcceptAtom(FreeAtom atom)
        {
            if (atom == null || !CanReceiveAtom)
                return false;

            bool isComplete = _atoms.Advance();
            atom.RequestDespawn();

            if (isComplete)
            {
                IsBonded = true;
                Bonded?.Invoke();
            }

            Changed?.Invoke();
            return true;
        }
    }
}
