using System.Collections.Generic;
using _Project.Scripts.Gameplay.Common.Time;
using _Project.Scripts.Gameplay.Drag;
using _Project.Scripts.Gameplay.Input.Service;
using _Project.Scripts.Gameplay.Talents;
using _Project.Scripts.Gameplay.Units.AtomCores;
using _Project.Scripts.Gameplay.Units.FreeAtoms;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules
{
    public class BattleMoleculeService : IBattleMoleculeService
    {
        private const float ARRIVAL_DISTANCE = 0.03f;
        private const float MIN_FLOW_RADIUS = 0.1f;
        private const string AB_TEST_PARALLEL_ATOM_FEED_KEY = "AtomicConnection.AbTest.ParallelAtomFeed";

        private readonly List<IBattleMoleculeRuntimeBehavior> _runtimeBehaviors = new();
        private readonly List<FreeAtom> _coreAtoms = new();
        private readonly List<FlowAtomState> _flowAtoms = new();
        private bool _isStarted;
        private BattleMolecule _activeMolecule;

        [Inject] private IBattleMoleculeFactory _battleMoleculeFactory;
        [Inject] private ITimeService _time;
        [Inject] private IAtomCoreService _atomCoreService;
        [Inject] private BattleMoleculeConfig _config;
        [Inject] private ITalentService _talentService;
        [Inject] private IDragService _dragService;
        [Inject] private IInputService _inputService;

        public void Start()
        {
            if (_isStarted)
                return;

            _isStarted = true;
            _battleMoleculeFactory.MoleculeCreated += TrackMolecule;

            foreach (BattleMolecule molecule in _battleMoleculeFactory.CreatedMolecules)
                TrackMolecule(molecule);
        }

        public void Update()
        {
            foreach (BattleMolecule molecule in _battleMoleculeFactory.CreatedMolecules)
            {
                if (molecule == null)
                    continue;

                molecule.Tick(_time.DeltaTime);
            }

            foreach (IBattleMoleculeRuntimeBehavior runtimeBehavior in _runtimeBehaviors)
            {
                if (runtimeBehavior == null)
                    continue;

                if (runtimeBehavior is Object unityObject && unityObject == null)
                    continue;

                runtimeBehavior.Tick(_time.DeltaTime);
            }

            TickMoleculeSelection();
            TickActiveAtomFlow(_time.DeltaTime);
        }

        public void FixedUpdate()
        {
        }

        public void Cleanup()
        {
            if (!_isStarted)
                return;

            _isStarted = false;
            _battleMoleculeFactory.MoleculeCreated -= TrackMolecule;

            foreach (BattleMolecule molecule in _battleMoleculeFactory.CreatedMolecules)
            {
                if (molecule == null)
                    continue;

                molecule.Bonded -= OnMoleculeBonded;
                molecule.SetActiveFeedVisual(false);
            }

            ReleaseFlowAtomControl();

            _runtimeBehaviors.Clear();
            _coreAtoms.Clear();
            _activeMolecule = null;
        }

        private void TrackMolecule(BattleMolecule molecule)
        {
            if (molecule == null)
                return;

            molecule.ConfigureCoreOrbit(_atomCoreService.CurrentCoreTransform, _config);

            MonoBehaviour[] components = molecule.GetComponents<MonoBehaviour>();
            BattleMoleculeRuntimeContext context = new(CurrentCore(), _config, _talentService);
            molecule.Bonded += OnMoleculeBonded;

            foreach (MonoBehaviour component in components)
            {
                if (component is IBattleMoleculeRuntimeBehavior runtimeBehavior)
                {
                    runtimeBehavior.Configure(context);
                    _runtimeBehaviors.Add(runtimeBehavior);
                }
            }

            if (molecule.IsBonded && !IsValidActiveMolecule(_activeMolecule))
                SetActiveMolecule(molecule);
        }

        private void OnMoleculeBonded(BattleMolecule molecule)
        {
            if (molecule == null)
                return;

            if (!IsValidActiveMolecule(_activeMolecule))
                SetActiveMolecule(molecule);
        }

        private void TickMoleculeSelection()
        {
            if (_inputService == null || !_inputService.GetLeftMouseButtonUp())
                return;

            if (_dragService != null && _dragService.DragWasStartedThisPress)
                return;

            Vector2 worldPosition = _inputService.GetWorldMousePosition();
            IReadOnlyList<BattleMolecule> molecules = _battleMoleculeFactory.CreatedMolecules;

            for (int i = molecules.Count - 1; i >= 0; i--)
            {
                BattleMolecule molecule = molecules[i];
                if (!IsValidActiveMolecule(molecule) || !molecule.ContainsPoint(worldPosition))
                    continue;

                SetActiveMolecule(molecule);
                return;
            }
        }

        private void TickActiveAtomFlow(float deltaTime)
        {
            AtomCore core = CurrentCore();

            if (core == null || core.OwnedAtoms == null || deltaTime <= 0f)
                return;

            BattleMolecule target = ActiveFeedTarget();

            if (target == null)
            {
                SetAllFlowAtomsReturning();
                TickFlowAtoms(core, deltaTime);
                return;
            }

            AssignFlowTargets(core, target);

            if (_dragService == null || !_dragService.IsDragActive)
                TryStartFlowAtoms(core, target);

            TickFlowAtoms(core, deltaTime);
        }

        private void TryStartFlowAtoms(AtomCore core, BattleMolecule target)
        {
            if (core == null || target == null || !target.CanReceiveConnectionAtom)
                return;

            int atomsToStart = FlowAtomStartCount(target);

            if (atomsToStart <= 0)
                return;

            core.OwnedAtoms.GetOwned(FreeAtomOwnerKind.Core, _coreAtoms);

            foreach (FreeAtom candidate in _coreAtoms)
            {
                if (atomsToStart <= 0)
                    return;

                if (!CanStartFlowAtom(candidate))
                    continue;

                StartFlowAtom(core, target, candidate);
                atomsToStart--;
            }
        }

        private void StartFlowAtom(AtomCore core, BattleMolecule target, FreeAtom atom)
        {
            FlowAtomState flowAtom = new()
            {
                Atom = atom,
                Target = target,
                Phase = FlowPhase.OrbitToConnection,
                Radius = Mathf.Max(MIN_FLOW_RADIUS, Vector2.Distance(atom.transform.position, core.transform.position))
            };

            atom.BeginConnectionFlow();
            _flowAtoms.Add(flowAtom);
        }

        private void AssignFlowTargets(AtomCore core, BattleMolecule target)
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
                        RetargetFlowAtom(core, target, flowAtom);

                    assignedAtoms++;
                    continue;
                }

                if (assignedAtoms >= atomsAllowed)
                {
                    SetFlowAtomReturning(flowAtom);
                    continue;
                }

                RetargetFlowAtom(core, target, flowAtom);
                assignedAtoms++;
            }
        }

        private void RetargetFlowAtom(AtomCore core, BattleMolecule target, FlowAtomState flowAtom)
        {
            flowAtom.Target = target;
            flowAtom.Phase = IsAtomNearCoreRim(core, flowAtom)
                ? FlowPhase.OrbitToConnection
                : FlowPhase.MoveToRim;
        }

        private void TickFlowAtoms(AtomCore core, float deltaTime)
        {
            for (int i = _flowAtoms.Count - 1; i >= 0; i--)
            {
                FlowAtomState flowAtom = _flowAtoms[i];

                if (flowAtom.Atom == null)
                {
                    _flowAtoms.RemoveAt(i);
                    continue;
                }

                if (_dragService != null && _dragService.IsDragging(flowAtom.Atom))
                {
                    ReleaseFlowAtomControl(flowAtom);
                    _flowAtoms.RemoveAt(i);
                    continue;
                }

                if (TickFlowAtom(core, flowAtom, deltaTime))
                    _flowAtoms.RemoveAt(i);
            }
        }

        private bool TickFlowAtom(AtomCore core, FlowAtomState flowAtom, float deltaTime)
        {
            if (flowAtom.Atom == null)
                return true;

            BattleMolecule target = flowAtom.Target;

            if (target == null || !target.CanReceiveConnectionAtom)
            {
                SetFlowAtomReturning(flowAtom);
                return TickReturnToCore(core, flowAtom, deltaTime);
            }

            switch (flowAtom.Phase)
            {
                case FlowPhase.MoveToRim:
                    if (MoveAtomToCoreRim(core, flowAtom, deltaTime))
                        flowAtom.Phase = FlowPhase.OrbitToConnection;
                    break;
                case FlowPhase.OrbitToConnection:
                    if (MoveAtomAlongCoreRim(core, target, flowAtom, deltaTime))
                        flowAtom.Phase = FlowPhase.Connection;
                    break;
                case FlowPhase.Connection:
                    if (MoveAtomToMolecule(target, flowAtom, deltaTime))
                        return DeliverFlowAtom(flowAtom, target);
                    break;
                case FlowPhase.ReturnToCore:
                    return TickReturnToCore(core, flowAtom, deltaTime);
                case FlowPhase.None:
                    flowAtom.Phase = FlowPhase.MoveToRim;
                    break;
                default:
                    flowAtom.Phase = FlowPhase.MoveToRim;
                    break;
            }

            return false;
        }

        private bool TickReturnToCore(AtomCore core, FlowAtomState flowAtom, float deltaTime)
        {
            if (flowAtom.Atom == null)
                return true;

            if (!MoveAtomToCoreRim(core, flowAtom, deltaTime))
                return false;

            flowAtom.Atom.EndConnectionFlow();
            core.TakeGeneratedAtom(flowAtom.Atom);
            return true;
        }

        private bool MoveAtomToCoreRim(AtomCore core, FlowAtomState flowAtom, float deltaTime)
        {
            float angle = AngleFromCoreTo(core, flowAtom.Atom.transform.position);
            return MoveFlowAtomTowards(flowAtom, CoreRimPosition(core, flowAtom, angle),
                _config.ConnectionAtomTravelSpeed * deltaTime);
        }

        private bool MoveAtomAlongCoreRim(AtomCore core, BattleMolecule target, FlowAtomState flowAtom, float deltaTime)
        {
            float currentAngle = AngleFromCoreTo(core, flowAtom.Atom.transform.position) * Mathf.Rad2Deg;
            float targetAngle = AngleFromCoreTo(core, target.transform.position) * Mathf.Rad2Deg;
            float angleStep = _config.ConnectionCoreRimDegreesPerSecond * deltaTime;
            float nextAngle = angleStep > 0f
                ? Mathf.MoveTowardsAngle(currentAngle, targetAngle, angleStep)
                : targetAngle;

            flowAtom.Atom.transform.position = CoreRimPosition(core, flowAtom, nextAngle * Mathf.Deg2Rad);
            return Mathf.Abs(Mathf.DeltaAngle(nextAngle, targetAngle)) <= 0.5f;
        }

        private bool MoveAtomToMolecule(BattleMolecule target, FlowAtomState flowAtom, float deltaTime)
        {
            return MoveFlowAtomTowards(flowAtom, target.transform.position, _config.ConnectionAtomTravelSpeed * deltaTime);
        }

        private bool MoveFlowAtomTowards(FlowAtomState flowAtom, Vector3 destination, float maxDistance)
        {
            if (flowAtom.Atom == null)
                return false;

            destination.z = flowAtom.Atom.transform.position.z;
            flowAtom.Atom.transform.position = Vector3.MoveTowards(flowAtom.Atom.transform.position, destination, maxDistance);
            return (flowAtom.Atom.transform.position - destination).sqrMagnitude <= ARRIVAL_DISTANCE * ARRIVAL_DISTANCE;
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

            if (PlayerPrefs.GetInt(AB_TEST_PARALLEL_ATOM_FEED_KEY, 0) != 0)
                return atomsRemaining;

            return _flowAtoms.Count == 0 ? 1 : 0;
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

            return _dragService == null || !_dragService.IsReserved(atom);
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
            flowAtom.Target = null;
            flowAtom.Phase = FlowPhase.ReturnToCore;
        }

        private void ReleaseFlowAtomControl()
        {
            foreach (FlowAtomState flowAtom in _flowAtoms)
                ReleaseFlowAtomControl(flowAtom);

            _flowAtoms.Clear();
        }

        private static void ReleaseFlowAtomControl(FlowAtomState flowAtom)
        {
            flowAtom.Atom?.EndConnectionFlow();
            flowAtom.Target = null;
            flowAtom.Phase = FlowPhase.None;
            flowAtom.Radius = 0f;
        }

        private BattleMolecule ActiveFeedTarget()
        {
            return IsValidActiveMolecule(_activeMolecule) && _activeMolecule.CanReceiveConnectionAtom
                ? _activeMolecule
                : null;
        }

        private bool IsValidActiveMolecule(BattleMolecule molecule)
        {
            return molecule != null && molecule.IsBonded;
        }

        private void SetActiveMolecule(BattleMolecule molecule)
        {
            if (!IsValidActiveMolecule(molecule))
                molecule = null;

            _activeMolecule = molecule;

            foreach (BattleMolecule createdMolecule in _battleMoleculeFactory.CreatedMolecules)
            {
                if (createdMolecule == null)
                    continue;

                createdMolecule.SetActiveFeedVisual(createdMolecule == _activeMolecule);
            }
        }

        private bool IsAtomNearCoreRim(AtomCore core, FlowAtomState flowAtom)
        {
            if (flowAtom.Atom == null || core == null)
                return false;

            float distance = Vector2.Distance(flowAtom.Atom.transform.position, core.transform.position);
            return Mathf.Abs(distance - flowAtom.Radius) <= ARRIVAL_DISTANCE * 2f;
        }

        private static Vector3 CoreRimPosition(AtomCore core, FlowAtomState flowAtom, float angle)
        {
            Vector3 center = core.transform.position;
            Vector3 offset = new(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
            Vector3 position = center + offset * flowAtom.Radius;
            position.z = flowAtom.Atom != null ? flowAtom.Atom.transform.position.z : position.z;
            return position;
        }

        private static float AngleFromCoreTo(AtomCore core, Vector3 position)
        {
            Vector3 offset = position - core.transform.position;
            return offset.sqrMagnitude > Mathf.Epsilon
                ? Mathf.Atan2(offset.y, offset.x)
                : 0f;
        }

        private AtomCore CurrentCore()
        {
            return _atomCoreService.CurrentCoreTransform != null
                ? _atomCoreService.CurrentCoreTransform.GetComponent<AtomCore>()
                : null;
        }

        private sealed class FlowAtomState
        {
            public FreeAtom Atom;
            public BattleMolecule Target;
            public FlowPhase Phase;
            public float Radius;
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
