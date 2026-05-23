using System.Collections.Generic;
using _Project.Scripts.Gameplay.Drag;
using _Project.Scripts.Gameplay.Units;
using _Project.Scripts.Gameplay.Units.BattleMolecules;
using _Project.Scripts.Gameplay.Units.FreeAtoms;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Units.AtomCores.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(OwnedAtoms))]
    public class AtomCoreConnectionAtomSource : MonoBehaviour
    {
        private readonly List<FreeAtom> _coreAtoms = new();

        [field: SerializeField] private OwnedAtoms OwnedAtoms { get; set; }

        [Inject] private IDragService _dragService;

        private void Awake()
        {
            if (OwnedAtoms == null)
                OwnedAtoms = GetComponent<OwnedAtoms>();
        }

        internal void StartFlowAtoms(
            BattleMolecule target,
            int atomsToStart,
            List<ConnectionAtomFlowState> activeFlowAtoms,
            AtomCoreConnectionAtomMotion motion,
            List<ConnectionAtomFlowState> results)
        {
            results?.Clear();

            if (results == null || OwnedAtoms == null || target == null || !target.CanReceiveConnectionAtom)
                return;

            if (atomsToStart <= 0 || motion == null)
                return;

            if (_dragService != null && _dragService.IsDragActive)
                return;

            OwnedAtoms.GetOwned(FreeAtomOwnerKind.Core, _coreAtoms);

            foreach (FreeAtom candidate in _coreAtoms)
            {
                if (atomsToStart <= 0)
                    return;

                if (!CanStartFlowAtom(candidate, activeFlowAtoms))
                    continue;

                ConnectionAtomFlowState flowAtom = new()
                {
                    Atom = candidate,
                    Target = target,
                    Phase = ConnectionAtomFlowPhase.OrbitToConnection
                };

                motion.InitializeStartState(flowAtom);
                candidate.BeginConnectionFlow();
                results.Add(flowAtom);
                atomsToStart--;
            }
        }

        internal bool IsDragInterrupted(ConnectionAtomFlowState flowAtom)
        {
            return _dragService != null
                   && flowAtom?.Atom != null
                   && _dragService.IsDragging(flowAtom.Atom.Draggable);
        }

        private bool CanStartFlowAtom(FreeAtom atom, List<ConnectionAtomFlowState> activeFlowAtoms)
        {
            if (atom == null || atom.IsInConnectionFlow)
                return false;

            if (IsFlowAtom(atom, activeFlowAtoms))
                return false;

            return atom.Draggable == null || _dragService == null || !_dragService.IsReserved(atom.Draggable);
        }

        private static bool IsFlowAtom(FreeAtom atom, List<ConnectionAtomFlowState> activeFlowAtoms)
        {
            if (activeFlowAtoms == null)
                return false;

            foreach (ConnectionAtomFlowState flowAtom in activeFlowAtoms)
            {
                if (flowAtom.Atom == atom)
                    return true;
            }

            return false;
        }
    }
}
