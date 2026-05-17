using System.Collections.Generic;
using _Project.Scripts.Gameplay.Level;
using _Project.Scripts.Infrastructure.AssetManagement;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules
{
    public class BattleMoleculeFactory : IBattleMoleculeFactory
    {
        private const string BATTLE_MOLECULE_PREFAB_PATH = "Gameplay/Units/BattleMolecule";
        private const string BATTLE_MOLECULES_CONTAINER_NAME = "BattleMolecules";

        private readonly List<BattleMolecule> _createdMolecules = new();

        [Inject] private IAssetProvider _assetProvider;
        [Inject] private IGameplayRuntimeHierarchy _runtimeHierarchy;
        [Inject] private IInstantiator _instantiator;

        public IReadOnlyList<BattleMolecule> CreatedMolecules => _createdMolecules;

        public BattleMolecule Create(Vector3 at, BattleMoleculeConfig config)
        {
            BattleMolecule prefab = _assetProvider.LoadAsset<BattleMolecule>(BATTLE_MOLECULE_PREFAB_PATH);

            if (prefab == null)
            {
                Debug.LogError($"BattleMolecule prefab is missing at Resources path '{BATTLE_MOLECULE_PREFAB_PATH}'.");
                return null;
            }

            BattleMolecule molecule = _instantiator.InstantiatePrefabForComponent<BattleMolecule>(
                prefab,
                at,
                Quaternion.identity,
                _runtimeHierarchy.GetOrCreateContainer(BATTLE_MOLECULES_CONTAINER_NAME));

            molecule.name = nameof(BattleMolecule);
            molecule.Configure(config);
            _createdMolecules.Add(molecule);

            return molecule;
        }

        public void Cleanup()
        {
            foreach (BattleMolecule molecule in _createdMolecules)
            {
                if (molecule != null)
                    Object.Destroy(molecule.gameObject);
            }

            _createdMolecules.Clear();
        }
    }
}
