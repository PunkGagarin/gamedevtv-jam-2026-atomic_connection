using System;
using UnityEngine;
using Zenject;
using _Project.Scripts.Gameplay.Level;
using _Project.Scripts.Gameplay.Common.Pooling;
using _Project.Scripts.Infrastructure.AssetManagement;

namespace _Project.Scripts.Gameplay.Units.FreeAtoms
{
    public class FreeAtomFactory : PooledFactory<FreeAtom>, IFreeAtomFactory
    {
        private const string FREE_ATOM_PREFAB_PATH = "Gameplay/Units/FreeAtom";
        private const string FREE_ATOMS_POOL_CONTAINER_NAME = "FreeAtomsPool";

        [Inject] private IAssetProvider _assetProvider;
        [Inject] private IGameplayRuntimeHierarchy _runtimeHierarchy;
        [Inject] private IInstantiator _instantiator;

        public FreeAtom Create(Vector3 at, Transform parent)
        {
            FreeAtom freeAtom = GetFromPoolOrCreate();

            freeAtom.transform.SetParent(parent, true);
            freeAtom.transform.SetPositionAndRotation(at, Quaternion.identity);
            freeAtom.name = nameof(FreeAtom);
            freeAtom.PrepareForSpawn();

            return freeAtom;
        }

        protected override FreeAtom CreateNew()
        {
            FreeAtom prefab = _assetProvider.LoadAsset<FreeAtom>(FREE_ATOM_PREFAB_PATH);

            if (prefab == null)
                throw new InvalidOperationException($"FreeAtom prefab is missing at Resources path '{FREE_ATOM_PREFAB_PATH}'.");

            FreeAtom freeAtom = _instantiator.InstantiatePrefabForComponent<FreeAtom>(
                prefab,
                Vector3.zero,
                Quaternion.identity,
                _runtimeHierarchy.GetOrCreateContainer(FREE_ATOMS_POOL_CONTAINER_NAME));

            freeAtom.DespawnRequested += OnFreeAtomDespawnRequested;
            return freeAtom;
        }

        private void OnFreeAtomDespawnRequested(FreeAtom freeAtom)
        {
            ReturnToPool(freeAtom);
        }

        protected override void OnBeforeReturnToPool(FreeAtom freeAtom)
        {
            freeAtom.transform.SetParent(_runtimeHierarchy.GetOrCreateContainer(FREE_ATOMS_POOL_CONTAINER_NAME), false);
            freeAtom.PrepareForPool();
        }

        protected override void OnBeforeDestroy(FreeAtom freeAtom)
        {
            freeAtom.DespawnRequested -= OnFreeAtomDespawnRequested;
        }
    }
}
