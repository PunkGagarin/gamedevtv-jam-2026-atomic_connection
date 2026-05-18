using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using _Project.Scripts.Gameplay.Level;
using _Project.Scripts.Gameplay.Common.Pooling;
using _Project.Scripts.Infrastructure.AssetManagement;

namespace _Project.Scripts.Gameplay.Enemies
{
    public class EnemyFactory : IEnemyFactory
    {
        private const string ENEMIES_CONTAINER_NAME = "Enemies";
        private const string ENEMIES_POOL_CONTAINER_NAME = "EnemiesPool";

        private readonly Dictionary<EnemyId, EnemyPool> _poolsByEnemyId = new();

        [Inject] private IAssetProvider _assetProvider;
        [Inject] private IGameplayRuntimeHierarchy _runtimeHierarchy;
        [Inject] private IInstantiator _instantiator;

        public EnemyUnit Create(EnemyDefinition definition, Vector3 at)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));

            return PoolFor(definition).Create(at);
        }

        public void Cleanup()
        {
            foreach (EnemyPool pool in _poolsByEnemyId.Values)
                pool.Cleanup();

            _poolsByEnemyId.Clear();
        }

        private EnemyPool PoolFor(EnemyDefinition definition)
        {
            if (_poolsByEnemyId.TryGetValue(definition.Id, out EnemyPool pool))
                return pool;

            pool = new EnemyPool(
                definition,
                _assetProvider,
                _runtimeHierarchy,
                _instantiator);

            _poolsByEnemyId.Add(definition.Id, pool);
            return pool;
        }

        private sealed class EnemyPool : PooledFactory<EnemyUnit>
        {
            private readonly EnemyDefinition _definition;
            private readonly IAssetProvider _assetProvider;
            private readonly IGameplayRuntimeHierarchy _runtimeHierarchy;
            private readonly IInstantiator _instantiator;

            public EnemyPool(
                EnemyDefinition definition,
                IAssetProvider assetProvider,
                IGameplayRuntimeHierarchy runtimeHierarchy,
                IInstantiator instantiator)
            {
                _definition = definition;
                _assetProvider = assetProvider;
                _runtimeHierarchy = runtimeHierarchy;
                _instantiator = instantiator;
            }

            public EnemyUnit Create(Vector3 at)
            {
                Transform parent = _runtimeHierarchy.GetOrCreateContainer(ENEMIES_CONTAINER_NAME);
                EnemyUnit enemy = GetFromPoolOrCreate(at, Quaternion.identity, parent);

                enemy.transform.SetParent(parent, true);
                enemy.transform.SetPositionAndRotation(at, Quaternion.identity);
                enemy.name = $"{_definition.Id}{nameof(EnemyUnit)}";
                enemy.Configure(_definition);
                enemy.PrepareForSpawn();

                return enemy;
            }

            protected override EnemyUnit CreateNew(Vector3 at, Quaternion rotation, Transform parent)
            {
                EnemyUnit prefab = _assetProvider.LoadAsset<EnemyUnit>(_definition.PrefabResourcePath);

                if (prefab == null)
                    throw new InvalidOperationException($"Enemy prefab is missing at Resources path '{_definition.PrefabResourcePath}'.");

                EnemyUnit enemy = _instantiator.InstantiatePrefabForComponent<EnemyUnit>(
                    prefab,
                    at,
                    rotation,
                    parent);

                enemy.Died += OnEnemyDied;
                return enemy;
            }

            private void OnEnemyDied(EnemyUnit enemy)
            {
                ReturnToPool(enemy);
            }

            protected override void OnBeforeReturnToPool(EnemyUnit enemy)
            {
                enemy.transform.SetParent(_runtimeHierarchy.GetOrCreateContainer(ENEMIES_POOL_CONTAINER_NAME), false);
                enemy.PrepareForPool();
            }

            protected override void OnBeforeDestroy(EnemyUnit enemy)
            {
                enemy.Died -= OnEnemyDied;
            }
        }
    }
}
