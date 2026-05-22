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

        private readonly List<IBattleMoleculeRuntimeBehavior> _runtimeBehaviors = new();
        private readonly List<FreeAtom> _coreAtoms = new();
        private bool _isStarted;
        private BattleMolecule _activeMolecule;
        private FreeAtom _flowAtom;
        private BattleMolecule _flowTarget;
        private FlowPhase _flowPhase;
        private float _flowRadius;

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

            if (_flowAtom != null && _dragService != null && _dragService.IsReserved(_flowAtom))
            {
                ReleaseFlowAtomControl();
                return;
            }

            BattleMolecule target = ActiveFeedTarget();

            if (target == null)
            {
                TickReturnToCore(core, deltaTime);
                return;
            }

            if (_flowAtom == null && (_dragService == null || !_dragService.IsDragActive))
                TryStartFlowAtom(core, target);

            if (_flowAtom == null)
                return;

            if (_flowTarget != target)
                RetargetFlowAtom(core, target);

            TickFlowAtom(core, target, deltaTime);
        }

        private void TryStartFlowAtom(AtomCore core, BattleMolecule target)
        {
            if (core == null || target == null || !target.CanReceiveConnectionAtom)
                return;

            if (!TryGetCoreAtom(core, out FreeAtom atom))
                return;

            _flowAtom = atom;
            _flowTarget = target;
            _flowRadius = Mathf.Max(MIN_FLOW_RADIUS, Vector2.Distance(atom.transform.position, core.transform.position));
            _flowPhase = FlowPhase.OrbitToConnection;
            atom.BeginConnectionFlow();
        }

        private void RetargetFlowAtom(AtomCore core, BattleMolecule target)
        {
            _flowTarget = target;
            _flowPhase = IsAtomNearCoreRim(core)
                ? FlowPhase.OrbitToConnection
                : FlowPhase.MoveToRim;
        }

        private void TickFlowAtom(AtomCore core, BattleMolecule target, float deltaTime)
        {
            if (_flowAtom == null)
                return;

            if (target == null || !target.CanReceiveConnectionAtom)
            {
                TickReturnToCore(core, deltaTime);
                return;
            }

            switch (_flowPhase)
            {
                case FlowPhase.MoveToRim:
                    if (MoveAtomToCoreRim(core, deltaTime))
                        _flowPhase = FlowPhase.OrbitToConnection;
                    break;
                case FlowPhase.OrbitToConnection:
                    if (MoveAtomAlongCoreRim(core, target, deltaTime))
                        _flowPhase = FlowPhase.Connection;
                    break;
                case FlowPhase.Connection:
                    if (MoveAtomToMolecule(target, deltaTime))
                        DeliverFlowAtom(target);
                    break;
                default:
                    _flowPhase = FlowPhase.MoveToRim;
                    break;
            }
        }

        private void TickReturnToCore(AtomCore core, float deltaTime)
        {
            if (_flowAtom == null)
                return;

            _flowTarget = null;
            _flowPhase = FlowPhase.ReturnToCore;

            if (!MoveAtomToCoreRim(core, deltaTime))
                return;

            _flowAtom.EndConnectionFlow();
            core.TakeGeneratedAtom(_flowAtom);
            _flowAtom = null;
            _flowPhase = FlowPhase.None;
        }

        private bool MoveAtomToCoreRim(AtomCore core, float deltaTime)
        {
            float angle = AngleFromCoreTo(core, _flowAtom.transform.position);
            return MoveFlowAtomTowards(CoreRimPosition(core, angle), _config.ConnectionAtomTravelSpeed * deltaTime);
        }

        private bool MoveAtomAlongCoreRim(AtomCore core, BattleMolecule target, float deltaTime)
        {
            float currentAngle = AngleFromCoreTo(core, _flowAtom.transform.position) * Mathf.Rad2Deg;
            float targetAngle = AngleFromCoreTo(core, target.transform.position) * Mathf.Rad2Deg;
            float angleStep = _config.ConnectionCoreRimDegreesPerSecond * deltaTime;
            float nextAngle = angleStep > 0f
                ? Mathf.MoveTowardsAngle(currentAngle, targetAngle, angleStep)
                : targetAngle;

            _flowAtom.transform.position = CoreRimPosition(core, nextAngle * Mathf.Deg2Rad);
            return Mathf.Abs(Mathf.DeltaAngle(nextAngle, targetAngle)) <= 0.5f;
        }

        private bool MoveAtomToMolecule(BattleMolecule target, float deltaTime)
        {
            return MoveFlowAtomTowards(target.transform.position, _config.ConnectionAtomTravelSpeed * deltaTime);
        }

        private bool MoveFlowAtomTowards(Vector3 destination, float maxDistance)
        {
            if (_flowAtom == null)
                return false;

            destination.z = _flowAtom.transform.position.z;
            _flowAtom.transform.position = Vector3.MoveTowards(_flowAtom.transform.position, destination, maxDistance);
            return (_flowAtom.transform.position - destination).sqrMagnitude <= ARRIVAL_DISTANCE * ARRIVAL_DISTANCE;
        }

        private void DeliverFlowAtom(BattleMolecule target)
        {
            if (_flowAtom == null)
                return;

            if (target != null && target.TryReceiveConnectionAtom(_flowAtom))
            {
                _flowAtom = null;
                _flowTarget = null;
                _flowPhase = FlowPhase.None;
                return;
            }

            _flowPhase = FlowPhase.ReturnToCore;
        }

        private bool TryGetCoreAtom(AtomCore core, out FreeAtom atom)
        {
            atom = null;

            if (core == null || core.OwnedAtoms == null)
                return false;

            core.OwnedAtoms.GetOwned(FreeAtomOwnerKind.Core, _coreAtoms);

            foreach (FreeAtom candidate in _coreAtoms)
            {
                if (candidate == null)
                    continue;

                if (_dragService != null && _dragService.IsReserved(candidate))
                    continue;

                atom = candidate;
                return true;
            }

            return false;
        }

        private void ReleaseFlowAtomControl()
        {
            _flowAtom?.EndConnectionFlow();
            _flowAtom = null;
            _flowTarget = null;
            _flowPhase = FlowPhase.None;
            _flowRadius = 0f;
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

        private bool IsAtomNearCoreRim(AtomCore core)
        {
            if (_flowAtom == null || core == null)
                return false;

            float distance = Vector2.Distance(_flowAtom.transform.position, core.transform.position);
            return Mathf.Abs(distance - _flowRadius) <= ARRIVAL_DISTANCE * 2f;
        }

        private Vector3 CoreRimPosition(AtomCore core, float angle)
        {
            Vector3 center = core.transform.position;
            Vector3 offset = new(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
            Vector3 position = center + offset * _flowRadius;
            position.z = _flowAtom != null ? _flowAtom.transform.position.z : position.z;
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
