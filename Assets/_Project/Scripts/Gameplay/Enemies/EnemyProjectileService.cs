using System.Collections.Generic;
using UnityEngine;
using Zenject;
using _Project.Scripts.Gameplay.Common.Time;
using _Project.Scripts.Gameplay.Enemies.Components;
using _Project.Scripts.Gameplay.Level;
using _Project.Scripts.Infrastructure.AssetManagement;

namespace _Project.Scripts.Gameplay.Enemies
{
    public class EnemyProjectileService : IEnemyProjectileService
    {
        private const string PROJECTILE_CONTAINER_NAME = "EnemyProjectiles";

        private readonly List<TrackedProjectile> _projectiles = new();

        [Inject] private IAssetProvider _assetProvider;
        [Inject] private IGameplayRuntimeHierarchy _runtimeHierarchy;
        [Inject] private IInstantiator _instantiator;
        [Inject] private ITimeService _time;

        public void Shoot(EnemyProjectileShot shot)
        {
            if (string.IsNullOrWhiteSpace(shot.PrefabResourcePath))
                return;

            EnemyProjectile prefab = _assetProvider.LoadAsset<EnemyProjectile>(shot.PrefabResourcePath);
            if (prefab == null)
            {
                Debug.LogError($"Enemy projectile prefab is missing at Resources path '{shot.PrefabResourcePath}'.");
                return;
            }

            EnemyProjectile projectile = _instantiator.InstantiatePrefabForComponent<EnemyProjectile>(
                prefab,
                shot.Position,
                Quaternion.identity,
                _runtimeHierarchy.GetOrCreateContainer(PROJECTILE_CONTAINER_NAME));

            if (projectile.Launch(shot.Target, shot.Direction, shot.Speed, shot.Damage, shot.Lifetime))
                _projectiles.Add(new TrackedProjectile(shot.Owner, projectile));
        }

        public void Update()
        {
            float deltaTime = _time.DeltaTime;

            for (int i = _projectiles.Count - 1; i >= 0; i--)
            {
                TrackedProjectile trackedProjectile = _projectiles[i];
                EnemyProjectile projectile = trackedProjectile.Projectile;

                if (projectile == null || !projectile.IsActive)
                {
                    _projectiles.RemoveAt(i);
                    continue;
                }

                if (!projectile.HasLiveTarget)
                {
                    DestroyTrackedProjectile(i);
                    continue;
                }

                if (deltaTime > 0f)
                {
                    projectile.TickLifetime(deltaTime);
                    projectile.TickMotion(deltaTime);
                }

                if (projectile.TryApplyImpact() || projectile.IsExpired)
                    DestroyTrackedProjectile(i);
            }
        }

        public void CleanupOwner(Component owner)
        {
            if (owner == null)
                return;

            for (int i = _projectiles.Count - 1; i >= 0; i--)
            {
                if (!ReferenceEquals(_projectiles[i].Owner, owner))
                    continue;

                DestroyTrackedProjectile(i);
            }
        }

        public void Cleanup()
        {
            foreach (TrackedProjectile trackedProjectile in _projectiles)
            {
                if (trackedProjectile.Projectile != null)
                    trackedProjectile.Projectile.DestroySelf();
            }

            _projectiles.Clear();
        }

        private void DestroyTrackedProjectile(int index)
        {
            EnemyProjectile projectile = _projectiles[index].Projectile;

            if (projectile != null)
                projectile.DestroySelf();

            _projectiles.RemoveAt(index);
        }

        private readonly struct TrackedProjectile
        {
            public TrackedProjectile(Component owner, EnemyProjectile projectile)
            {
                Owner = owner;
                Projectile = projectile;
            }

            public Component Owner { get; }
            public EnemyProjectile Projectile { get; }
        }
    }
}
