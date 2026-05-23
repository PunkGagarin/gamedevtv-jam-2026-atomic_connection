using System;
using _Project.Scripts.Gameplay.Common.Progress;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    public class BattleMoleculeCharge : MonoBehaviour
    {
        private readonly CompletionThreshold _atoms = new();

        public bool IsCharged { get; private set; }

        public event Action Charged;
        public event Action Spent;

        public void Configure(int atomsRequired)
        {
            _atoms.Configure(atomsRequired);
            ResetCharge();
        }

        public bool CanReceiveAtom(int currentAtomsCount)
        {
            return _atoms.HasRequirement
                   && !IsCharged
                   && currentAtomsCount < _atoms.Required;
        }

        public int RemainingAtoms(int currentAtomsCount)
        {
            if (!_atoms.HasRequirement || IsCharged)
                return 0;

            return _atoms.RemainingFrom(currentAtomsCount);
        }

        public void RegisterAtomCount(int atomsCount)
        {
            if (!_atoms.HasRequirement || IsCharged || !_atoms.SetCurrent(atomsCount))
                return;

            IsCharged = true;
            Charged?.Invoke();
        }

        public void Spend()
        {
            if (!IsCharged)
                return;

            _atoms.Reset();
            IsCharged = false;
            Spent?.Invoke();
        }

        private void ResetCharge()
        {
            _atoms.Reset();
            IsCharged = false;
            Spent?.Invoke();
        }
    }
}
