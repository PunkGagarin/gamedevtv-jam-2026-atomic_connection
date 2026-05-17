using System;
using UnityEngine;
using Zenject;
using _Project.Scripts.Gameplay.Level;
using _Project.Scripts.Gameplay.Common.Pooling;
using _Project.Scripts.Infrastructure.AssetManagement;

namespace _Project.Scripts.Gameplay.Enemies
{
    public class EnemyFactory : PooledFactory<EnemyUnit>, IEnemyFactory
    {
        private const string ENEMY_UNIT_PREFAB_PATH = "Gameplay/Enemies/EnemyUnit";
        private const string ENEMIES_CONTAINER_NAME = "Enemies";
        private const string ENEMIES_POOL_CONTAINER_NAME = "EnemiesPool";

        [Inject] private IAssetProvider _assetProvider;
        [Inject] private IGameplayRuntimeHierarchy _runtimeHierarchy;
        [Inject] private IInstantiator _instantiator;

        public EnemyUnit Create(Vector3 at)
        {
            EnemyUnit enemy = GetFromPoolOrCreate();

            Transform parent = _runtimeHierarchy.GetOrCreateContainer(ENEMIES_CONTAINER_NAME);
            enemy.transform.SetParent(parent, true);
            enemy.transform.SetPositionAndRotation(at, Quaternion.identity);
            enemy.name = nameof(EnemyUnit);
            enemy.PrepareForSpawn();

            return enemy;
        }

        protected override EnemyUnit CreateNew()
        {
            EnemyUnit prefab = _assetProvider.LoadAsset<EnemyUnit>(ENEMY_UNIT_PREFAB_PATH);

            if (prefab == null)
                throw new InvalidOperationException($"EnemyUnit prefab is missing at Resources path '{ENEMY_UNIT_PREFAB_PATH}'.");

            EnemyUnit enemy = _instantiator.InstantiatePrefabForComponent<EnemyUnit>(
                prefab,
                Vector3.zero,
                Quaternion.identity,
                _runtimeHierarchy.GetOrCreateContainer(ENEMIES_CONTAINER_NAME));

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
