using _Project.Scripts.Gameplay.Drag;
using _Project.Scripts.Gameplay.Units.FreeAtoms;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.AtomCores.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AtomCore))]
    public class AtomCoreAtomDragSource : MonoBehaviour, IDragSource
    {
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

            return _core.OwnedAtoms.TryGetFirstOwned(FreeAtomOwnerKind.Core, out FreeAtom atom)
                ? atom
                : null;
        }
    }
}
