using _Project.Scripts.Gameplay.Units;
using _Project.Scripts.Gameplay.Units.BattleMolecules;
using _Project.Scripts.Gameplay.Units.FreeAtoms;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(OwnedAtomReceiver))]
    [RequireComponent(typeof(BattleMoleculeBond))]
    [RequireComponent(typeof(BattleMoleculeCharge))]
    [RequireComponent(typeof(OwnedAtomOrbitLayout))]
    [RequireComponent(typeof(AtomOrbit))]
    [RequireComponent(typeof(BattleMoleculeShotQueue))]
    [RequireComponent(typeof(BattleMoleculeConnectionArrival))]
    [RequireComponent(typeof(BattleMoleculeConnectionVisual))]
    public class BattleMoleculeSetup : MonoBehaviour
    {
        [field: SerializeField] private OwnedAtomReceiver AtomReceiver { get; set; }
        [field: SerializeField] private BattleMoleculeBond Bond { get; set; }
        [field: SerializeField] private BattleMoleculeCharge Charge { get; set; }
        [field: SerializeField] private OwnedAtomOrbitLayout AtomOrbitLayout { get; set; }
        [field: SerializeField] private AtomOrbit AtomOrbit { get; set; }
        [field: SerializeField] private BattleMoleculeShotQueue ShotQueue { get; set; }
        [field: SerializeField] private BattleMoleculeConnectionArrival ConnectionArrival { get; set; }
        [field: SerializeField] private BattleMoleculeConnectionVisual ConnectionVisual { get; set; }

        private void Awake()
        {
            if (AtomReceiver == null)
                AtomReceiver = GetComponent<OwnedAtomReceiver>();

            if (Bond == null)
                Bond = GetComponent<BattleMoleculeBond>();

            if (Charge == null)
                Charge = GetComponent<BattleMoleculeCharge>();

            if (AtomOrbitLayout == null)
                AtomOrbitLayout = GetComponent<OwnedAtomOrbitLayout>();

            if (AtomOrbit == null)
                AtomOrbit = GetComponent<AtomOrbit>();

            if (ShotQueue == null)
                ShotQueue = GetComponent<BattleMoleculeShotQueue>();

            if (ConnectionArrival == null)
                ConnectionArrival = GetComponent<BattleMoleculeConnectionArrival>();

            if (ConnectionVisual == null)
                ConnectionVisual = GetComponent<BattleMoleculeConnectionVisual>();
        }

        public void Configure(BattleMoleculeConfig config, int atomsRequired, int bondAtomsRequired)
        {
            if (config == null)
                return;

            AtomReceiver?.Configure(FreeAtomOwnerKind.BattleMolecule);
            Bond?.Configure(bondAtomsRequired);
            Charge?.Configure(atomsRequired);
            ConnectionArrival?.Configure(config.AtomsPosCircleRadius);
            AtomOrbitLayout?.ConfigureFixedRadius(FreeAtomOwnerKind.BattleMolecule, config.AtomsPosCircleRadius);
            AtomOrbit?.Configure(config.DepositedAtomsOrbitDegreesPerSecond);
            ShotQueue?.Configure(config);
            ConnectionVisual?.RefreshVisibility();
        }
    }
}
