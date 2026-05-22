using System;
using System.Collections.Generic;
using _Project.Scripts.Gameplay.Common.Random;
using _Project.Scripts.Gameplay.Common.Physics;
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
        private readonly List<EnemyMergeGroup> _mergeGroups = new();
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
        [Inject] private IPhysicsService _physicsService;
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
            TickMergeDeathWaves(deltaTime);
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
            CleanupMergeGroups();

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

                if (!CanParticipateInMerge(first))
                    continue;

                EnemyUnit second = FindMergeCandidatePassingChance(first, mergeRadiusSqr);

                if (second == null)
                    continue;

                LinkEnemies(first, second);
            }
        }

        private float MergeRadiusSqr()
        {
            float mergeRadius = Mathf.Max(0f, _enemyMergeConfig.MergeRadius);
            return mergeRadius * mergeRadius;
        }

        private EnemyUnit FindMergeCandidatePassingChance(EnemyUnit first, float mergeRadiusSqr)
        {
            for (int i = 0; i < _activeEnemies.Count; i++)
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

        private bool CanParticipateInMerge(EnemyUnit enemy)
        {
            return enemy != null
                   && enemy.IsAlive
                   && !_enemyMergeConfig.IsMergeExcluded(enemy.Id);
        }

        private bool CanMerge(EnemyUnit first, EnemyUnit second, float mergeRadiusSqr)
        {
            if (first == second
                || !CanParticipateInMerge(second)
                || first.Id != second.Id
                || HasMergeLink(first, second)
                || MergeLinkCountFor(first) >= MaxMergeLinksPerEnemy()
                || MergeLinkCountFor(second) >= MaxMergeLinksPerEnemy()
                || !CanFitMergedGroup(first, second))
                return false;

            return (first.transform.position - second.transform.position).sqrMagnitude <= mergeRadiusSqr;
        }

        private bool CanFitMergedGroup(EnemyUnit first, EnemyUnit second)
        {
            EnemyMergeGroup firstGroup = first.MergeGroup;
            EnemyMergeGroup secondGroup = second.MergeGroup;
            int maxGroupSize = MaxMergeGroupSize();

            if (firstGroup == null && secondGroup == null)
                return maxGroupSize >= 2;

            if (firstGroup != null && secondGroup != null)
                return firstGroup == secondGroup || firstGroup.Count + secondGroup.Count <= maxGroupSize;

            EnemyMergeGroup existingGroup = firstGroup ?? secondGroup;
            return existingGroup.Count < maxGroupSize;
        }

        private int MaxMergeGroupSize()
        {
            return Mathf.Max(2, _enemyMergeConfig.MaxMergeGroupSize);
        }

        private int MaxMergeLinksPerEnemy()
        {
            return Mathf.Max(1, _enemyMergeConfig.MaxMergeLinksPerEnemy);
        }

        private void LinkEnemies(EnemyUnit first, EnemyUnit second)
        {
            EnemyMergeLinkView linkView = CreateMergeLinkView(first.transform, second.transform);
            ActiveEnemyMergeLink link = new(first, second, linkView);
            EnemyMergeGroup group = ResolveMergeGroup(first, second);

            group.AddLink(link);
            _activeMergeLinks.Add(link);
        }

        private EnemyMergeGroup ResolveMergeGroup(EnemyUnit first, EnemyUnit second)
        {
            EnemyMergeGroup firstGroup = first.MergeGroup;
            EnemyMergeGroup secondGroup = second.MergeGroup;

            if (firstGroup == null && secondGroup == null)
            {
                EnemyMergeGroup group = new();
                group.ConfigureDeathWave(MergeDeathWaveStepSeconds());
                group.AddUngrouped(first);
                group.AddUngrouped(second);
                _mergeGroups.Add(group);
                return group;
            }

            if (firstGroup != null && secondGroup != null)
            {
                if (firstGroup == secondGroup)
                    return firstGroup;

                firstGroup.MergeWith(secondGroup);
                firstGroup.ConfigureDeathWave(MergeDeathWaveStepSeconds());
                _mergeGroups.Remove(secondGroup);
                return firstGroup;
            }

            EnemyMergeGroup existingGroup = firstGroup ?? secondGroup;
            EnemyUnit ungroupedEnemy = firstGroup == null ? first : second;
            existingGroup.ConfigureDeathWave(MergeDeathWaveStepSeconds());
            existingGroup.AddUngrouped(ungroupedEnemy);
            return existingGroup;
        }

        private float MergeDeathWaveStepSeconds()
        {
            return Mathf.Max(0f, _enemyMergeConfig.MergeDeathWaveStepSeconds);
        }

        private EnemyMergeLinkView CreateMergeLinkView(Transform first, Transform second)
        {
            EnemyMergeLinkView prefab = _assetProvider.LoadAsset<EnemyMergeLinkView>(_enemyMergeConfig.MergeLinkViewResourcePath);

            if (prefab == null)
                throw new InvalidOperationException($"Enemy merge link prefab is missing at Resources path '{_enemyMergeConfig.MergeLinkViewResourcePath}'.");

            Transform parent = _runtimeHierarchy.GetOrCreateContainer(ENEMY_MERGE_LINKS_CONTAINER_NAME);
            EnemyMergeLinkView view = _instantiator.InstantiatePrefabForComponent<EnemyMergeLinkView>(
                prefab,
                Vector3.zero,
                Quaternion.identity,
                parent);

            view.Configure(
                first,
                second,
                _enemyMergeConfig.MergeLinkWidth,
                _enemyMergeConfig.MergeLinkZOffset,
                _enemyMergeConfig.MergeLinkIntermediatePointCount);
            return view;
        }

        private void TickMergeLinks(float deltaTime)
        {
            for (int i = _activeMergeLinks.Count - 1; i >= 0; i--)
            {
                ActiveEnemyMergeLink link = _activeMergeLinks[i];

                if (!link.IsAlive || !IsMergeLinkStillValid(link))
                {
                    link.DestroyView();
                    _activeMergeLinks.RemoveAt(i);
                    continue;
                }

                link.Tick(deltaTime, _enemyMergeConfig);
            }
        }

        private void RemoveMergeLinkFor(EnemyUnit enemy)
        {
            for (int i = _activeMergeLinks.Count - 1; i >= 0; i--)
            {
                ActiveEnemyMergeLink link = _activeMergeLinks[i];

                if (!link.Contains(enemy))
                    continue;

                link.DestroyView();
                _activeMergeLinks.RemoveAt(i);
            }

            CleanupEmptyMergeGroups();
        }

        private void CleanupMergeLinks()
        {
            foreach (ActiveEnemyMergeLink link in _activeMergeLinks)
                link.DestroyView();

            _activeMergeLinks.Clear();
        }

        private void CleanupMergeGroups()
        {
            foreach (EnemyMergeGroup group in _mergeGroups)
                group.ClearMembers();

            _mergeGroups.Clear();
        }

        private void CleanupEmptyMergeGroups()
        {
            for (int i = _mergeGroups.Count - 1; i >= 0; i--)
            {
                if (_mergeGroups[i].Count == 0)
                    _mergeGroups.RemoveAt(i);
            }
        }

        private void TickMergeDeathWaves(float deltaTime)
        {
            for (int i = _mergeGroups.Count - 1; i >= 0;)
            {
                if (i >= _mergeGroups.Count)
                {
                    i = _mergeGroups.Count - 1;
                    continue;
                }

                EnemyMergeGroup group = _mergeGroups[i];
                group.TickDeathWave(deltaTime);

                if (group.Count == 0)
                    _mergeGroups.Remove(group);

                i--;
            }
        }

        private bool HasMergeLink(EnemyUnit first, EnemyUnit second)
        {
            foreach (ActiveEnemyMergeLink link in _activeMergeLinks)
            {
                if (link.Connects(first, second))
                    return true;
            }

            return false;
        }

        private int MergeLinkCountFor(EnemyUnit enemy)
        {
            int count = 0;

            foreach (ActiveEnemyMergeLink link in _activeMergeLinks)
            {
                if (link.Contains(enemy))
                    count++;
            }

            return count;
        }

        private bool IsMergeLinkStillValid(ActiveEnemyMergeLink link)
        {
            if (link.First.MergeGroup == null || link.First.MergeGroup != link.Second.MergeGroup)
                return false;

            return link.First.MergeGroup.Contains(link.First) && link.First.MergeGroup.Contains(link.Second);
        }

        private void TickEnemies(float deltaTime)
        {
            for (int i = _activeEnemies.Count - 1; i >= 0; i--)
                _activeEnemies[i].TickMovement(deltaTime);

            for (int i = _activeEnemies.Count - 1; i >= 0; i--)
                _activeEnemies[i].TickRuntimeBehaviors(deltaTime);

            TickMergeLinks(deltaTime);
            _physicsService.SyncTransforms();

            for (int i = _activeEnemies.Count - 1; i >= 0; i--)
                _activeEnemies[i].TickCoreCollision();
        }

        private void OnEnemyDied(EnemyUnit enemy)
        {
            EnemyMergeGroup mergeGroup = enemy.MergeGroup;

            if (mergeGroup != null && mergeGroup.IsDeathWaveActive)
                mergeGroup.ReleaseDeathWaveMember(enemy);
            else
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

            int reward = Mathf.Max(0, Mathf.RoundToInt(enemy.NucleotideReward * enemy.KillRewardMultiplier));

            _currencyPickupService.Spawn(
                new CurrencyAmount(CurrencyId.Nucleotides, reward),
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
