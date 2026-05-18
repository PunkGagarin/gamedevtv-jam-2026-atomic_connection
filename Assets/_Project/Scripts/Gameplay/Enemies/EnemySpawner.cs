using _Project.Scripts.Gameplay.Cameras.Provider;
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

        public EnemyUnit Spawn(EnemyDefinition definition, Transform target, float offscreenPadding)
        {
            if (!TryGetOffscreenSpawnPosition(target, offscreenPadding, out Vector3 spawnPosition))
                return null;

            EnemyUnit enemy = _enemyFactory.Create(definition, spawnPosition);
            enemy.MoveTo(target, definition.MoveSpeed);
            enemy.CollideWithCore(target != null && target.TryGetComponent(out AtomCore core) ? core : null);
            return enemy;
        }

        private bool TryGetOffscreenSpawnPosition(Transform target, float offscreenSpawnPadding, out Vector3 spawnPosition)
        {
            spawnPosition = Vector3.zero;

            Camera camera = _cameraProvider.MainCamera;
            if (camera == null || target == null)
                return false;

            float depth = Mathf.Abs(camera.transform.position.z - target.position.z);
            Vector3 bottomLeft = camera.ViewportToWorldPoint(new Vector3(0, 0, depth));
            Vector3 topRight = camera.ViewportToWorldPoint(new Vector3(1, 1, depth));
            float padding = Mathf.Max(0, offscreenSpawnPadding);
            int side = _random.Range(0, 4);

            spawnPosition = side switch
            {
                0 => new Vector3(bottomLeft.x - padding, _random.Range(bottomLeft.y, topRight.y), target.position.z),
                1 => new Vector3(topRight.x + padding, _random.Range(bottomLeft.y, topRight.y), target.position.z),
                2 => new Vector3(_random.Range(bottomLeft.x, topRight.x), bottomLeft.y - padding, target.position.z),
                _ => new Vector3(_random.Range(bottomLeft.x, topRight.x), topRight.y + padding, target.position.z)
            };

            return true;
        }
    }
}
