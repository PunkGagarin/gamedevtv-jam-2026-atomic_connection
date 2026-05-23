using System.Collections.Generic;
using _Project.Scripts.Gameplay.Drag;
using _Project.Scripts.Gameplay.Units;
using _Project.Scripts.Gameplay.Units.FreeAtoms;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.AtomCores.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AtomCore))]
    [RequireComponent(typeof(OwnedAtoms))]
    public class AtomCoreAtomDragSource : MonoBehaviour, IDragSource
    {
        private readonly List<FreeAtom> _atomsBuffer = new();
        [field: SerializeField] private AtomCore Core { get; set; }
        [field: SerializeField] private OwnedAtoms OwnedAtoms { get; set; }

        private void Awake()
        {
            if (Core == null)
                Core = GetComponent<AtomCore>();

            if (OwnedAtoms == null)
                OwnedAtoms = GetComponent<OwnedAtoms>();
        }

        public IDraggable GetDraggable()
        {
            if (Core == null || !Core.IsAlive || OwnedAtoms == null)
                return null;

            OwnedAtoms.GetOwned(FreeAtomOwnerKind.Core, _atomsBuffer);
            FreeAtom fallback = null;

            foreach (FreeAtom atom in _atomsBuffer)
            {
                if (atom == null)
                    continue;

                fallback ??= atom;

                if (!atom.IsInConnectionFlow)
                    return atom.Draggable;
            }

            return fallback != null ? fallback.Draggable : null;
        }
    }
}
