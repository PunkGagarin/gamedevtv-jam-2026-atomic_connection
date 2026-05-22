using System;
using System.Collections.Generic;
using _Project.Scripts.Gameplay.Level;
using _Project.Scripts.Gameplay.Talents;
using _Project.Scripts.Infrastructure.AssetManagement;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules
{
    public class BattleMoleculeFactory : IBattleMoleculeFactory
    {
        private const string STINGER_MOLECULE_PREFAB_PATH = "Gameplay/Units/StingerMolecule";
        private const string MEMBRANE_MOLECULE_PREFAB_PATH = "Gameplay/Units/MembraneMolecule";
        private const string SWARM_MOLECULE_PREFAB_PATH = "Gameplay/Units/SwarmMolecule";
        private const string BATTLE_MOLECULES_CONTAINER_NAME = "BattleMolecules";

        private readonly List<BattleMolecule> _createdMolecules = new();

        [Inject] private IAssetProvider _assetProvider;
        [Inject] private IGameplayRuntimeHierarchy _runtimeHierarchy;
        [Inject] private IInstantiator _instantiator;
        [Inject] private ITalentService _talentService;
        public IReadOnlyList<BattleMolecule> CreatedMolecules => _createdMolecules;
        public event Action<BattleMolecule> MoleculeCreated;

        public BattleMolecule CreateStinger(Vector3 at, BattleMoleculeConfig config)
        {
            return Create(
                at,
                config,
                STINGER_MOLECULE_PREFAB_PATH,
                "StingerMolecule",
                AdjustedAtomsRequired(config.StingerMoleculeAtomsRequired),
                AdjustedAtomsRequired(config.StingerMoleculeBondAtomsRequired),
                BattleMoleculeKind.Stinger);
        }

        public BattleMolecule CreateMembrane(Vector3 at, BattleMoleculeConfig config)
        {
            int atomsRequired = config.MembraneMoleculeAtomsRequired -
                                Mathf.RoundToInt(_talentService.BonusOf(TalentType.MembraneMoleculeChargeReduction));

            return Create(
                at,
                config,
                MEMBRANE_MOLECULE_PREFAB_PATH,
                "MembraneMolecule",
                AdjustedAtomsRequired(atomsRequired),
                AdjustedAtomsRequired(config.MembraneMoleculeBondAtomsRequired),
                BattleMoleculeKind.Membrane);
        }

        public BattleMolecule CreateSwarm(Vector3 at, BattleMoleculeConfig config)
        {
            return Create(
                at,
                config,
                SWARM_MOLECULE_PREFAB_PATH,
                "SwarmMolecule",
                AdjustedAtomsRequired(config.SwarmMoleculeAtomsRequired),
                AdjustedAtomsRequired(config.SwarmMoleculeBondAtomsRequired),
                BattleMoleculeKind.Swarm);
        }

        private BattleMolecule Create(
            Vector3 at,
            BattleMoleculeConfig config,
            string prefabPath,
            string moleculeName,
            int atomsRequired,
            int bondAtomsRequired,
            BattleMoleculeKind kind)
        {
            BattleMolecule prefab = _assetProvider.LoadAsset<BattleMolecule>(prefabPath);

            if (prefab == null)
            {
                Debug.LogError($"BattleMolecule prefab is missing at Resources path '{prefabPath}'.");
                return null;
            }

            BattleMolecule molecule = _instantiator.InstantiatePrefabForComponent<BattleMolecule>(
                prefab,
                at,
                Quaternion.identity,
                _runtimeHierarchy.GetOrCreateContainer(BATTLE_MOLECULES_CONTAINER_NAME));

            molecule.name = moleculeName;
            molecule.Configure(config, atomsRequired, bondAtomsRequired, kind);
            _createdMolecules.Add(molecule);
            MoleculeCreated?.Invoke(molecule);

            return molecule;
        }

        private int AdjustedAtomsRequired(int atomsRequired)
        {
            return Mathf.Max(1, atomsRequired);
        }

        public void Cleanup()
        {
            foreach (BattleMolecule molecule in _createdMolecules)
            {
                if (molecule != null)
                    UnityEngine.Object.Destroy(molecule.gameObject);
            }

            _createdMolecules.Clear();
            MoleculeCreated = null;
        }
    }
}
