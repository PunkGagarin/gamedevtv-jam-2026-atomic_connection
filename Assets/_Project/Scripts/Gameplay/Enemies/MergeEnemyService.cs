using System;
using System.Collections.Generic;
using _Project.Scripts.Gameplay.Common.Random;
using _Project.Scripts.Gameplay.Enemies.Components;
using _Project.Scripts.Gameplay.Level;
using _Project.Scripts.Gameplay.Levels;
using _Project.Scripts.Infrastructure.AssetManagement;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Enemies
{
    public class MergeEnemyService : IMergeEnemyService
    {
        private const string ENEMY_MERGE_LINKS_CONTAINER_NAME = "EnemyMergeLinks";

        private readonly List<EnemyUnit> _activeEnemies = new();
        private readonly List<EnemyMergeGroup> _mergeGroups = new();
        private readonly List<ActiveEnemyMergeLink> _activeMergeLinks = new();

        private LevelDefinition _level;
        private float _timeToMergeCheck;

        [Inject] private IAssetProvider _assetProvider;
        [Inject] private IGameplayRuntimeHierarchy _runtimeHierarchy;
        [Inject] private IInstantiator _instantiator;
        [Inject] private LevelCatalogConfig _levelCatalog;
        [Inject] private EnemyMergeConfig _enemyMergeConfig;
        [Inject] private ILevelSelectionService _levelSelectionService;
        [Inject] private IRandomService _random;

        public void Start()
        {
            Cleanup();

            _level = _levelCatalog.LevelFor(_levelSelectionService.SelectedLevel);
            _timeToMergeCheck = _enemyMergeConfig.MergeCheckIntervalSeconds;
        }

        public void RegisterEnemy(EnemyUnit enemy)
        {
            if (enemy == null || _activeEnemies.Contains(enemy))
                return;

            _activeEnemies.Add(enemy);
        }

        public void UnregisterEnemy(EnemyUnit enemy)
        {
            if (enemy == null)
                return;

            EnemyMergeGroup mergeGroup = enemy.MergeGroup;

            if (mergeGroup != null && mergeGroup.IsDeathWaveActive)
                mergeGroup.ReleaseDeathWaveMember(enemy);
            else
                RemoveMergeLinkFor(enemy);

            _activeEnemies.Remove(enemy);
        }

        public void TickLinks(float deltaTime, float elapsedSeconds)
        {
            for (int i = _activeMergeLinks.Count - 1; i >= 0; i--)
            {
                ActiveEnemyMergeLink link = _activeMergeLinks[i];

                if (!link.IsAlive || !IsMergeLinkStillValid(link))
                {
                    link.DestroyVisual();
                    _activeMergeLinks.RemoveAt(i);
                    continue;
                }

                link.Tick(deltaTime, elapsedSeconds, _enemyMergeConfig);
            }
        }

        public void TickDeathWaves(float deltaTime)
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

        public void TickMerge(float deltaTime)
        {
            if (!CanTickEnemyMerge() || _activeEnemies.Count < 2)
                return;

            _timeToMergeCheck -= deltaTime;
            if (_timeToMergeCheck > 0f)
                return;

            _timeToMergeCheck = _enemyMergeConfig.MergeCheckIntervalSeconds;
            TryMergeEnemyPairs();
        }

        public void Cleanup()
        {
            _level = null;
            _timeToMergeCheck = 0;
            _activeEnemies.Clear();
            CleanupMergeLinks();
            CleanupMergeGroups();
        }

        private bool CanTickEnemyMerge()
        {
            return _enemyMergeConfig.MergeEnabled
                   && _level != null
                   && _level.LevelNumber >= _enemyMergeConfig.StartLevel;
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
            EnemyMergeLinkVisual linkVisual = CreateMergeLinkVisual(first.transform, second.transform);
            ActiveEnemyMergeLink link = new(first, second, linkVisual);
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

            CleanupEmptyMergeGroups();
        }

        private void CleanupMergeLinks()
        {
            foreach (ActiveEnemyMergeLink link in _activeMergeLinks)
                link.DestroyVisual();

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
    }
}
