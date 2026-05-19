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
        private const string BATTLE_MOLECULE_PREFAB_PATH = "Gameplay/Units/BattleMolecule";
        private const string SHIELD_BATTLE_MOLECULE_PREFAB_PATH = "Gameplay/Units/ShieldBattleMolecule";
        private const string MASS_BATTLE_MOLECULE_PREFAB_PATH = "Gameplay/Units/MassBattleMolecule";
        private const string BATTLE_MOLECULES_CONTAINER_NAME = "BattleMolecules";

        private readonly List<BattleMolecule> _createdMolecules = new();

        [Inject] private IAssetProvider _assetProvider;
        [Inject] private IGameplayRuntimeHierarchy _runtimeHierarchy;
        [Inject] private IInstantiator _instantiator;
        [Inject] private ITalentService _talentService;
        public IReadOnlyList<BattleMolecule> CreatedMolecules => _createdMolecules;
        public event Action<BattleMolecule> MoleculeCreated;

        public BattleMolecule Create(Vector3 at, BattleMoleculeConfig config)
        {
            return Create(
                at,
                config,
                BATTLE_MOLECULE_PREFAB_PATH,
                nameof(BattleMolecule),
                AdjustedAtomsRequired(config.AtomsRequired),
                BattleMoleculeKind.Regular);
        }

        public BattleMolecule CreateShield(Vector3 at, BattleMoleculeConfig config)
        {
            int atomsRequired = config.ShieldMoleculeAtomsRequired -
                                Mathf.RoundToInt(_talentService.BonusOf(TalentType.ShieldChargeReduction));

            return Create(
                at,
                config,
                SHIELD_BATTLE_MOLECULE_PREFAB_PATH,
                "ShieldBattleMolecule",
                AdjustedAtomsRequired(atomsRequired),
                BattleMoleculeKind.Shield);
        }

        public BattleMolecule CreateMass(Vector3 at, BattleMoleculeConfig config)
        {
            return Create(
                at,
                config,
                MASS_BATTLE_MOLECULE_PREFAB_PATH,
                "MassBattleMolecule",
                AdjustedAtomsRequired(config.MassMoleculeAtomsRequired),
                BattleMoleculeKind.Mass);
        }

        private BattleMolecule Create(
            Vector3 at,
            BattleMoleculeConfig config,
            string prefabPath,
            string moleculeName,
            int atomsRequired,
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
            molecule.Configure(config, atomsRequired, kind);
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
