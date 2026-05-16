using System.Collections.Generic;
using _Project.Scripts.Infrastructure.AssetManagement;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Units.Atom
{
    public class AtomFactory : IAtomFactory
    {
        private const string ATOM_PREFAB_PATH = "Gameplay/Units/Atom";

        private readonly List<Atom> _createdAtoms = new();

        [Inject] private IAssetProvider _assetProvider;
        [Inject] private IInstantiator _instantiator;

        public Atom Create(Vector3 at)
        {
            Atom prefab = _assetProvider.LoadAsset<Atom>(ATOM_PREFAB_PATH);

            if (prefab == null)
            {
                Debug.LogError($"Atom prefab is missing at Resources path '{ATOM_PREFAB_PATH}'.");
                return null;
            }

            Atom atom = _instantiator.InstantiatePrefabForComponent<Atom>(
                prefab,
                at,
                Quaternion.identity,
                parentTransform: null);

            atom.name = nameof(Atom);
            _createdAtoms.Add(atom);

            return atom;
        }

        public void Cleanup()
        {
            foreach (Atom atom in _createdAtoms)
            {
                if (atom != null)
                    Object.Destroy(atom.gameObject);
            }

            _createdAtoms.Clear();
        }
    }
}
