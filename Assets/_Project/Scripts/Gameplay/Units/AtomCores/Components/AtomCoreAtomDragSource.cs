using System.Collections.Generic;
using _Project.Scripts.Gameplay.Drag;
using _Project.Scripts.Gameplay.Units.FreeAtoms;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.AtomCores.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AtomCore))]
    public class AtomCoreAtomDragSource : MonoBehaviour, IDragSource
    {
        private readonly List<FreeAtom> _atomsBuffer = new();
        private AtomCore _core;

        private void Awake()
        {
            _core = GetComponent<AtomCore>();
        }

        public IDraggable GetDraggable()
        {
            if (_core == null)
                _core = GetComponent<AtomCore>();

            if (_core == null || !_core.IsAlive || _core.OwnedAtoms == null)
                return null;

            _core.OwnedAtoms.GetOwned(FreeAtomOwnerKind.Core, _atomsBuffer);
            FreeAtom fallback = null;

            foreach (FreeAtom atom in _atomsBuffer)
            {
                if (atom == null)
                    continue;

                fallback ??= atom;

                if (!atom.IsInConnectionFlow)
                    return atom;
            }

            return fallback;
        }
    }
}
