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
        private readonly List<FreeAtom> _supplementalAtoms = new();
        private readonly Dictionary<FreeAtom, int> _atomOrder = new();

        [field: SerializeField] private OwnedAtoms OwnedAtoms { get; set; }

        [Inject] private IDragService _dragService;
        [Inject] private IBattleMoleculeConnectionAtomSourceProvider _supplementalAtomSourceProvider;

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

            int atomsRemainingToStart = atomsToStart;

            OwnedAtoms.GetOwned(FreeAtomOwnerKind.Core, _coreAtoms);
            SortAtomsByDistanceTo(target, _coreAtoms);
            StartFlowAtomsFromCandidates(target, _coreAtoms, activeFlowAtoms, motion, results, ref atomsRemainingToStart);

            if (atomsRemainingToStart <= 0)
                return;

            _supplementalAtomSourceProvider?.CollectSupplementalConnectionAtoms(target, _supplementalAtoms);
            SortAtomsByDistanceTo(target, _supplementalAtoms);
            StartFlowAtomsFromCandidates(target, _supplementalAtoms, activeFlowAtoms, motion, results, ref atomsRemainingToStart);
        }

        private void StartFlowAtomsFromCandidates(
            BattleMolecule target,
            List<FreeAtom> candidates,
            List<ConnectionAtomFlowState> activeFlowAtoms,
            AtomCoreConnectionAtomMotion motion,
            List<ConnectionAtomFlowState> results,
            ref int atomsToStart)
        {
            foreach (FreeAtom candidate in candidates)
            {
                if (atomsToStart <= 0)
                    return;

                if (!TryStartFlowAtom(target, candidate, activeFlowAtoms, motion, results))
                    continue;

                atomsToStart--;
            }
        }

        private bool TryStartFlowAtom(
            BattleMolecule target,
            FreeAtom candidate,
            List<ConnectionAtomFlowState> activeFlowAtoms,
            AtomCoreConnectionAtomMotion motion,
            List<ConnectionAtomFlowState> results)
        {
            if (!CanStartFlowAtom(candidate, activeFlowAtoms))
                return false;

            BattleMolecule source = TryGetSourceMolecule(candidate);
            ConnectionAtomFlowState flowAtom = new()
            {
                Atom = candidate,
                Source = source,
                Target = target,
                Phase = source != null
                    ? ConnectionAtomFlowPhase.MoveToSourceConnection
                    : ConnectionAtomFlowPhase.OrbitToConnection
            };

            candidate.BeginConnectionFlow();

            if (candidate.Owner != transform || candidate.OwnerKind != FreeAtomOwnerKind.Core)
                OwnedAtoms.TakeOwnership(candidate, FreeAtomOwnerKind.Core);

            motion.InitializeStartState(flowAtom);
            results.Add(flowAtom);
            return true;
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

        private BattleMolecule TryGetSourceMolecule(FreeAtom atom)
        {
            if (atom == null || atom.Owner == null || atom.Owner == transform)
                return null;

            return atom.Owner.GetComponent<BattleMolecule>();
        }

        private void SortAtomsByDistanceTo(BattleMolecule target, List<FreeAtom> atoms)
        {
            if (target == null || atoms == null)
                return;

            Vector3 targetPosition = target.transform.position;
            _atomOrder.Clear();

            for (int i = 0; i < atoms.Count; i++)
                _atomOrder[atoms[i]] = i;

            atoms.Sort((left, right) =>
            {
                if (left == right)
                    return 0;

                if (left == null)
                    return 1;

                if (right == null)
                    return -1;

                float leftDistanceSqr = (left.transform.position - targetPosition).sqrMagnitude;
                float rightDistanceSqr = (right.transform.position - targetPosition).sqrMagnitude;
                int distanceComparison = leftDistanceSqr.CompareTo(rightDistanceSqr);
                return distanceComparison != 0
                    ? distanceComparison
                    : _atomOrder[left].CompareTo(_atomOrder[right]);
            });
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
