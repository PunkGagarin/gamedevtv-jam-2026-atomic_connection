using System;
using UnityEngine;

using _Project.Scripts.Gameplay.Units.FreeAtoms;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    public class BattleMoleculeBond : MonoBehaviour
    {
        private int _atomsRequired;
        private int _atomsReceived;

        public bool IsBonded { get; private set; }
        public int AtomsRequired => _atomsRequired;
        public int AtomsReceived => _atomsReceived;
        public bool CanReceiveAtom => !IsBonded && _atomsReceived < _atomsRequired;

        public event Action Bonded;
        public event Action Changed;

        public void Configure(int atomsRequired)
        {
            _atomsRequired = Mathf.Max(0, atomsRequired);
            _atomsReceived = 0;
            IsBonded = _atomsRequired <= 0;
            Changed?.Invoke();
        }

        public bool TryAcceptAtom(FreeAtom atom)
        {
            if (atom == null || !CanReceiveAtom)
                return false;

            _atomsReceived++;
            atom.RequestDespawn();

            if (_atomsReceived >= _atomsRequired)
            {
                IsBonded = true;
                Bonded?.Invoke();
            }

            Changed?.Invoke();
            return true;
        }
    }
}
