using System;
using System.Collections.Generic;
using _Project.Scripts.Gameplay.Common.Physics;
using _Project.Scripts.Gameplay.Common.Time;
using _Project.Scripts.Gameplay.Currencies;
using _Project.Scripts.Gameplay.Levels;
using UnityEngine;
using UnityEngine.EventSystems;
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
        private float _elapsedSeconds;

        [Inject] private IEnemySpawner _enemySpawner;
        [Inject] private IEnemyFactory _enemyFactory;
        [Inject] private LevelCatalogConfig _levelCatalog;
        [Inject] private ILevelSelectionService _levelSelectionService;
        [Inject] private ITimeService _time;
        [Inject] private IPhysicsService _physicsService;
        [Inject] private ICurrencyService _currencyService;

        public event Action BossKilled;

        public void Start(Transform target)
        {
            Cleanup();

            _target = target;
            _level = _levelCatalog.LevelFor(_levelSelectionService.SelectedLevel);
            PrepareSpawnTracks();
            _isActive = target != null;
            _spawnWasStarted = false;
            _elapsedSeconds = 0;
        }

        public void Update()
        {
            if (!_isActive || _target == null)
                return;

            if (!_spawnWasStarted)
            {
                if (FirstGameplayClickWasReceived())
                    _spawnWasStarted = true;

                return;
            }

            float deltaTime = _time.DeltaTime;
            TickEnemies(deltaTime);
            _elapsedSeconds += deltaTime;
            TickSpawnTracks(deltaTime);
        }

        public void Cleanup()
        {
            UnsubscribeFromActiveEnemies();

            _target = null;
            _level = null;
            _isActive = false;
            _spawnWasStarted = false;
            _elapsedSeconds = 0;
            _spawnTracks.Clear();

            _enemyFactory.Cleanup();
        }

        private bool FirstGameplayClickWasReceived()
        {
            if (!UnityEngine.Input.GetMouseButtonDown(0))
                return false;

            return EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject();
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
            EnemyDefinition enemyDefinition = _levelCatalog.EnemyFor(wave.EnemyId);
            float offscreenSpawnPadding = _level != null ? _level.OffscreenSpawnPadding : 1f;

            for (int i = 0; i < wave.SpawnCount; i++)
                TrackEnemy(_enemySpawner.Spawn(enemyDefinition, _target, offscreenSpawnPadding));
        }

        private void TrackEnemy(EnemyUnit enemy)
        {
            if (enemy == null)
                return;

            enemy.Died += OnEnemyDied;
            enemy.Killed += OnEnemyKilled;
            _activeEnemies.Add(enemy);
        }

        private void TickEnemies(float deltaTime)
        {
            for (int i = _activeEnemies.Count - 1; i >= 0; i--)
                _activeEnemies[i].TickMovement(deltaTime);

            _physicsService.SyncTransforms();

            for (int i = _activeEnemies.Count - 1; i >= 0; i--)
                _activeEnemies[i].TickCoreCollision();
        }

        private void OnEnemyDied(EnemyUnit enemy)
        {
            enemy.Died -= OnEnemyDied;
            enemy.Killed -= OnEnemyKilled;

            if (!_activeEnemies.Remove(enemy))
                throw new InvalidOperationException($"{nameof(EnemyService)} received death from an untracked {nameof(EnemyUnit)}.");
        }

        private void OnEnemyKilled(EnemyUnit enemy)
        {
            _currencyService.Add(new CurrencyAmount(CurrencyId.Nucleotides, enemy.NucleotideReward));

            if (enemy.Id == EnemyId.Boss)
                BossKilled?.Invoke();
        }

        private void UnsubscribeFromActiveEnemies()
        {
            foreach (EnemyUnit enemy in _activeEnemies)
            {
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
