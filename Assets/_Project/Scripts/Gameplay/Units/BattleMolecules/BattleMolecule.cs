using System;
using _Project.Scripts.Gameplay.Units.BattleMolecules.Components;
using _Project.Scripts.Gameplay.Units.AtomCores;
using _Project.Scripts.Gameplay.Units.FreeAtoms;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules
{
    [RequireComponent(typeof(OwnedAtoms))]
    [RequireComponent(typeof(OwnedAtomOrbitLayout))]
    [RequireComponent(typeof(BattleMoleculeCharge))]
    [RequireComponent(typeof(BattleMoleculeAtomReceiver))]
    [RequireComponent(typeof(BattleMoleculeAtomOrbit))]
    [RequireComponent(typeof(BattleMoleculeCoreOrbit))]
    [RequireComponent(typeof(BattleMoleculeShotQueue))]
    public class BattleMolecule : MonoBehaviour
    {
        [field: SerializeField] private OwnedAtoms OwnedAtoms { get; set; }
        [field: SerializeField] private OwnedAtomOrbitLayout AtomOrbitLayout { get; set; }
        [field: SerializeField] private BattleMoleculeCharge Charge { get; set; }
        [field: SerializeField] private BattleMoleculeAtomReceiver AtomReceiver { get; set; }
        [field: SerializeField] private BattleMoleculeAtomOrbit AtomOrbit { get; set; }
        [field: SerializeField] private BattleMoleculeCoreOrbit CoreOrbit { get; set; }
        [field: SerializeField] private BattleMoleculeShotQueue ShotQueue { get; set; }
        [field: SerializeField] private ShieldMoleculeActivation ShieldActivation { get; set; }
        [field: SerializeField] public Collider2D CollisionCollider { get; private set; }

        public BattleMoleculeKind Kind { get; private set; } = BattleMoleculeKind.Regular;

        public event Action<BattleMoleculeShotRequest> ShotRequested
        {
            add
            {
                if (ShotQueue != null)
                    ShotQueue.ShotRequested += value;
            }
            remove
            {
                if (ShotQueue != null)
                    ShotQueue.ShotRequested -= value;
            }
        }

        private void Awake()
        {
            if (OwnedAtoms == null)
                OwnedAtoms = GetComponent<OwnedAtoms>();

            if (AtomOrbitLayout == null)
                AtomOrbitLayout = GetComponent<OwnedAtomOrbitLayout>();

            if (Charge == null)
                Charge = GetComponent<BattleMoleculeCharge>();

            if (AtomReceiver == null)
                AtomReceiver = GetComponent<BattleMoleculeAtomReceiver>();

            if (AtomOrbit == null)
                AtomOrbit = GetComponent<BattleMoleculeAtomOrbit>();

            if (CoreOrbit == null)
                CoreOrbit = GetComponent<BattleMoleculeCoreOrbit>();

            if (ShotQueue == null)
                ShotQueue = GetComponent<BattleMoleculeShotQueue>();

            if (ShieldActivation == null)
                ShieldActivation = GetComponent<ShieldMoleculeActivation>();

            if (CollisionCollider == null)
                CollisionCollider = GetComponent<Collider2D>();
        }

        public void Configure(BattleMoleculeConfig config, int atomsRequired, BattleMoleculeKind kind)
        {
            if (config == null)
                return;

            Kind = kind;
            Charge.Configure(atomsRequired);
            AtomOrbitLayout?.ConfigureFixedRadius(FreeAtomOwnerKind.BattleMolecule, config.AtomsPosCircleRadius);
            AtomOrbit.Configure(config.DepositedAtomsOrbitDegreesPerSecond);
            ShotQueue?.Configure(config);
        }

        public void ConfigureCoreOrbit(Transform coreTransform, BattleMoleculeConfig config)
        {
            if (config == null)
                return;

            CoreOrbit.Configure(coreTransform, config.CoreOrbitDegreesPerSecond);
        }

        public void Tick(float deltaTime)
        {
            AtomOrbit.Tick(deltaTime);
            ShieldActivation?.Tick();
        }

        public void FixedTick(float fixedDeltaTime)
        {
            CoreOrbit.FixedTick(fixedDeltaTime);
        }

        public bool TryAutoLoadAtom(FreeAtom atom)
        {
            return AtomReceiver != null && AtomReceiver.TryAcceptAtom(atom);
        }

        public void ConfigureShield(AtomCore core, float duration, float secondsLostPerDamage)
        {
            ShieldActivation?.Configure(core, duration, secondsLostPerDamage);
        }
    }
}
