using _Project.Scripts.Gameplay.Level;
using _Project.Scripts.Gameplay.Talents;
using _Project.Scripts.Infrastructure.AssetManagement;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules
{
    public class BattleMoleculeFactory : IBattleMoleculeFactory
    {
        private const string NEEDLE_MOLECULE_PREFAB_PATH = "Gameplay/Units/NeedleMolecule";
        private const string STINGER_MOLECULE_PREFAB_PATH = "Gameplay/Units/StingerMolecule";
        private const string MEMBRANE_MOLECULE_PREFAB_PATH = "Gameplay/Units/MembraneMolecule";
        private const string SWARM_MOLECULE_PREFAB_PATH = "Gameplay/Units/SwarmMolecule";
        private const string BATTLE_MOLECULES_CONTAINER_NAME = "BattleMolecules";

        [Inject] private IAssetProvider _assetProvider;
        [Inject] private IGameplayRuntimeHierarchy _runtimeHierarchy;
        [Inject] private IInstantiator _instantiator;
        [Inject] private ITalentService _talentService;

        public BattleMolecule CreateNeedle(Vector3 at, BattleMoleculeConfig config)
        {
            int atomsRequired = config.NeedleMoleculeAtomsRequired -
                                Mathf.RoundToInt(_talentService.BonusOf(TalentEffectType.NeedleMoleculeChargeReduction));

            return Create(
                at,
                config,
                NEEDLE_MOLECULE_PREFAB_PATH,
                "NeedleMolecule",
                AdjustedAtomsRequired(atomsRequired),
                AdjustedAtomsRequired(config.NeedleMoleculeBondAtomsRequired));
        }

        public BattleMolecule CreateStinger(Vector3 at, BattleMoleculeConfig config)
        {
            int atomsRequired = config.StingerMoleculeAtomsRequired -
                                Mathf.RoundToInt(_talentService.BonusOf(TalentEffectType.StingerMoleculeChargeReduction));

            return Create(
                at,
                config,
                STINGER_MOLECULE_PREFAB_PATH,
                "StingerMolecule",
                AdjustedAtomsRequired(atomsRequired),
                AdjustedAtomsRequired(config.StingerMoleculeBondAtomsRequired));
        }

        public BattleMolecule CreateMembrane(Vector3 at, BattleMoleculeConfig config)
        {
            int atomsRequired = config.MembraneMoleculeAtomsRequired -
                                Mathf.RoundToInt(_talentService.BonusOf(TalentEffectType.MembraneMoleculeChargeReduction));

            return Create(
                at,
                config,
                MEMBRANE_MOLECULE_PREFAB_PATH,
                "MembraneMolecule",
                AdjustedAtomsRequired(atomsRequired),
                AdjustedAtomsRequired(config.MembraneMoleculeBondAtomsRequired));
        }

        public BattleMolecule CreateSwarm(Vector3 at, BattleMoleculeConfig config)
        {
            return Create(
                at,
                config,
                SWARM_MOLECULE_PREFAB_PATH,
                "SwarmMolecule",
                AdjustedAtomsRequired(config.SwarmMoleculeAtomsRequired),
                AdjustedAtomsRequired(config.SwarmMoleculeBondAtomsRequired));
        }

        private BattleMolecule Create(
            Vector3 at,
            BattleMoleculeConfig config,
            string prefabPath,
            string moleculeName,
            int atomsRequired,
            int bondAtomsRequired)
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
            molecule.Configure(config, atomsRequired, bondAtomsRequired);

            return molecule;
        }

        private int AdjustedAtomsRequired(int atomsRequired)
        {
            return Mathf.Max(1, atomsRequired);
        }
    }
}
