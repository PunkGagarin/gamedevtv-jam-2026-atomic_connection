using System.Collections.Generic;
using _Project.Scripts.Gameplay.Common.Movement;
using _Project.Scripts.Gameplay.Drag;
using _Project.Scripts.Gameplay.Talents;
using _Project.Scripts.Gameplay.Units;
using _Project.Scripts.Gameplay.Units.BattleMolecules;
using _Project.Scripts.Gameplay.Units.FreeAtoms;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Units.AtomCores.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(OwnedAtoms))]
    [RequireComponent(typeof(OwnedAtomReceiver))]
    public class AtomCoreConnectionAtomFlow : MonoBehaviour
    {
        private readonly List<FreeAtom> _coreAtoms = new();
        private readonly List<FlowAtomState> _flowAtoms = new();

        [field: SerializeField] private OwnedAtoms OwnedAtoms { get; set; }
        [field: SerializeField] private OwnedAtomReceiver AtomReceiver { get; set; }

        [Inject] private BattleMoleculeConfig _config;
        [Inject] private ITalentService _talentService;
        [Inject] private IDragService _dragService;

        private void Awake()
        {
            if (OwnedAtoms == null)
                OwnedAtoms = GetComponent<OwnedAtoms>();

            if (AtomReceiver == null)
                AtomReceiver = GetComponent<OwnedAtomReceiver>();
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

            if (_dragService == null || !_dragService.IsDragActive)
                TryStartFlowAtoms(target);

            TickFlowAtoms(deltaTime);
        }

        public void ReleaseControl()
        {
            foreach (FlowAtomState flowAtom in _flowAtoms)
                ReleaseFlowAtomControl(flowAtom);

            _flowAtoms.Clear();
            _coreAtoms.Clear();
        }

        private void TryStartFlowAtoms(BattleMolecule target)
        {
            if (OwnedAtoms == null || target == null || !target.CanReceiveConnectionAtom)
                return;

            int atomsToStart = FlowAtomStartCount(target);

            if (atomsToStart <= 0)
                return;

            OwnedAtoms.GetOwned(FreeAtomOwnerKind.Core, _coreAtoms);

            foreach (FreeAtom candidate in _coreAtoms)
            {
                if (atomsToStart <= 0)
                    return;

                if (!CanStartFlowAtom(candidate))
                    continue;

                StartFlowAtom(target, candidate);
                atomsToStart--;
            }
        }

        private void StartFlowAtom(BattleMolecule target, FreeAtom atom)
        {
            FlowAtomState flowAtom = new()
            {
                Atom = atom,
                Target = target,
                Phase = FlowPhase.OrbitToConnection,
                Radius = Mathf.Max(ConnectionAtomMinimumFlowRadius(), Vector2.Distance(atom.transform.position, transform.position))
            };

            atom.BeginConnectionFlow();
            _flowAtoms.Add(flowAtom);
        }

        private void AssignFlowTargets(BattleMolecule target)
        {
            int atomsAllowed = target != null ? target.ConnectionAtomsRemaining : 0;
            int assignedAtoms = 0;

            foreach (FlowAtomState flowAtom in _flowAtoms)
            {
                if (flowAtom.Atom == null)
                    continue;

                if (flowAtom.Target == target && assignedAtoms < atomsAllowed)
                {
                    if (flowAtom.Phase == FlowPhase.ReturnToCore)
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

        private void RetargetFlowAtom(BattleMolecule target, FlowAtomState flowAtom)
        {
            ClearConnectionProgress(flowAtom);
            flowAtom.Target = target;
            flowAtom.Phase = IsAtomNearCoreRim(flowAtom)
                ? FlowPhase.OrbitToConnection
                : FlowPhase.MoveToRim;
        }

        private void TickFlowAtoms(float deltaTime)
        {
            for (int i = _flowAtoms.Count - 1; i >= 0; i--)
            {
                FlowAtomState flowAtom = _flowAtoms[i];

                if (flowAtom.Atom == null)
                {
                    _flowAtoms.RemoveAt(i);
                    continue;
                }

                if (_dragService != null && _dragService.IsDragging(flowAtom.Atom.Draggable))
                {
                    ReleaseFlowAtomControl(flowAtom);
                    _flowAtoms.RemoveAt(i);
                    continue;
                }

                if (TickFlowAtom(flowAtom, deltaTime))
                    _flowAtoms.RemoveAt(i);
            }
        }

        private bool TickFlowAtom(FlowAtomState flowAtom, float deltaTime)
        {
            if (flowAtom.Atom == null)
                return true;

            BattleMolecule target = flowAtom.Target;

            if (target == null || !target.CanReceiveConnectionAtom)
            {
                SetFlowAtomReturning(flowAtom);
                return TickReturnToCore(flowAtom, deltaTime);
            }

            switch (flowAtom.Phase)
            {
                case FlowPhase.MoveToRim:
                    if (MoveAtomToCoreRim(flowAtom, deltaTime, true))
                        flowAtom.Phase = FlowPhase.OrbitToConnection;
                    break;
                case FlowPhase.OrbitToConnection:
                    if (MoveAtomAlongCoreRim(target, flowAtom, deltaTime))
                    {
                        flowAtom.Phase = FlowPhase.Connection;
                        ClearConnectionProgress(flowAtom);
                    }
                    break;
                case FlowPhase.Connection:
                    if (MoveAtomToMolecule(target, flowAtom, deltaTime))
                        return DeliverFlowAtom(flowAtom, target);
                    break;
                case FlowPhase.ReturnToCore:
                    return TickReturnToCore(flowAtom, deltaTime);
                case FlowPhase.None:
                    flowAtom.Phase = FlowPhase.MoveToRim;
                    break;
                default:
                    flowAtom.Phase = FlowPhase.MoveToRim;
                    break;
            }

            return false;
        }

        private bool TickReturnToCore(FlowAtomState flowAtom, float deltaTime)
        {
            if (flowAtom.Atom == null)
                return true;

            if (!MoveAtomToCoreRim(flowAtom, deltaTime, false))
                return false;

            flowAtom.Atom.EndConnectionFlow();
            AtomReceiver.TryTake(flowAtom.Atom);
            return true;
        }

        private bool MoveAtomToCoreRim(FlowAtomState flowAtom, float deltaTime, bool useTalentSpeed)
        {
            SetFlowAtomParent(flowAtom, transform);

            float angle = AngleFromCoreTo(flowAtom.Atom.transform.position);
            float speed = useTalentSpeed ? ConnectionAtomTravelSpeed() : _config.ConnectionAtomTravelSpeed;
            return MoveFlowAtomTowards(flowAtom, CoreRimPosition(flowAtom, angle), speed * deltaTime);
        }

        private bool MoveAtomAlongCoreRim(BattleMolecule target, FlowAtomState flowAtom, float deltaTime)
        {
            SetFlowAtomParent(flowAtom, transform);

            float currentAngle = AngleFromCoreTo(flowAtom.Atom.transform.position) * Mathf.Rad2Deg;
            float targetAngle = AngleFromCoreTo(target.transform.position) * Mathf.Rad2Deg;
            float angleStep = ConnectionCoreRimDegreesPerSecond() * deltaTime;
            float nextAngle = angleStep > 0f
                ? Mathf.MoveTowardsAngle(currentAngle, targetAngle, angleStep)
                : targetAngle;

            flowAtom.Atom.transform.position = CoreRimPosition(flowAtom, nextAngle * Mathf.Deg2Rad);
            return Mathf.Abs(Mathf.DeltaAngle(nextAngle, targetAngle)) <= ConnectionCoreRimArrivalAngleDegrees();
        }

        private bool MoveAtomToMolecule(BattleMolecule target, FlowAtomState flowAtom, float deltaTime)
        {
            SetFlowAtomParent(flowAtom, transform);

            Vector3 start = ConnectionStartPosition(target, flowAtom);
            Vector3 destination = ConnectionDestinationPosition(target, flowAtom);
            EnsureConnectionProgress(flowAtom, start, destination);

            float distance = Vector2.Distance(start, destination);
            if (distance <= ConnectionAtomArrivalDistance())
                return true;

            flowAtom.ConnectionProgress = Mathf.Clamp01(
                flowAtom.ConnectionProgress + ConnectionAtomTravelSpeed() * deltaTime / distance);

            Vector3 position = Vector3.Lerp(start, destination, flowAtom.ConnectionProgress);
            position.z = flowAtom.Atom.transform.position.z;
            flowAtom.Atom.transform.position = position;

            return flowAtom.ConnectionProgress >= 1f;
        }

        private float ConnectionAtomSpeedMultiplier()
        {
            float bonus = _talentService != null ? _talentService.BonusOf(TalentType.ConnectionAtomSpeed) : 0f;
            return Mathf.Max(0f, 1f + bonus);
        }

        private float ConnectionAtomTravelSpeed()
        {
            return _config.ConnectionAtomTravelSpeed * ConnectionAtomSpeedMultiplier();
        }

        private float ConnectionCoreRimDegreesPerSecond()
        {
            return _config.ConnectionCoreRimDegreesPerSecond * ConnectionAtomSpeedMultiplier();
        }

        private bool MoveFlowAtomTowards(FlowAtomState flowAtom, Vector3 destination, float maxDistance)
        {
            if (flowAtom.Atom == null)
                return false;

            destination.z = flowAtom.Atom.transform.position.z;
            flowAtom.Atom.transform.position = Vector3.MoveTowards(flowAtom.Atom.transform.position, destination, maxDistance);
            float arrivalDistance = ConnectionAtomArrivalDistance();
            return (flowAtom.Atom.transform.position - destination).sqrMagnitude <= arrivalDistance * arrivalDistance;
        }

        private bool DeliverFlowAtom(FlowAtomState flowAtom, BattleMolecule target)
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

            foreach (FlowAtomState flowAtom in _flowAtoms)
            {
                if (flowAtom.Atom != null && flowAtom.Target == target && flowAtom.Phase != FlowPhase.ReturnToCore)
                    count++;
            }

            return count;
        }

        private bool CanStartFlowAtom(FreeAtom atom)
        {
            if (atom == null || atom.IsInConnectionFlow)
                return false;

            if (IsFlowAtom(atom))
                return false;

            return atom.Draggable == null || _dragService == null || !_dragService.IsReserved(atom.Draggable);
        }

        private bool IsFlowAtom(FreeAtom atom)
        {
            foreach (FlowAtomState flowAtom in _flowAtoms)
            {
                if (flowAtom.Atom == atom)
                    return true;
            }

            return false;
        }

        private void SetAllFlowAtomsReturning()
        {
            foreach (FlowAtomState flowAtom in _flowAtoms)
                SetFlowAtomReturning(flowAtom);
        }

        private static void SetFlowAtomReturning(FlowAtomState flowAtom)
        {
            ClearConnectionProgress(flowAtom);
            flowAtom.Target = null;
            flowAtom.Phase = FlowPhase.ReturnToCore;
        }

        private static void ReleaseFlowAtomControl(FlowAtomState flowAtom)
        {
            SetFlowAtomParent(flowAtom, flowAtom.Atom != null ? flowAtom.Atom.Owner : null);
            flowAtom.Atom?.EndConnectionFlow();
            ClearConnectionProgress(flowAtom);
            flowAtom.Target = null;
            flowAtom.Phase = FlowPhase.None;
            flowAtom.Radius = 0f;
        }

        private Vector3 ConnectionStartPosition(BattleMolecule target, FlowAtomState flowAtom)
        {
            if (target == null)
                return flowAtom.Atom != null ? flowAtom.Atom.transform.position : Vector3.zero;

            float angle = AngleFromCoreTo(target.transform.position);
            return CoreRimPosition(flowAtom, angle);
        }

        private Vector3 ConnectionDestinationPosition(BattleMolecule target, FlowAtomState flowAtom)
        {
            if (target == null)
                return flowAtom.Atom != null ? flowAtom.Atom.transform.position : Vector3.zero;

            return target.GetConnectionArrivalPosition(transform.position);
        }

        private static void EnsureConnectionProgress(FlowAtomState flowAtom, Vector3 start, Vector3 destination)
        {
            if (flowAtom.HasConnectionProgress || flowAtom.Atom == null)
                return;

            flowAtom.ConnectionProgress = ProjectSegmentProgress(
                start,
                destination,
                flowAtom.Atom.transform.position);
            flowAtom.HasConnectionProgress = true;
        }

        private static float ProjectSegmentProgress(Vector3 start, Vector3 destination, Vector3 position)
        {
            Vector2 segment = new(destination.x - start.x, destination.y - start.y);
            float lengthSquared = segment.sqrMagnitude;
            if (lengthSquared <= Mathf.Epsilon)
                return 1f;

            Vector2 offset = new(position.x - start.x, position.y - start.y);
            return Mathf.Clamp01(Vector2.Dot(offset, segment) / lengthSquared);
        }

        private static void ClearConnectionProgress(FlowAtomState flowAtom)
        {
            flowAtom.HasConnectionProgress = false;
            flowAtom.ConnectionProgress = 0f;
        }

        private static void SetFlowAtomParent(FlowAtomState flowAtom, Transform parent)
        {
            if (flowAtom.Atom == null || parent == null || flowAtom.Atom.transform.parent == parent)
                return;

            flowAtom.Atom.transform.SetParent(parent, true);
        }

        private bool IsAtomNearCoreRim(FlowAtomState flowAtom)
        {
            if (flowAtom.Atom == null)
                return false;

            float distance = Vector2.Distance(flowAtom.Atom.transform.position, transform.position);
            return Mathf.Abs(distance - flowAtom.Radius) <= ConnectionCoreRimSnapDistance();
        }

        private Vector3 CoreRimPosition(FlowAtomState flowAtom, float angle)
        {
            float z = flowAtom.Atom != null ? flowAtom.Atom.transform.position.z : transform.position.z;
            return OrbitMath.PositionOnCircle(transform.position, flowAtom.Radius, angle, z);
        }

        private float AngleFromCoreTo(Vector3 position)
        {
            return OrbitMath.AngleFromCenter(transform.position, position);
        }

        private float ConnectionAtomArrivalDistance()
        {
            return _config != null ? Mathf.Max(0f, _config.ConnectionAtomArrivalDistance) : 0f;
        }

        private float ConnectionAtomMinimumFlowRadius()
        {
            return _config != null ? Mathf.Max(0f, _config.ConnectionAtomMinimumFlowRadius) : 0f;
        }

        private float ConnectionCoreRimSnapDistance()
        {
            return _config != null ? Mathf.Max(0f, _config.ConnectionCoreRimSnapDistance) : 0f;
        }

        private float ConnectionCoreRimArrivalAngleDegrees()
        {
            return _config != null ? Mathf.Max(0f, _config.ConnectionCoreRimArrivalAngleDegrees) : 0f;
        }

        private sealed class FlowAtomState
        {
            public FreeAtom Atom;
            public BattleMolecule Target;
            public FlowPhase Phase;
            public float Radius;
            public bool HasConnectionProgress;
            public float ConnectionProgress;
        }

        private enum FlowPhase
        {
            None,
            MoveToRim,
            OrbitToConnection,
            Connection,
            ReturnToCore
        }
    }
}
