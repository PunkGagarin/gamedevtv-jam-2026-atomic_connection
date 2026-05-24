using System;
using System.Collections.Generic;
using _Project.Scripts.Gameplay.Common.Physics;
using _Project.Scripts.Gameplay.Common.Time;
using _Project.Scripts.Gameplay.Level;
using _Project.Scripts.Gameplay.Levels;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Enemies
{
    public class EnemyService : IEnemyService
    {
        private readonly List<EnemyUnit> _activeEnemies = new();
        private readonly List<SpawnTrack> _spawnTracks = new();

        private Transform _target;
        private LevelDefinition _level;
        private bool _isActive;
        private bool _spawnWasStarted;
        private float _timeToSpawnStart;
        private float _elapsedSeconds;

        [Inject] private IEnemySpawner _enemySpawner;
        [Inject] private IEnemyFactory _enemyFactory;
        [Inject] private LevelCatalogConfig _levelCatalog;
        [Inject] private EnemySpawnerConfig _enemySpawnerConfig;
        [Inject] private IMergeEnemyService _mergeEnemyService;
        [Inject] private ILevelSelectionService _levelSelectionService;
        [Inject] private ITimeService _time;
        [Inject] private IPhysicsService _physicsService;
        [Inject] private IEnemyKillRewardService _killRewardService;

        public event Action BossKilled;

        public void Start(Transform target)
        {
            Cleanup();

            _target = target;
            _level = _levelCatalog.LevelFor(_levelSelectionService.SelectedLevel);
            PrepareSpawnTracks();
            _isActive = target != null;
            _spawnWasStarted = false;
            _timeToSpawnStart = _enemySpawnerConfig.InitialSpawnDelaySeconds;
            _elapsedSeconds = 0;
        }

        public void Update()
        {
            if (!_isActive || _target == null)
                return;

            float deltaTime = _time.DeltaTime;

            if (!TickSpawnStartDelay(deltaTime))
                return;

            TickEnemies(deltaTime);
            _mergeEnemyService.TickDeathWaves(deltaTime);
            _elapsedSeconds += deltaTime;
            TickSpawnTracks(deltaTime);
            _mergeEnemyService.TickMerge(deltaTime);
        }

        public int PushEnemiesFrom(Vector3 origin, float radius, float distance, float duration)
        {
            if (radius <= 0f || distance <= 0f)
                return 0;

            float radiusSqr = radius * radius;
            int pushedCount = 0;

            foreach (EnemyUnit enemy in _activeEnemies)
            {
                if (enemy == null || !enemy.IsAlive || enemy.Id == EnemyId.Boss)
                    continue;

                if ((enemy.transform.position - origin).sqrMagnitude > radiusSqr)
                    continue;

                enemy.PushFrom(origin, distance, duration);
                pushedCount++;
            }

            return pushedCount;
        }

        public bool TryGetNearestEnemyPosition(Vector3 origin, out Vector3 position)
        {
            position = default;
            EnemyUnit nearestEnemy = null;
            float nearestDistanceSqr = float.MaxValue;

            foreach (EnemyUnit enemy in _activeEnemies)
            {
                if (enemy == null || !enemy.IsAlive)
                    continue;

                float distanceSqr = (enemy.transform.position - origin).sqrMagnitude;
                if (distanceSqr >= nearestDistanceSqr)
                    continue;

                nearestDistanceSqr = distanceSqr;
                nearestEnemy = enemy;
            }

            if (nearestEnemy == null)
                return false;

            position = nearestEnemy.transform.position;
            return true;
        }

        public void Cleanup()
        {
            UnsubscribeFromActiveEnemies();

            _target = null;
            _level = null;
            _isActive = false;
            _spawnWasStarted = false;
            _timeToSpawnStart = 0;
            _elapsedSeconds = 0;
            _spawnTracks.Clear();

            _enemyFactory.Cleanup();
        }

        private bool TickSpawnStartDelay(float deltaTime)
        {
            if (_spawnWasStarted)
                return true;

            _timeToSpawnStart -= deltaTime;
            if (_timeToSpawnStart > 0f)
                return false;

            _spawnWasStarted = true;
            return true;
        }

        private void PrepareSpawnTracks()
        {
            _spawnTracks.Clear();

            if (_level?.Waves == null)
                return;

            foreach (LevelWaveDefinition wave in _level.Waves)
                _spawnTracks.Add(new SpawnTrack(wave));
        }

        private void TickSpawnTracks(float deltaTime)
        {
            foreach (SpawnTrack track in _spawnTracks)
            {
                if (!track.ShouldTick(_elapsedSeconds))
                    continue;

                track.Tick(deltaTime);

                if (!track.ShouldSpawn)
                    continue;

                SpawnWave(track.Wave);
                track.MarkSpawned();
            }
        }

        private void SpawnWave(LevelWaveDefinition wave)
        {
            EnemyDefinition enemyDefinition = _enemySpawnerConfig.EnemyFor(wave.EnemyId);
            float offscreenSpawnPadding = _level != null ? _level.OffscreenSpawnPadding : 1f;
            int maxHealth = wave.MaxHealthFor(enemyDefinition);
            int coreCollisionDamage = wave.CoreCollisionDamageFor(enemyDefinition);

            foreach (EnemyUnit enemy in _enemySpawner.SpawnGroup(
                         enemyDefinition,
                         maxHealth,
                         coreCollisionDamage,
                         _target,
                         offscreenSpawnPadding,
                         wave.SpawnCount))
            {
                TrackEnemy(enemy);
            }
        }

        private void TrackEnemy(EnemyUnit enemy)
        {
            if (enemy == null)
                return;

            _mergeEnemyService.RegisterEnemy(enemy);
            _killRewardService.RegisterEnemy(enemy);
            enemy.Died += OnEnemyDied;
            enemy.Killed += OnEnemyKilled;
            _activeEnemies.Add(enemy);
        }

        private void TickEnemies(float deltaTime)
        {
            for (int i = _activeEnemies.Count - 1; i >= 0; i--)
                _activeEnemies[i].TickMovement(deltaTime);

            for (int i = _activeEnemies.Count - 1; i >= 0; i--)
                _activeEnemies[i].TickKnockback(deltaTime);

            for (int i = _activeEnemies.Count - 1; i >= 0; i--)
                _activeEnemies[i].TickRuntimeBehaviors(deltaTime);

            _mergeEnemyService.TickLinks(deltaTime, _elapsedSeconds);

            TickCoreCollisions();
        }

        private void TickCoreCollisions()
        {
            if (_activeEnemies.Count == 0)
                return;

            _physicsService?.SyncTransforms();

            for (int i = _activeEnemies.Count - 1; i >= 0; i--)
                _activeEnemies[i].TickCoreCollision();
        }

        private void OnEnemyDied(EnemyUnit enemy)
        {
            _mergeEnemyService.UnregisterEnemy(enemy);
            _killRewardService.UnregisterEnemy(enemy);
            enemy.Died -= OnEnemyDied;
            enemy.Killed -= OnEnemyKilled;

            if (!_activeEnemies.Remove(enemy))
                throw new InvalidOperationException($"{nameof(EnemyService)} received death from an untracked {nameof(EnemyUnit)}.");
        }

        private void OnEnemyKilled(EnemyUnit enemy)
        {
            if (enemy.Id != EnemyId.Boss)
                return;

            BossKilled?.Invoke();
        }

        private void UnsubscribeFromActiveEnemies()
        {
            foreach (EnemyUnit enemy in _activeEnemies)
            {
                _mergeEnemyService.UnregisterEnemy(enemy);
                _killRewardService.UnregisterEnemy(enemy);
                enemy.Died -= OnEnemyDied;
                enemy.Killed -= OnEnemyKilled;
            }

            _activeEnemies.Clear();
        }

        private sealed class SpawnTrack
        {
            private float _timeToNextSpawn;
            private int _spawnedCount;

            public SpawnTrack(LevelWaveDefinition wave)
            {
                Wave = wave;
                _timeToNextSpawn = 0;
            }

            public LevelWaveDefinition Wave { get; }
            public bool ShouldSpawn => _timeToNextSpawn <= 0;

            public bool ShouldTick(float elapsedSeconds)
            {
                if (elapsedSeconds < Wave.StartTimeSeconds)
                    return false;

                if (Wave.HasEndTime && elapsedSeconds >= Wave.EndTimeSeconds)
                    return false;

                return !Wave.HasSpawnLimit || _spawnedCount < Wave.SpawnLimit;
            }

            public void Tick(float deltaTime)
            {
                _timeToNextSpawn -= deltaTime;
            }

            public void MarkSpawned()
            {
                _spawnedCount++;
                _timeToNextSpawn = Wave.SpawnIntervalSeconds;
            }
        }
    }
}
