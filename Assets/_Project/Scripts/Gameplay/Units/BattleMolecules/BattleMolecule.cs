using System;
using _Project.Scripts.Gameplay.Units.BattleMolecules.Components;
using _Project.Scripts.Gameplay.Units.FreeAtoms;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules
{
    [RequireComponent(typeof(OwnedAtoms))]
    [RequireComponent(typeof(OwnedAtomOrbitLayout))]
    [RequireComponent(typeof(BattleMoleculeCharge))]
    [RequireComponent(typeof(BattleMoleculeBond))]
    [RequireComponent(typeof(BattleMoleculeAtomReceiver))]
    [RequireComponent(typeof(BattleMoleculeAtomOrbit))]
    [RequireComponent(typeof(BattleMoleculeCoreOrbit))]
    [RequireComponent(typeof(BattleMoleculeConnectionView))]
    [RequireComponent(typeof(BattleMoleculeShotQueue))]
    public class BattleMolecule : MonoBehaviour
    {
        [field: SerializeField] private OwnedAtoms OwnedAtoms { get; set; }
        [field: SerializeField] private OwnedAtomOrbitLayout AtomOrbitLayout { get; set; }
        [field: SerializeField] private BattleMoleculeCharge Charge { get; set; }
        [field: SerializeField] private BattleMoleculeBond Bond { get; set; }
        [field: SerializeField] private BattleMoleculeAtomReceiver AtomReceiver { get; set; }
        [field: SerializeField] private BattleMoleculeAtomOrbit AtomOrbit { get; set; }
        [field: SerializeField] private BattleMoleculeCoreOrbit CoreOrbit { get; set; }
        [field: SerializeField] private BattleMoleculeConnectionView ConnectionView { get; set; }
        [field: SerializeField] private BattleMoleculeShotQueue ShotQueue { get; set; }
        [field: SerializeField] public Collider2D CollisionCollider { get; private set; }

        public BattleMoleculeKind Kind { get; private set; } = BattleMoleculeKind.Stinger;
        public bool IsBonded => Bond != null && Bond.IsBonded;
        public bool CanReceiveBondAtom => Bond != null && Bond.CanReceiveAtom;
        public bool CanReceiveConnectionAtom => IsBonded
                                                && AtomReceiver != null
                                                && AtomReceiver.CanReceiveConnectionAtom;

        public event Action<BattleMolecule> Bonded;

        private void Awake()
        {
            if (OwnedAtoms == null)
                OwnedAtoms = GetComponent<OwnedAtoms>();

            if (AtomOrbitLayout == null)
                AtomOrbitLayout = GetComponent<OwnedAtomOrbitLayout>();

            if (Charge == null)
                Charge = GetComponent<BattleMoleculeCharge>();

            if (Bond == null)
                Bond = GetComponent<BattleMoleculeBond>();

            if (Bond == null)
                Bond = gameObject.AddComponent<BattleMoleculeBond>();

            if (AtomReceiver == null)
                AtomReceiver = GetComponent<BattleMoleculeAtomReceiver>();

            if (AtomOrbit == null)
                AtomOrbit = GetComponent<BattleMoleculeAtomOrbit>();

            if (CoreOrbit == null)
                CoreOrbit = GetComponent<BattleMoleculeCoreOrbit>();

            if (ConnectionView == null)
                ConnectionView = GetComponent<BattleMoleculeConnectionView>();

            if (ConnectionView == null)
                ConnectionView = gameObject.AddComponent<BattleMoleculeConnectionView>();

            if (ShotQueue == null)
                ShotQueue = GetComponent<BattleMoleculeShotQueue>();

            if (CollisionCollider == null)
                CollisionCollider = GetComponent<Collider2D>();
        }

        private void OnEnable()
        {
            if (Bond != null)
                Bond.Bonded += OnBonded;
        }

        private void OnDisable()
        {
            if (Bond != null)
                Bond.Bonded -= OnBonded;
        }

        public void Configure(BattleMoleculeConfig config, int atomsRequired, int bondAtomsRequired, BattleMoleculeKind kind)
        {
            if (config == null)
                return;

            Kind = kind;
            Bond.Configure(bondAtomsRequired);
            Charge.Configure(atomsRequired);
            AtomOrbitLayout?.ConfigureFixedRadius(FreeAtomOwnerKind.BattleMolecule, config.AtomsPosCircleRadius);
            AtomOrbit.Configure(config.DepositedAtomsOrbitDegreesPerSecond);
            ShotQueue?.Configure(config);
            RefreshConnectionVisibility();
        }

        public void ConfigureCoreOrbit(Transform coreTransform, BattleMoleculeConfig config)
        {
            if (config == null)
                return;

            CoreOrbit.Configure(coreTransform, config.CoreOrbitDegreesPerSecond);
            ConnectionView.Configure(coreTransform, config);
            RefreshConnectionVisibility();
        }

        public void Tick(float deltaTime)
        {
            AtomOrbit.Tick(deltaTime);
            CoreOrbit.Tick(deltaTime);
            RefreshConnectionVisibility();
            ConnectionView.Tick();
        }

        public bool TryAcceptBondAtom(FreeAtom atom)
        {
            return AtomReceiver != null && AtomReceiver.TryAcceptBondAtom(atom);
        }

        public bool TryReceiveConnectionAtom(FreeAtom atom)
        {
            return IsBonded && AtomReceiver != null && AtomReceiver.TryReceiveConnectionAtom(atom);
        }

        public bool ContainsPoint(Vector2 worldPosition)
        {
            return CollisionCollider != null && CollisionCollider.OverlapPoint(worldPosition);
        }

        public void SetActiveFeedVisual(bool isActive)
        {
            if (ConnectionView == null)
                return;

            ConnectionView.SetActiveConnection(isActive);
        }

        private void OnBonded()
        {
            RefreshConnectionVisibility();
            Bonded?.Invoke(this);
        }

        private void RefreshConnectionVisibility()
        {
            if (ConnectionView == null)
                return;

            ConnectionView.SetVisible(IsBonded);
        }
    }
}
