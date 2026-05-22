using System;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    public class BattleMoleculeCharge : MonoBehaviour
    {
        private int _atomsRequired;

        public bool IsCharged { get; private set; }
        public int AtomsRequired => _atomsRequired;

        public event Action Charged;
        public event Action Spent;

        public void Configure(int atomsRequired)
        {
            _atomsRequired = atomsRequired;
            ResetCharge();
        }

        public bool CanReceiveAtom(int currentAtomsCount)
        {
            return _atomsRequired > 0
                   && !IsCharged
                   && currentAtomsCount < _atomsRequired;
        }

        public int RemainingAtoms(int currentAtomsCount)
        {
            if (_atomsRequired <= 0 || IsCharged)
                return 0;

            return Mathf.Max(0, _atomsRequired - currentAtomsCount);
        }

        public void RegisterAtomCount(int atomsCount)
        {
            if (_atomsRequired <= 0 || IsCharged || atomsCount < _atomsRequired)
                return;

            IsCharged = true;
            Charged?.Invoke();
        }

        public void Spend()
        {
            if (!IsCharged)
                return;

            IsCharged = false;
            Spent?.Invoke();
        }

        private void ResetCharge()
        {
            IsCharged = false;
            Spent?.Invoke();
        }
    }
}
