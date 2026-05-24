using System;
using _Project.Scripts.Gameplay.Talents;
using _Project.Scripts.Gameplay.Units.AtomCores;
using _Project.Scripts.Gameplay.Units.BattleMolecules.Components;
using _Project.Scripts.Gameplay.Units.FreeAtoms;
using UnityEngine;
using UnityEngine.Events;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules
{
    [RequireComponent(typeof(OwnedAtoms))]
    [RequireComponent(typeof(OwnedAtomOrbitLayout))]
    [RequireComponent(typeof(PointHitArea))]
    [RequireComponent(typeof(BattleMoleculeSetup))]
    [RequireComponent(typeof(BattleMoleculeCharge))]
    [RequireComponent(typeof(BattleMoleculeChargeConsumption))]
    [RequireComponent(typeof(BattleMoleculeBond))]
    [RequireComponent(typeof(BattleMoleculeBondEventRelay))]
    [RequireComponent(typeof(BattleMoleculeConnectionAtomReceiver))]
    [RequireComponent(typeof(BattleMoleculeAtomReceiver))]
    [RequireComponent(typeof(AtomOrbit))]
    [RequireComponent(typeof(BattleMoleculeCoreOrbit))]
    [RequireComponent(typeof(BattleMoleculeCoreLink))]
    [RequireComponent(typeof(BattleMoleculeConnectionVisual))]
    [RequireComponent(typeof(BattleMoleculeConnectionArrival))]
    [RequireComponent(typeof(BattleMoleculeShotQueue))]
    public class BattleMolecule : MonoBehaviour
    {
        [field: SerializeField] private BattleMoleculeSetup Setup { get; set; }
        [field: SerializeField] private BattleMoleculeBond Bond { get; set; }
        [field: SerializeField] private BattleMoleculeBondEventRelay BondEvents { get; set; }
        [field: SerializeField] private BattleMoleculeConnectionAtomReceiver ConnectionAtomReceiver { get; set; }
        [field: SerializeField] private AtomOrbit AtomOrbit { get; set; }
        [field: SerializeField] private BattleMoleculeCoreLink CoreLink { get; set; }
        [field: SerializeField] private BattleMoleculeConnectionVisual ConnectionVisual { get; set; }
        [field: SerializeField] private BattleMoleculeConnectionArrival ConnectionArrival { get; set; }
        [field: SerializeField] private PointHitArea HitArea { get; set; }
        [field: SerializeField] private BattleMoleculeAimLineVisual AimLineVisual { get; set; }
        [field: SerializeField] private MembraneMoleculeActivation MembraneActivation { get; set; }
        [field: SerializeField] private BattleMoleculeCharge Charge { get; set; }
        [field: SerializeField] private BattleMoleculeShotQueue ShotQueue { get; set; }

        [Header("Activation Events")]
        [field: SerializeField] private UnityEvent OnActivate { get; set; }
        [field: SerializeField] private UnityEvent OnDeactivate { get; set; }

        private bool _wasActiveFeedTarget;

        public bool IsBonded => Bond.IsBonded;
        public bool CanReceiveConnectionAtom => ConnectionAtomReceiver.CanReceiveAtom;
        public int ConnectionAtomsRemaining => ConnectionAtomReceiver.RemainingAtoms;
        public bool IsCharged => Charge != null && Charge.IsCharged;

        public event Action<BattleMolecule> Bonded
        {
            add => BondEvents.Bonded += value;
            remove => BondEvents.Bonded -= value;
        }

        public event Action<BattleMolecule> Charged;
        public event Action<BattleMoleculeShotRequest> ShotRequested;

        private void Awake()
        {
            Setup = GetComponent<BattleMoleculeSetup>();
            Bond = GetComponent<BattleMoleculeBond>();
            BondEvents = GetComponent<BattleMoleculeBondEventRelay>();
            ConnectionAtomReceiver = GetComponent<BattleMoleculeConnectionAtomReceiver>();
            AtomOrbit = GetComponent<AtomOrbit>();
            CoreLink = GetComponent<BattleMoleculeCoreLink>();
            ConnectionVisual = GetComponent<BattleMoleculeConnectionVisual>();
            ConnectionArrival = GetComponent<BattleMoleculeConnectionArrival>();
            HitArea = GetComponent<PointHitArea>();
            AimLineVisual = GetComponent<BattleMoleculeAimLineVisual>();
            MembraneActivation = GetComponent<MembraneMoleculeActivation>();
            Charge = GetComponent<BattleMoleculeCharge>();
            ShotQueue = GetComponent<BattleMoleculeShotQueue>();

            if (Charge != null)
                Charge.Charged += OnCharged;

            if (ShotQueue != null)
                ShotQueue.ShotRequested += OnShotRequested;
        }

        private void OnDestroy()
        {
            if (Charge != null)
                Charge.Charged -= OnCharged;

            if (ShotQueue != null)
                ShotQueue.ShotRequested -= OnShotRequested;
        }

        public void Configure(BattleMoleculeConfig config, int atomsRequired, int bondAtomsRequired)
        {
            Setup.Configure(config, atomsRequired, bondAtomsRequired);
        }

        public void ConfigureCoreOrbit(Transform coreTransform, BattleMoleculeConfig config)
        {
            CoreLink.Configure(coreTransform, config);
        }

        public void ConfigureCoreInteraction(AtomCore core, BattleMoleculeConfig config, ITalentService talentService)
        {
            MembraneActivation?.Configure(core, config, talentService);
        }

        public void Tick(float deltaTime)
        {
            AtomOrbit.Tick(deltaTime);
            CoreLink.Tick(deltaTime);
            AimLineVisual?.Tick(deltaTime);
            MembraneActivation?.Tick(deltaTime);
        }

        public bool TryReceiveConnectionAtom(FreeAtom atom)
        {
            return ConnectionAtomReceiver.TryReceive(atom);
        }

        public bool ContainsPoint(Vector2 worldPosition)
        {
            return HitArea.Contains(worldPosition);
        }

        public Vector3 GetConnectionArrivalPosition(Vector3 fromPosition)
        {
            return ConnectionArrival.PositionFrom(fromPosition);
        }

        public void SetActiveFeedVisual(bool isActive)
        {
            ConnectionVisual.SetActiveConnection(isActive);

            if (isActive && !_wasActiveFeedTarget)
                OnActivate?.Invoke();
            else if (!isActive && _wasActiveFeedTarget)
                OnDeactivate?.Invoke();

            _wasActiveFeedTarget = isActive;
        }

        private void OnCharged()
        {
            Charged?.Invoke(this);
        }

        private void OnShotRequested(BattleMoleculeShotRequest request)
        {
            ShotRequested?.Invoke(request);
        }
    }
}
