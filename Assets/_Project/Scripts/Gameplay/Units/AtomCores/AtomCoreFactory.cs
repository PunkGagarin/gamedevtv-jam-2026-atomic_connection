using System.Collections.Generic;
using _Project.Scripts.Gameplay.Level;
using _Project.Scripts.Infrastructure.AssetManagement;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Units.AtomCores
{
    public class AtomCoreFactory : IAtomCoreFactory
    {
        private const string ATOM_CORE_PREFAB_PATH = "Gameplay/Units/AtomCore";
        private const string ATOM_CORES_CONTAINER_NAME = "AtomCores";

        private readonly List<AtomCore> _createdCores = new();

        [Inject] private IAssetProvider _assetProvider;
        [Inject] private IGameplayRuntimeHierarchy _runtimeHierarchy;
        [Inject] private IInstantiator _instantiator;

        public AtomCore Create(Vector3 at)
        {
            AtomCore prefab = _assetProvider.LoadAsset<AtomCore>(ATOM_CORE_PREFAB_PATH);

            if (prefab == null)
            {
                Debug.LogError($"AtomCore prefab is missing at Resources path '{ATOM_CORE_PREFAB_PATH}'.");
                return null;
            }

            AtomCore core = _instantiator.InstantiatePrefabForComponent<AtomCore>(
                prefab,
                at,
                Quaternion.identity,
                _runtimeHierarchy.GetOrCreateContainer(ATOM_CORES_CONTAINER_NAME));

            core.name = nameof(AtomCore);
            _createdCores.Add(core);

            return core;
        }

        public void Cleanup()
        {
            foreach (AtomCore core in _createdCores)
            {
                if (core != null)
                    Object.Destroy(core.gameObject);
            }

            _createdCores.Clear();
        }
    }
}
