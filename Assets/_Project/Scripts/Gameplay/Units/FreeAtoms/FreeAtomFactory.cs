using System.Collections.Generic;
using _Project.Scripts.Infrastructure.AssetManagement;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Units.FreeAtoms
{
    public class FreeAtomFactory : IFreeAtomFactory
    {
        private const string FREE_ATOM_PREFAB_PATH = "Gameplay/Units/FreeAtom";

        private readonly List<FreeAtom> _createdFreeAtoms = new();

        [Inject] private IAssetProvider _assetProvider;
        [Inject] private IInstantiator _instantiator;

        public FreeAtom Create(Vector3 at)
        {
            FreeAtom prefab = _assetProvider.LoadAsset<FreeAtom>(FREE_ATOM_PREFAB_PATH);

            if (prefab == null)
            {
                Debug.LogError($"FreeAtom prefab is missing at Resources path '{FREE_ATOM_PREFAB_PATH}'.");
                return null;
            }

            FreeAtom freeAtom = _instantiator.InstantiatePrefabForComponent<FreeAtom>(
                prefab,
                at,
                Quaternion.identity,
                parentTransform: null);

            freeAtom.name = nameof(FreeAtom);
            _createdFreeAtoms.Add(freeAtom);

            return freeAtom;
        }

        public void Cleanup()
        {
            foreach (FreeAtom freeAtom in _createdFreeAtoms)
            {
                if (freeAtom != null)
                    Object.Destroy(freeAtom.gameObject);
            }

            _createdFreeAtoms.Clear();
        }
    }
}
