using System;
using System.Collections.Generic;
using _Project.Scripts.Gameplay.Cameras.Provider;
using _Project.Scripts.Gameplay.Currencies;
using _Project.Scripts.Gameplay.Levels;
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
        private readonly List<SpawnTrack> _spawnTracks = new();

        private Transform _target;
        private Collider2D _targetCollider;
        private LevelDefinition _level;
        private bool _isActive;
        private bool _spawnWasStarted;
        private float _elapsedSeconds;

        [Inject] private IEnemyFactory _enemyFactory;
        [Inject] private LevelCatalogConfig _levelCatalog;
        [Inject] private ILevelSelectionService _levelSelectionService;
        [Inject] private ICameraProvider _cameraProvider;
        [Inject] private IRandomService _random;
        [Inject] private ITimeService _time;
        [Inject] private ICurrencyService _currencyService;

        public event Action BossKilled;

        public void Start(Transform target)
        {
            Cleanup();

            _target = target;
            _targetCollider = target != null ? target.GetComponent<Collider2D>() : null;
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

            TickEnemies();

            float deltaTime = _time.DeltaTime;
            _elapsedSeconds += deltaTime;
            TickSpawnTracks(deltaTime);
        }

        public void Cleanup()
        {
            UnsubscribeFromActiveEnemies();

            _target = null;
            _targetCollider = null;
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

            for (int i = 0; i < wave.SpawnCount; i++)
                SpawnEnemy(enemyDefinition);
        }

        private void SpawnEnemy(EnemyDefinition enemyDefinition)
        {
            if (!TryGetOffscreenSpawnPosition(out Vector3 spawnPosition))
                return;

            EnemyUnit enemy = _enemyFactory.Create(enemyDefinition, spawnPosition);
            enemy.Died += OnEnemyDied;
            enemy.Killed += OnEnemyKilled;
            enemy.MoveTo(_target, enemyDefinition.MoveSpeed);

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
            float padding = _level != null ? _level.OffscreenSpawnPadding : 1f;
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
            }

            Physics2D.SyncTransforms();

            for (int i = _activeEnemies.Count - 1; i >= 0; i--)
            {
                EnemyUnit enemy = _activeEnemies[i];
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
                ApplyCoreCollision(enemy, core);

            enemy.DieFromCore();
        }

        private static void ApplyCoreCollision(EnemyUnit enemy, AtomCore core)
        {
            if (enemy.KillsCoreOnCollision)
            {
                core.TakeDamage(int.MaxValue);
                return;
            }

            core.TakeDamage(enemy.CoreCollisionDamage);
        }

        private void OnEnemyDied(EnemyUnit enemy)
        {
            enemy.Died -= OnEnemyDied;
            enemy.Killed -= OnEnemyKilled;

            if (!_activeEnemies.Remove(enemy))
                throw new System.InvalidOperationException($"{nameof(EnemySpawner)} received death from an untracked {nameof(EnemyUnit)}.");
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
