using _Project.Scripts.Gameplay.Units.BattleMolecules.Components;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules
{
    [RequireComponent(typeof(OwnedAtoms))]
    [RequireComponent(typeof(BattleMoleculeCharge))]
    [RequireComponent(typeof(BattleMoleculeAtomReceiver))]
    [RequireComponent(typeof(BattleMoleculeAtomOrbit))]
    public class BattleMolecule : MonoBehaviour
    {
        [field: SerializeField] private OwnedAtoms OwnedAtoms { get; set; }
        [field: SerializeField] private BattleMoleculeCharge Charge { get; set; }
        [field: SerializeField] private BattleMoleculeAtomReceiver AtomReceiver { get; set; }
        [field: SerializeField] private BattleMoleculeAtomOrbit AtomOrbit { get; set; }

        private void Awake()
        {
            if (OwnedAtoms == null)
                OwnedAtoms = GetComponent<OwnedAtoms>();

            if (Charge == null)
                Charge = GetComponent<BattleMoleculeCharge>();

            if (AtomReceiver == null)
                AtomReceiver = GetComponent<BattleMoleculeAtomReceiver>();

            if (AtomOrbit == null)
                AtomOrbit = GetComponent<BattleMoleculeAtomOrbit>();
        }

        public void Configure(BattleMoleculeConfig config)
        {
            if (config == null)
                return;

            Charge.Configure(config.AtomsRequired);
            AtomReceiver.Configure(config.AtomsPosCircleRadius);
            AtomOrbit.Configure(config.DepositedAtomsOrbitDegreesPerSecond);
        }

        public void Tick(float deltaTime)
        {
            AtomOrbit.Tick(deltaTime);
        }
    }
}
