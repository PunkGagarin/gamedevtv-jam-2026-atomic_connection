using System.Collections.Generic;
using _Project.Scripts.Gameplay.Units.BattleMolecules;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.AtomCores.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(OwnedAtomReceiver))]
    [RequireComponent(typeof(AtomCoreConnectionAtomSource))]
    [RequireComponent(typeof(AtomCoreConnectionAtomMotion))]
    public class AtomCoreConnectionAtomFlow : MonoBehaviour
    {
        private readonly List<ConnectionAtomFlowState> _flowAtoms = new();
        private readonly List<ConnectionAtomFlowState> _startedFlowAtoms = new();

        [field: SerializeField] private OwnedAtomReceiver AtomReceiver { get; set; }
        [field: SerializeField] private AtomCoreConnectionAtomSource AtomSource { get; set; }
        [field: SerializeField] private AtomCoreConnectionAtomMotion AtomMotion { get; set; }

        private void Awake()
        {
            if (AtomReceiver == null)
                AtomReceiver = GetComponent<OwnedAtomReceiver>();

            if (AtomSource == null)
                AtomSource = GetComponent<AtomCoreConnectionAtomSource>();

            if (AtomMotion == null)
                AtomMotion = GetComponent<AtomCoreConnectionAtomMotion>();
        }

        public void Tick(BattleMolecule target, float deltaTime)
        {
            if (deltaTime <= 0f)
                return;

            if (target == null)
            {
                SetAllFlowAtomsReturning();
                TickFlowAtoms(deltaTime);
                return;
            }

            AssignFlowTargets(target);

            TryStartFlowAtoms(target);
            TickFlowAtoms(deltaTime);
        }

        public void ReleaseControl()
        {
            foreach (ConnectionAtomFlowState flowAtom in _flowAtoms)
                ReleaseFlowAtomControl(flowAtom);

            _flowAtoms.Clear();
            _startedFlowAtoms.Clear();
        }

        private void TryStartFlowAtoms(BattleMolecule target)
        {
            if (AtomSource == null || AtomMotion == null || target == null || !target.CanReceiveConnectionAtom)
                return;

            int atomsToStart = FlowAtomStartCount(target);

            if (atomsToStart <= 0)
                return;

            AtomSource.StartFlowAtoms(target, atomsToStart, _flowAtoms, AtomMotion, _startedFlowAtoms);
            _flowAtoms.AddRange(_startedFlowAtoms);
        }

        private void AssignFlowTargets(BattleMolecule target)
        {
            int atomsAllowed = target != null ? target.ConnectionAtomsRemaining : 0;
            int assignedAtoms = 0;

            foreach (ConnectionAtomFlowState flowAtom in _flowAtoms)
            {
                if (flowAtom.Atom == null)
                    continue;

                if (flowAtom.Target == target && assignedAtoms < atomsAllowed)
                {
                    if (flowAtom.Phase == ConnectionAtomFlowPhase.ReturnToCore)
                        RetargetFlowAtom(target, flowAtom);

                    assignedAtoms++;
                    continue;
                }

                if (assignedAtoms >= atomsAllowed)
                {
                    SetFlowAtomReturning(flowAtom);
                    continue;
                }

                RetargetFlowAtom(target, flowAtom);
                assignedAtoms++;
            }
        }

        private void RetargetFlowAtom(BattleMolecule target, ConnectionAtomFlowState flowAtom)
        {
            flowAtom.ClearConnectionProgress();
            flowAtom.Target = target;
            flowAtom.Phase = AtomMotion != null
                ? AtomMotion.RetargetPhase(flowAtom)
                : ConnectionAtomFlowPhase.MoveToRim;
        }

        private void TickFlowAtoms(float deltaTime)
        {
            for (int i = _flowAtoms.Count - 1; i >= 0; i--)
            {
                ConnectionAtomFlowState flowAtom = _flowAtoms[i];

                if (flowAtom.Atom == null)
                {
                    _flowAtoms.RemoveAt(i);
                    continue;
                }

                if (AtomSource != null && AtomSource.IsDragInterrupted(flowAtom))
                {
                    ReleaseFlowAtomControl(flowAtom);
                    _flowAtoms.RemoveAt(i);
                    continue;
                }

                if (TickFlowAtom(flowAtom, deltaTime))
                    _flowAtoms.RemoveAt(i);
            }
        }

        private bool TickFlowAtom(ConnectionAtomFlowState flowAtom, float deltaTime)
        {
            if (flowAtom.Atom == null)
                return true;

            BattleMolecule target = flowAtom.Target;

            if (target == null || !target.CanReceiveConnectionAtom)
            {
                SetFlowAtomReturning(flowAtom);
                target = null;
            }

            if (AtomMotion == null)
            {
                ReleaseFlowAtomControl(flowAtom);
                return true;
            }

            ConnectionAtomFlowMotionResult result = AtomMotion.Tick(flowAtom, deltaTime);

            switch (result)
            {
                case ConnectionAtomFlowMotionResult.DeliverToTarget:
                    return DeliverFlowAtom(flowAtom, target);
                case ConnectionAtomFlowMotionResult.ReturnToCore:
                    ReturnFlowAtomToCore(flowAtom);
                    return true;
                case ConnectionAtomFlowMotionResult.Remove:
                    return true;
                case ConnectionAtomFlowMotionResult.Continue:
                    return false;
                default:
                    return false;
            }
        }

        private void ReturnFlowAtomToCore(ConnectionAtomFlowState flowAtom)
        {
            if (flowAtom.Atom == null)
                return;

            flowAtom.Atom.EndConnectionFlow();
            AtomReceiver?.TryTake(flowAtom.Atom);
        }

        private bool DeliverFlowAtom(ConnectionAtomFlowState flowAtom, BattleMolecule target)
        {
            if (flowAtom.Atom == null)
                return true;

            if (target != null && target.TryReceiveConnectionAtom(flowAtom.Atom))
                return true;

            SetFlowAtomReturning(flowAtom);
            return false;
        }

        private int FlowAtomStartCount(BattleMolecule target)
        {
            int atomsRemaining = target.ConnectionAtomsRemaining - CountFlowAtomsTargeting(target);

            if (atomsRemaining <= 0)
                return 0;

            return atomsRemaining;
        }

        private int CountFlowAtomsTargeting(BattleMolecule target)
        {
            int count = 0;

            foreach (ConnectionAtomFlowState flowAtom in _flowAtoms)
            {
                if (flowAtom.Atom != null && flowAtom.Target == target && flowAtom.Phase != ConnectionAtomFlowPhase.ReturnToCore)
                    count++;
            }

            return count;
        }

        private void SetAllFlowAtomsReturning()
        {
            foreach (ConnectionAtomFlowState flowAtom in _flowAtoms)
                SetFlowAtomReturning(flowAtom);
        }

        private static void SetFlowAtomReturning(ConnectionAtomFlowState flowAtom)
        {
            flowAtom.ClearConnectionProgress();
            flowAtom.Target = null;
            flowAtom.Phase = ConnectionAtomFlowPhase.ReturnToCore;
        }

        private static void ReleaseFlowAtomControl(ConnectionAtomFlowState flowAtom)
        {
            SetFlowAtomParent(flowAtom, flowAtom.Atom != null ? flowAtom.Atom.Owner : null);
            flowAtom.Atom?.EndConnectionFlow();
            flowAtom.ClearConnectionProgress();
            flowAtom.Target = null;
            flowAtom.Phase = ConnectionAtomFlowPhase.None;
            flowAtom.Radius = 0f;
        }

        private static void SetFlowAtomParent(ConnectionAtomFlowState flowAtom, Transform parent)
        {
            if (flowAtom.Atom == null || parent == null || flowAtom.Atom.transform.parent == parent)
                return;

            flowAtom.Atom.transform.SetParent(parent, true);
        }
    }
}
