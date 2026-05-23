using System;
using System.Collections.Generic;
using _Project.Scripts.Gameplay.Common.Random;
using _Project.Scripts.Gameplay.Common.Time;
using _Project.Scripts.Gameplay.Currencies;
using _Project.Scripts.Gameplay.CurrencyDrops;
using _Project.Scripts.Gameplay.Enemies.Components;
using _Project.Scripts.Gameplay.Level;
using _Project.Scripts.Gameplay.Levels;
using _Project.Scripts.Infrastructure.AssetManagement;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Enemies
{
    public class EnemyService : IEnemyService
    {
        private const string ENEMY_MERGE_LINKS_CONTAINER_NAME = "EnemyMergeLinks";
        private readonly List<EnemyUnit> _activeEnemies = new();
        private readonly List<SpawnTrack> _spawnTracks = new();
        private readonly List<ActiveEnemyMergeLink> _activeMergeLinks = new();

        private Transform _target;
        private LevelDefinition _level;
        private bool _isActive;
        private bool _spawnWasStarted;
        private float _timeToSpawnStart;
        private float _elapsedSeconds;
        private float _timeToMergeCheck;

        [Inject] private IEnemySpawner _enemySpawner;
        [Inject] private IEnemyFactory _enemyFactory;
        [Inject] private IAssetProvider _assetProvider;
        [Inject] private IGameplayRuntimeHierarchy _runtimeHierarchy;
        [Inject] private IInstantiator _instantiator;
        [Inject] private LevelCatalogConfig _levelCatalog;
        [Inject] private EnemySpawnerConfig _enemySpawnerConfig;
        [Inject] private EnemyMergeConfig _enemyMergeConfig;
        [Inject] private ILevelSelectionService _levelSelectionService;
        [Inject] private IRandomService _random;
        [Inject] private ITimeService _time;
        [Inject] private ICurrencyPickupService _currencyPickupService;

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
            _timeToMergeCheck = _enemyMergeConfig.MergeCheckIntervalSeconds;
        }

        public void Update()
        {
            if (!_isActive || _target == null)
                return;

            float deltaTime = _time.DeltaTime;

            if (!TickSpawnStartDelay(deltaTime))
                return;

            TickEnemies(deltaTime);
            _elapsedSeconds += deltaTime;
            TickSpawnTracks(deltaTime);
            TickEnemyMerge(deltaTime);
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
            _timeToMergeCheck = 0;
            _spawnTracks.Clear();
            CleanupMergeLinks();

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

            enemy.Died += OnEnemyDied;
            enemy.Killed += OnEnemyKilled;
            _activeEnemies.Add(enemy);
        }

        private void TickEnemyMerge(float deltaTime)
        {
            if (!_enemyMergeConfig.MergeEnabled || _activeEnemies.Count < 2)
                return;

            _timeToMergeCheck -= deltaTime;
            if (_timeToMergeCheck > 0f)
                return;

            _timeToMergeCheck = _enemyMergeConfig.MergeCheckIntervalSeconds;
            TryMergeEnemyPairs();
        }

        private void TryMergeEnemyPairs()
        {
            float mergeRadiusSqr = MergeRadiusSqr();

            for (int i = 0; i < _activeEnemies.Count; i++)
            {
                EnemyUnit first = _activeEnemies[i];

                if (!CanMerge(first))
                    continue;

                EnemyUnit second = FindMergeCandidatePassingChance(first, i + 1, mergeRadiusSqr);

                if (second == null)
                    continue;

                MergeEnemies(first, second);
            }
        }

        private float MergeRadiusSqr()
        {
            float mergeRadius = Mathf.Max(0f, _enemyMergeConfig.MergeRadius);
            return mergeRadius * mergeRadius;
        }

        private EnemyUnit FindMergeCandidatePassingChance(EnemyUnit first, int startIndex, float mergeRadiusSqr)
        {
            for (int i = startIndex; i < _activeEnemies.Count; i++)
            {
                EnemyUnit second = _activeEnemies[i];

                if (CanMerge(first, second, mergeRadiusSqr) && RollMergeChance())
                    return second;
            }

            return null;
        }

        private bool RollMergeChance()
        {
            float chance = Mathf.Clamp01(_enemyMergeConfig.MergeChance);

            if (chance <= 0f)
                return false;

            return chance >= 1f || _random.Range(0f, 1f) < chance;
        }

        private bool CanMerge(EnemyUnit enemy)
        {
            return enemy != null
                   && enemy.IsAlive
                   && !enemy.IsMergeLinked
                   && !_enemyMergeConfig.IsMergeExcluded(enemy.Id);
        }

        private bool CanMerge(EnemyUnit first, EnemyUnit second, float mergeRadiusSqr)
        {
            if (!CanMerge(second) || first.Id != second.Id)
                return false;

            return (first.transform.position - second.transform.position).sqrMagnitude <= mergeRadiusSqr;
        }

        private void MergeEnemies(EnemyUnit first, EnemyUnit second)
        {
            EnemyMergeLinkVisual linkVisual = CreateMergeLinkVisual(first.transform, second.transform);
            int mergedMaxHealth = first.MaxHealth + second.MaxHealth;
            int mergedCurrentHealth = first.CurrentHealth + second.CurrentHealth;

            first.BeginMergeLink(second, mergedMaxHealth, mergedCurrentHealth);
            second.BeginMergeLink(first, mergedMaxHealth, mergedCurrentHealth);
            _activeMergeLinks.Add(new ActiveEnemyMergeLink(first, second, linkVisual));
        }

        private EnemyMergeLinkVisual CreateMergeLinkVisual(Transform first, Transform second)
        {
            EnemyMergeLinkVisual prefab = _assetProvider.LoadAsset<EnemyMergeLinkVisual>(_enemyMergeConfig.MergeLinkVisualResourcePath);

            if (prefab == null)
                throw new InvalidOperationException($"Enemy merge link prefab is missing at Resources path '{_enemyMergeConfig.MergeLinkVisualResourcePath}'.");

            Transform parent = _runtimeHierarchy.GetOrCreateContainer(ENEMY_MERGE_LINKS_CONTAINER_NAME);
            EnemyMergeLinkVisual visual = _instantiator.InstantiatePrefabForComponent<EnemyMergeLinkVisual>(
                prefab,
                Vector3.zero,
                Quaternion.identity,
                parent);

            visual.Configure(
                first,
                second,
                _enemyMergeConfig.MergeLinkWidth,
                _enemyMergeConfig.MergeLinkZOffset,
                _enemyMergeConfig.MergeLinkIntermediatePointCount);
            return visual;
        }

        private void TickMergeLinks(float deltaTime)
        {
            for (int i = _activeMergeLinks.Count - 1; i >= 0; i--)
            {
                ActiveEnemyMergeLink link = _activeMergeLinks[i];

                if (!link.IsAlive)
                {
                    link.DestroyVisual();
                    _activeMergeLinks.RemoveAt(i);
                    continue;
                }

                link.Tick(deltaTime, _elapsedSeconds, _enemyMergeConfig);
            }
        }

        private void RemoveMergeLinkFor(EnemyUnit enemy)
        {
            for (int i = _activeMergeLinks.Count - 1; i >= 0; i--)
            {
                ActiveEnemyMergeLink link = _activeMergeLinks[i];

                if (!link.Contains(enemy))
                    continue;

                link.DestroyVisual();
                _activeMergeLinks.RemoveAt(i);
            }
        }

        private void CleanupMergeLinks()
        {
            foreach (ActiveEnemyMergeLink link in _activeMergeLinks)
                link.DestroyVisual();

            _activeMergeLinks.Clear();
        }

        private void TickEnemies(float deltaTime)
        {
            for (int i = _activeEnemies.Count - 1; i >= 0; i--)
                _activeEnemies[i].TickMovement(deltaTime);

            for (int i = _activeEnemies.Count - 1; i >= 0; i--)
                _activeEnemies[i].TickRuntimeBehaviors(deltaTime);

            TickMergeLinks(deltaTime);

            for (int i = _activeEnemies.Count - 1; i >= 0; i--)
                _activeEnemies[i].TickCoreCollision();
        }

        private void OnEnemyDied(EnemyUnit enemy)
        {
            RemoveMergeLinkFor(enemy);
            enemy.Died -= OnEnemyDied;
            enemy.Killed -= OnEnemyKilled;

            if (!_activeEnemies.Remove(enemy))
                throw new InvalidOperationException($"{nameof(EnemyService)} received death from an untracked {nameof(EnemyUnit)}.");
        }

        private void OnEnemyKilled(EnemyUnit enemy)
        {
            if (enemy.Id == EnemyId.Boss)
            {
                BossKilled?.Invoke();
                return;
            }

            _currencyPickupService.Spawn(
                new CurrencyAmount(CurrencyId.Nucleotides, enemy.NucleotideReward),
                enemy.transform.position);
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
