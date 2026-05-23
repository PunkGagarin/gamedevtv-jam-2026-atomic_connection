using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Enemies
{
    [CreateAssetMenu(fileName = "EnemyMergeConfig", menuName = "Game Resources/Configs/Enemy Merge")]
    public class EnemyMergeConfig : ScriptableObject
    {
        [field: Header("Rules")]
        [field: SerializeField] public bool MergeEnabled { get; private set; } = true;
        [field: SerializeField, Range(0f, 1f)] public float MergeChance { get; private set; } = 0.3f;
        [field: SerializeField, Min(0f)] public float MergeRadius { get; private set; } = 5f;
        [field: SerializeField, Min(0.01f)] public float MergeCheckIntervalSeconds { get; private set; } = 0.5f;
        [field: SerializeField, Min(2)] public int MaxMergeGroupSize { get; private set; } = 5;
        [field: SerializeField, Min(1)] public int MaxMergeLinksPerEnemy { get; private set; } = 2;
        [field: SerializeField, Min(0f)] public float MergeDeathWaveStepSeconds { get; private set; } = 0.08f;
        [field: SerializeField] public List<EnemyId> MergeExcludedEnemyIds { get; private set; } = new() { EnemyId.Boss };

        [field: Header("Link Visual")]
        [field: SerializeField, Min(0f)] public float MergeLinkWidth { get; private set; } = 0.12f;
        [field: SerializeField] public float MergeLinkZOffset { get; private set; } = 0.05f;
        [field: SerializeField, Min(0)] public int MergeLinkIntermediatePointCount { get; private set; } = 5;
        [field: SerializeField] public string MergeLinkVisualResourcePath { get; private set; } = "Gameplay/Enemies/EnemyMergeLinkVisual";

        [field: Header("Tether Motion")]
        [field: SerializeField, Min(0f)] public float MergeTetherMinimumDistance { get; private set; } = 2.32f;
        [field: SerializeField, Min(0f)] public float MergeTetherPreferredDistance { get; private set; } = 2.8f;
        [field: SerializeField, Min(0f)] public float MergeTetherPullStrength { get; private set; } = 5.5f;
        [field: SerializeField, Min(0f)] public float MergeTetherDeadZone { get; private set; } = 0.08f;
        [field: SerializeField, Min(0f)] public float MergeTetherTugAmplitude { get; private set; } = 1.05f;
        [field: SerializeField, Min(0f)] public float MergeTetherTugMinFrequency { get; private set; } = 2.2f;
        [field: SerializeField, Min(0f)] public float MergeTetherTugMaxFrequency { get; private set; } = 5.4f;
        [field: SerializeField, Min(0f)] public float MergeTetherContestLoserPull { get; private set; } = 1f;
        [field: SerializeField, Min(0f)] public float MergeTetherContestWinnerDrift { get; private set; } = 0.25f;

        public bool IsMergeExcluded(EnemyId enemyId)
        {
            return MergeExcludedEnemyIds != null && MergeExcludedEnemyIds.Contains(enemyId);
        }
    }
}
