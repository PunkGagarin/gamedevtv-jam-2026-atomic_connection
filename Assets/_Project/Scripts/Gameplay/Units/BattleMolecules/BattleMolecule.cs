using System;
using _Project.Scripts.Gameplay.Units.BattleMolecules.Components;
using _Project.Scripts.Gameplay.Units.FreeAtoms;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules
{
    [RequireComponent(typeof(OwnedAtoms))]
    [RequireComponent(typeof(OwnedAtomOrbitLayout))]
    [RequireComponent(typeof(ObjectRadius))]
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

        public bool IsBonded => Bond.IsBonded;
        public bool CanReceiveConnectionAtom => ConnectionAtomReceiver.CanReceiveAtom;
        public int ConnectionAtomsRemaining => ConnectionAtomReceiver.RemainingAtoms;

        public event Action<BattleMolecule> Bonded
        {
            add => BondEvents.Bonded += value;
            remove => BondEvents.Bonded -= value;
        }

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
        }

        public void Configure(BattleMoleculeConfig config, int atomsRequired, int bondAtomsRequired)
        {
            Setup.Configure(config, atomsRequired, bondAtomsRequired);
        }

        public void ConfigureCoreOrbit(Transform coreTransform, BattleMoleculeConfig config)
        {
            CoreLink.Configure(coreTransform, config);
        }

        public void Tick(float deltaTime)
        {
            AtomOrbit.Tick(deltaTime);
            CoreLink.Tick(deltaTime);
        }

        public bool TryReceiveConnectionAtom(FreeAtom atom)
        {
            return ConnectionAtomReceiver.TryReceive(atom);
        }

        public bool ContainsPoint(Vector2 worldPosition)
        {
            return HitArea.Contains(worldPosition);
        }

        public Vector3 GetConnectionArrivalPosition(Vector3 fromPosition, float incomingAtomRadius)
        {
            return ConnectionArrival.PositionFrom(fromPosition, incomingAtomRadius);
        }

        public bool IsConnectionArrivalReached(Vector3 fromPosition, float incomingAtomRadius, float tolerance)
        {
            return ConnectionArrival.IsReached(fromPosition, incomingAtomRadius, tolerance);
        }

        public void SetActiveFeedVisual(bool isActive)
        {
            ConnectionVisual.SetActiveConnection(isActive);
        }
    }
}
