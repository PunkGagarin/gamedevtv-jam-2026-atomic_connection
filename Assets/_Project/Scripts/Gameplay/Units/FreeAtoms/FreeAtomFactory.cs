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

        public FreeAtom Create(Vector3 at, Transform parent)
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
                parent);

            freeAtom.name = nameof(FreeAtom);
            freeAtom.Destroyed += OnFreeAtomDestroyed;
            _createdFreeAtoms.Add(freeAtom);

            return freeAtom;
        }

        public void Cleanup()
        {
            for (int i = _createdFreeAtoms.Count - 1; i >= 0; i--)
            {
                FreeAtom freeAtom = _createdFreeAtoms[i];

                if (freeAtom != null)
                {
                    freeAtom.Destroyed -= OnFreeAtomDestroyed;
                    Object.Destroy(freeAtom.gameObject);
                }

                _createdFreeAtoms.RemoveAt(i);
            }
        }

        private void OnFreeAtomDestroyed(FreeAtom freeAtom)
        {
            if (freeAtom != null)
                freeAtom.Destroyed -= OnFreeAtomDestroyed;

            _createdFreeAtoms.Remove(freeAtom);
        }
    }
}
