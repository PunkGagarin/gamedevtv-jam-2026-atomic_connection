using System.Collections.Generic;
using _Project.Scripts.Gameplay.Cameras.Provider;
using _Project.Scripts.Gameplay.Common.Movement;
using _Project.Scripts.Gameplay.Common.Random;
using _Project.Scripts.Gameplay.Units.AtomCores;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Enemies
{
    public class EnemySpawner : IEnemySpawner
    {
        [Inject] private IEnemyFactory _enemyFactory;
        [Inject] private ICameraProvider _cameraProvider;
        [Inject] private IRandomService _random;
        [Inject] private EnemySpawnerConfig _config;

        public IReadOnlyList<EnemyUnit> SpawnGroup(
            EnemyDefinition definition,
            int maxHealth,
            int coreCollisionDamage,
            Transform target,
            float offscreenPadding,
            int count)
        {
            int spawnCount = Mathf.Max(1, count);
            List<EnemyUnit> enemies = new(spawnCount);

            if (!TryGetOffscreenSpawnPositions(target, offscreenPadding, spawnCount, out List<Vector3> spawnPositions))
                return enemies;

            float groupMovementSign = _random.Range(0, 2) == 0 ? -1f : 1f;

            foreach (Vector3 spawnPosition in spawnPositions)
            {
                EnemyUnit enemy = _enemyFactory.Create(definition, maxHealth, coreCollisionDamage, spawnPosition);
                enemy.MoveTo(target, definition.MoveSpeed, groupMovementSign);
                enemy.CollideWithCore(target != null && target.TryGetComponent(out AtomCore core) ? core : null);
                enemies.Add(enemy);
            }

            return enemies;
        }

        private bool TryGetOffscreenSpawnPositions(Transform target, float offscreenSpawnPadding, int count, out List<Vector3> spawnPositions)
        {
            spawnPositions = new List<Vector3>();

            Camera camera = _cameraProvider.MainCamera;
            if (camera == null || target == null)
                return false;

            float depth = Mathf.Abs(camera.transform.position.z - target.position.z);
            Vector3 bottomLeft = camera.ViewportToWorldPoint(new Vector3(0, 0, depth));
            Vector3 topRight = camera.ViewportToWorldPoint(new Vector3(1, 1, depth));
            float padding = Mathf.Max(0f, offscreenSpawnPadding);
            float centerAngle = _random.Range(0f, Mathf.PI * 2f);
            float groupSpacing = Mathf.Max(0f, _config.GroupSpawnSpacing);
            float groupJitter = Mathf.Max(0f, _config.GroupSpawnJitter);
            float minimumSpawnRadius = MinimumSpawnRadius(bottomLeft, topRight, padding);
            float groupRadius = Mathf.Max(
                SpawnRadiusOnScreenRay(target.position, padding, bottomLeft, topRight, centerAngle),
                minimumSpawnRadius);
            float angularSpacing = groupRadius > Mathf.Epsilon ? groupSpacing / groupRadius : 0f;
            float angularJitter = groupRadius > Mathf.Epsilon ? groupJitter / groupRadius : 0f;

            for (int i = 0; i < count; i++)
            {
                float offset = count <= 1 ? 0f : (i - (count - 1) * 0.5f) * angularSpacing;
                float jitter = count <= 1 ? 0f : _random.Range(-angularJitter, angularJitter);
                float angle = centerAngle + offset + jitter;
                float spawnRadius = Mathf.Max(
                    SpawnRadiusOnScreenRay(target.position, padding, bottomLeft, topRight, angle),
                    minimumSpawnRadius);

                spawnPositions.Add(OrbitMath.PositionOnCircle(
                    target.position,
                    spawnRadius,
                    angle,
                    target.position.z));
            }

            return true;
        }

        private static float SpawnRadiusOnScreenRay(
            Vector3 center,
            float padding,
            Vector3 bottomLeft,
            Vector3 topRight,
            float angle)
        {
            Vector2 direction = new(Mathf.Cos(angle), Mathf.Sin(angle));
            float edgeDistance = DistanceToScreenEdge(center, bottomLeft, topRight, direction);
            return edgeDistance + padding;
        }

        private static float DistanceToScreenEdge(Vector3 center, Vector3 bottomLeft, Vector3 topRight, Vector2 direction)
        {
            float distance = float.PositiveInfinity;

            if (Mathf.Abs(direction.x) > Mathf.Epsilon)
            {
                float xEdge = direction.x > 0f ? topRight.x : bottomLeft.x;
                distance = Mathf.Min(distance, (xEdge - center.x) / direction.x);
            }

            if (Mathf.Abs(direction.y) > Mathf.Epsilon)
            {
                float yEdge = direction.y > 0f ? topRight.y : bottomLeft.y;
                distance = Mathf.Min(distance, (yEdge - center.y) / direction.y);
            }

            return float.IsPositiveInfinity(distance) ? 0f : Mathf.Max(0f, distance);
        }

        private static float MinimumSpawnRadius(Vector3 bottomLeft, Vector3 topRight, float padding)
        {
            float width = Mathf.Max(0f, topRight.x - bottomLeft.x);
            float height = Mathf.Max(0f, topRight.y - bottomLeft.y);
            return Mathf.Min(width, height) * 0.5f + padding;
        }
    }
}
