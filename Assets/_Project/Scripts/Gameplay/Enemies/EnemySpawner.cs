using System.Collections.Generic;
using _Project.Scripts.Gameplay.Cameras.Provider;
using _Project.Scripts.Gameplay.Common.Random;
using _Project.Scripts.Gameplay.Units.AtomCores;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Enemies
{
    public class EnemySpawner : IEnemySpawner
    {
        private const float GROUP_SPAWN_SPACING = 0.65f;
        private const float GROUP_SPAWN_JITTER = 0.12f;

        [Inject] private IEnemyFactory _enemyFactory;
        [Inject] private ICameraProvider _cameraProvider;
        [Inject] private IRandomService _random;

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
            float padding = Mathf.Max(0, offscreenSpawnPadding);
            int side = _random.Range(0, 4);
            bool verticalSide = side <= 1;
            float axisMin = verticalSide ? bottomLeft.y : bottomLeft.x;
            float axisMax = verticalSide ? topRight.y : topRight.x;
            float axisLength = axisMax - axisMin;
            float groupSpan = Mathf.Min(axisLength, (Mathf.Max(1, count) - 1) * GROUP_SPAWN_SPACING);
            float centerMin = axisMin + groupSpan * 0.5f;
            float centerMax = axisMax - groupSpan * 0.5f;
            float center = centerMin >= centerMax ? (axisMin + axisMax) * 0.5f : _random.Range(centerMin, centerMax);

            for (int i = 0; i < count; i++)
            {
                float offset = count <= 1 ? 0f : (i - (count - 1) * 0.5f) * GROUP_SPAWN_SPACING;
                float jitter = count <= 1 ? 0f : _random.Range(-GROUP_SPAWN_JITTER, GROUP_SPAWN_JITTER);
                float axisValue = Mathf.Clamp(center + offset + jitter, axisMin, axisMax);
                spawnPositions.Add(PositionOnSide(side, axisValue, bottomLeft, topRight, padding, target.position.z));
            }

            return true;
        }

        private static Vector3 PositionOnSide(int side, float axisValue, Vector3 bottomLeft, Vector3 topRight, float padding, float z)
        {
            return side switch
            {
                0 => new Vector3(bottomLeft.x - padding, axisValue, z),
                1 => new Vector3(topRight.x + padding, axisValue, z),
                2 => new Vector3(axisValue, bottomLeft.y - padding, z),
                _ => new Vector3(axisValue, topRight.y + padding, z)
            };
        }
    }
}
