using _Project.Scripts.Gameplay.Common.Movement;
using _Project.Scripts.Gameplay.Talents;
using _Project.Scripts.Gameplay.Units;
using _Project.Scripts.Gameplay.Units.BattleMolecules;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Units.AtomCores.Components
{
    [DisallowMultipleComponent]
    public class AtomCoreConnectionAtomMotion : MonoBehaviour
    {
        [Inject] private BattleMoleculeConfig _config;
        [Inject] private AtomCoreConfig _coreConfig;
        [Inject] private ITalentService _talentService;

        internal void InitializeStartState(ConnectionAtomFlowState flowAtom)
        {
            if (flowAtom?.Atom == null)
                return;

            flowAtom.Radius = flowAtom.Source != null
                ? ConnectionAtomCoreFlowRadius()
                : Mathf.Max(
                    ConnectionAtomMinimumFlowRadius(),
                    Vector2.Distance(flowAtom.Atom.transform.position, transform.position));
            flowAtom.ClearConnectionProgress();
        }

        internal ConnectionAtomFlowPhase RetargetPhase(ConnectionAtomFlowState flowAtom)
        {
            return IsAtomNearCoreRim(flowAtom)
                ? ConnectionAtomFlowPhase.OrbitToConnection
                : ConnectionAtomFlowPhase.MoveToRim;
        }

        internal ConnectionAtomFlowMotionResult Tick(ConnectionAtomFlowState flowAtom, float deltaTime)
        {
            if (flowAtom?.Atom == null)
                return ConnectionAtomFlowMotionResult.Remove;

            switch (flowAtom.Phase)
            {
                case ConnectionAtomFlowPhase.MoveToSourceConnection:
                    if (MoveAtomToSourceConnection(flowAtom, deltaTime))
                    {
                        flowAtom.Phase = ConnectionAtomFlowPhase.SourceConnectionToCore;
                        flowAtom.ClearConnectionProgress();
                    }
                    break;
                case ConnectionAtomFlowPhase.SourceConnectionToCore:
                    if (MoveAtomFromSourceConnectionToCore(flowAtom, deltaTime))
                    {
                        flowAtom.Source = null;
                        flowAtom.Phase = ConnectionAtomFlowPhase.OrbitToConnection;
                        flowAtom.ClearConnectionProgress();
                    }
                    break;
                case ConnectionAtomFlowPhase.MoveToRim:
                    if (MoveAtomToCoreRim(flowAtom, deltaTime, true))
                        flowAtom.Phase = ConnectionAtomFlowPhase.OrbitToConnection;
                    break;
                case ConnectionAtomFlowPhase.OrbitToConnection:
                    if (MoveAtomAlongCoreRim(flowAtom.Target, flowAtom, deltaTime))
                    {
                        flowAtom.Phase = ConnectionAtomFlowPhase.Connection;
                        flowAtom.ClearConnectionProgress();
                    }
                    break;
                case ConnectionAtomFlowPhase.Connection:
                    if (MoveAtomToMolecule(flowAtom.Target, flowAtom, deltaTime))
                        return ConnectionAtomFlowMotionResult.DeliverToTarget;
                    break;
                case ConnectionAtomFlowPhase.ReturnToCore:
                    if (MoveAtomToCoreRim(flowAtom, deltaTime, false))
                        return ConnectionAtomFlowMotionResult.ReturnToCore;
                    break;
                case ConnectionAtomFlowPhase.None:
                    flowAtom.Phase = ConnectionAtomFlowPhase.MoveToRim;
                    break;
                default:
                    flowAtom.Phase = ConnectionAtomFlowPhase.MoveToRim;
                    break;
            }

            return ConnectionAtomFlowMotionResult.Continue;
        }

        private bool MoveAtomToSourceConnection(ConnectionAtomFlowState flowAtom, float deltaTime)
        {
            if (flowAtom.Source == null)
                return MoveAtomToCoreRim(flowAtom, deltaTime, true);

            SetFlowAtomParent(flowAtom, transform);
            return MoveFlowAtomTowards(flowAtom, SourceConnectionPosition(flowAtom.Source), ConnectionAtomTravelSpeed() * deltaTime);
        }

        private bool MoveAtomFromSourceConnectionToCore(ConnectionAtomFlowState flowAtom, float deltaTime)
        {
            if (flowAtom.Source == null)
                return MoveAtomToCoreRim(flowAtom, deltaTime, true);

            SetFlowAtomParent(flowAtom, transform);

            Vector3 start = SourceConnectionPosition(flowAtom.Source);
            Vector3 destination = SourceCoreRimPosition(flowAtom);
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

        private bool MoveAtomToCoreRim(ConnectionAtomFlowState flowAtom, float deltaTime, bool useTalentSpeed)
        {
            SetFlowAtomParent(flowAtom, transform);

            float angle = AngleFromCoreTo(flowAtom.Atom.transform.position);
            float speed = useTalentSpeed ? ConnectionAtomTravelSpeed() : BaseConnectionAtomTravelSpeed();
            return MoveFlowAtomTowards(flowAtom, CoreRimPosition(flowAtom, angle), speed * deltaTime);
        }

        private bool MoveAtomAlongCoreRim(BattleMolecule target, ConnectionAtomFlowState flowAtom, float deltaTime)
        {
            if (target == null)
                return false;

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

        private bool MoveAtomToMolecule(BattleMolecule target, ConnectionAtomFlowState flowAtom, float deltaTime)
        {
            if (target == null)
                return false;

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
            float bonus = _talentService != null ? _talentService.BonusOf(TalentEffectType.ConnectionAtomSpeed) : 0f;
            return Mathf.Max(0f, 1f + bonus);
        }

        private float BaseConnectionAtomTravelSpeed()
        {
            return _config != null ? Mathf.Max(0f, _config.ConnectionAtomTravelSpeed) : 0f;
        }

        private float ConnectionAtomTravelSpeed()
        {
            return BaseConnectionAtomTravelSpeed() * ConnectionAtomSpeedMultiplier();
        }

        private float ConnectionCoreRimDegreesPerSecond()
        {
            float degreesPerSecond = _config != null ? Mathf.Max(0f, _config.ConnectionCoreRimDegreesPerSecond) : 0f;
            return degreesPerSecond * ConnectionAtomSpeedMultiplier();
        }

        private bool MoveFlowAtomTowards(ConnectionAtomFlowState flowAtom, Vector3 destination, float maxDistance)
        {
            if (flowAtom.Atom == null)
                return false;

            destination.z = flowAtom.Atom.transform.position.z;
            flowAtom.Atom.transform.position = Vector3.MoveTowards(flowAtom.Atom.transform.position, destination, maxDistance);
            float arrivalDistance = ConnectionAtomArrivalDistance();
            return (flowAtom.Atom.transform.position - destination).sqrMagnitude <= arrivalDistance * arrivalDistance;
        }

        private Vector3 ConnectionStartPosition(BattleMolecule target, ConnectionAtomFlowState flowAtom)
        {
            if (target == null)
                return flowAtom.Atom != null ? flowAtom.Atom.transform.position : Vector3.zero;

            float angle = AngleFromCoreTo(target.transform.position);
            return CoreRimPosition(flowAtom, angle);
        }

        private Vector3 ConnectionDestinationPosition(BattleMolecule target, ConnectionAtomFlowState flowAtom)
        {
            if (target == null)
                return flowAtom.Atom != null ? flowAtom.Atom.transform.position : Vector3.zero;

            return target.GetConnectionArrivalPosition(transform.position);
        }

        private Vector3 SourceConnectionPosition(BattleMolecule source)
        {
            return source != null
                ? source.GetConnectionArrivalPosition(transform.position)
                : transform.position;
        }

        private Vector3 SourceCoreRimPosition(ConnectionAtomFlowState flowAtom)
        {
            if (flowAtom.Source == null)
                return flowAtom.Atom != null ? flowAtom.Atom.transform.position : transform.position;

            float angle = AngleFromCoreTo(flowAtom.Source.transform.position);
            return CoreRimPosition(flowAtom, angle);
        }

        private static void EnsureConnectionProgress(ConnectionAtomFlowState flowAtom, Vector3 start, Vector3 destination)
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

        private static void SetFlowAtomParent(ConnectionAtomFlowState flowAtom, Transform parent)
        {
            if (flowAtom.Atom == null || parent == null || flowAtom.Atom.transform.parent == parent)
                return;

            flowAtom.Atom.transform.SetParent(parent, true);
        }

        private bool IsAtomNearCoreRim(ConnectionAtomFlowState flowAtom)
        {
            if (flowAtom?.Atom == null)
                return false;

            float distance = Vector2.Distance(flowAtom.Atom.transform.position, transform.position);
            return Mathf.Abs(distance - flowAtom.Radius) <= ConnectionCoreRimSnapDistance();
        }

        private Vector3 CoreRimPosition(ConnectionAtomFlowState flowAtom, float angle)
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

        private float ConnectionAtomCoreFlowRadius()
        {
            float coreOrbitRadius = _coreConfig != null ? Mathf.Max(0f, _coreConfig.FreeAtomOrbitRadius) : 0f;
            return Mathf.Max(ConnectionAtomMinimumFlowRadius(), coreOrbitRadius);
        }

        private float ConnectionCoreRimSnapDistance()
        {
            return _config != null ? Mathf.Max(0f, _config.ConnectionCoreRimSnapDistance) : 0f;
        }

        private float ConnectionCoreRimArrivalAngleDegrees()
        {
            return _config != null ? Mathf.Max(0f, _config.ConnectionCoreRimArrivalAngleDegrees) : 0f;
        }
    }
}
