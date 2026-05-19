using System;
using _Project.Scripts.Gameplay.Units.BattleMolecules.Components;
using _Project.Scripts.Gameplay.Units.FreeAtoms;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules
{
    [RequireComponent(typeof(OwnedAtoms))]
    [RequireComponent(typeof(OwnedAtomOrbitLayout))]
    [RequireComponent(typeof(BattleMoleculeCharge))]
    [RequireComponent(typeof(BattleMoleculeAtomReceiver))]
    [RequireComponent(typeof(BattleMoleculeAtomOrbit))]
    [RequireComponent(typeof(BattleMoleculeShotQueue))]
    public class BattleMolecule : MonoBehaviour
    {
        [field: SerializeField] private OwnedAtoms OwnedAtoms { get; set; }
        [field: SerializeField] private OwnedAtomOrbitLayout AtomOrbitLayout { get; set; }
        [field: SerializeField] private BattleMoleculeCharge Charge { get; set; }
        [field: SerializeField] private BattleMoleculeAtomReceiver AtomReceiver { get; set; }
        [field: SerializeField] private BattleMoleculeAtomOrbit AtomOrbit { get; set; }
        [field: SerializeField] private BattleMoleculeShotQueue ShotQueue { get; set; }

        public event Action<Vector3, Vector3> ShotRequested
        {
            add => ShotQueue.ShotRequested += value;
            remove => ShotQueue.ShotRequested -= value;
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

            if (ShotQueue == null)
                ShotQueue = GetComponent<BattleMoleculeShotQueue>();
        }

        public void Configure(BattleMoleculeConfig config, int atomsRequired)
        {
            if (config == null)
                return;

            Charge.Configure(atomsRequired);
            AtomOrbitLayout?.ConfigureFixedRadius(FreeAtomOwnerKind.BattleMolecule, config.AtomsPosCircleRadius);
            AtomOrbit.Configure(config.DepositedAtomsOrbitDegreesPerSecond);
        }

        public void Tick(float deltaTime)
        {
            AtomOrbit.Tick(deltaTime);
        }
    }
}
