using System.Collections.Generic;
using _Project.Scripts.Gameplay.Cameras.Provider;
using _Project.Scripts.Gameplay.Units.AtomCores;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;
using _Project.Scripts.Gameplay.Common.Random;
using _Project.Scripts.Gameplay.Common.Time;

namespace _Project.Scripts.Gameplay.Enemies
{
    public class EnemySpawner : IEnemySpawner
    {
        private readonly List<EnemyUnit> _activeEnemies = new();

        private Transform _target;
        private Collider2D _targetCollider;
        private bool _isActive;
        private bool _spawnWasStarted;
        private float _timeToNextSpawn;

        [Inject] private IEnemyFactory _enemyFactory;
        [Inject] private EnemySpawnerConfig _config;
        [Inject] private ICameraProvider _cameraProvider;
        [Inject] private IRandomService _random;
        [Inject] private ITimeService _time;

        public void Start(Transform target)
        {
            _target = target;
            _targetCollider = target != null ? target.GetComponent<Collider2D>() : null;
            _isActive = target != null;
            _spawnWasStarted = false;
            _timeToNextSpawn = 0;
        }

        public void Update()
        {
            if (!_isActive || _target == null)
                return;

            TickEnemies();

            if (!_spawnWasStarted)
            {
                if (FirstGameplayClickWasReceived())
                    _spawnWasStarted = true;

                return;
            }

            _timeToNextSpawn -= _time.DeltaTime;

            if (_timeToNextSpawn > 0)
                return;

            SpawnEnemy();
            _timeToNextSpawn = _config.SpawnIntervalSeconds;
        }

        public void Cleanup()
        {
            UnsubscribeFromActiveEnemies();

            _target = null;
            _targetCollider = null;
            _isActive = false;
            _spawnWasStarted = false;
            _timeToNextSpawn = 0;

            _enemyFactory.Cleanup();
        }

        private bool FirstGameplayClickWasReceived()
        {
            if (!UnityEngine.Input.GetMouseButtonDown(0))
                return false;

            return EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject();
        }

        private void SpawnEnemy()
        {
            if (!TryGetOffscreenSpawnPosition(out Vector3 spawnPosition))
                return;

            EnemyUnit enemy = _enemyFactory.Create(spawnPosition);
            enemy.Died += OnEnemyDied;
            enemy.MoveTo(_target, _config.MoveSpeed);

            _activeEnemies.Add(enemy);
        }

        private bool TryGetOffscreenSpawnPosition(out Vector3 spawnPosition)
        {
            spawnPosition = Vector3.zero;

            Camera camera = _cameraProvider.MainCamera;
            if (camera == null || _target == null)
                return false;

            float depth = Mathf.Abs(camera.transform.position.z - _target.position.z);
            Vector3 bottomLeft = camera.ViewportToWorldPoint(new Vector3(0, 0, depth));
            Vector3 topRight = camera.ViewportToWorldPoint(new Vector3(1, 1, depth));
            float padding = _config.OffscreenSpawnPadding;
            int side = _random.Range(0, 4);

            spawnPosition = side switch
            {
                0 => new Vector3(bottomLeft.x - padding, _random.Range(bottomLeft.y, topRight.y), _target.position.z),
                1 => new Vector3(topRight.x + padding, _random.Range(bottomLeft.y, topRight.y), _target.position.z),
                2 => new Vector3(_random.Range(bottomLeft.x, topRight.x), bottomLeft.y - padding, _target.position.z),
                _ => new Vector3(_random.Range(bottomLeft.x, topRight.x), topRight.y + padding, _target.position.z)
            };

            return true;
        }

        private void TickEnemies()
        {
            float deltaTime = _time.DeltaTime;

            for (int i = _activeEnemies.Count - 1; i >= 0; i--)
            {
                EnemyUnit enemy = _activeEnemies[i];
                enemy.Tick(deltaTime);
                TryHitCore(enemy);
            }
        }

        private void TryHitCore(EnemyUnit enemy)
        {
            if (_targetCollider == null || enemy == null || !enemy.IsAlive)
                return;

            Collider2D enemyCollider = enemy.GetComponent<Collider2D>();

            if (enemyCollider == null || !enemyCollider.Distance(_targetCollider).isOverlapped)
                return;

            if (_target.TryGetComponent(out AtomCore core))
                core.TakeDamage(_config.CoreCollisionDamage);

            enemy.Kill();
        }

        private void OnEnemyDied(EnemyUnit enemy)
        {
            enemy.Died -= OnEnemyDied;

            if (!_activeEnemies.Remove(enemy))
                throw new System.InvalidOperationException($"{nameof(EnemySpawner)} received death from an untracked {nameof(EnemyUnit)}.");
        }

        private void UnsubscribeFromActiveEnemies()
        {
            foreach (EnemyUnit enemy in _activeEnemies)
                enemy.Died -= OnEnemyDied;

            _activeEnemies.Clear();
        }
    }
}
