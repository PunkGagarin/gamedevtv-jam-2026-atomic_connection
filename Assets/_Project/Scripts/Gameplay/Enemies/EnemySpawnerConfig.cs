using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Enemies
{
    [CreateAssetMenu(fileName = "EnemySpawnerConfig", menuName = "Game Resources/Configs/Enemy Spawner")]
    public class EnemySpawnerConfig : ScriptableObject
    {
        [field: Header("Enemy Definitions")]
        [field: SerializeField] public List<EnemyDefinition> Enemies { get; private set; } = new();

        [field: Header("Spawn Defaults")]
        [field: SerializeField, Min(0.01f)] public float SpawnIntervalSeconds { get; private set; } = 6f;
        [field: SerializeField, Min(0f)] public float OffscreenSpawnPadding { get; private set; } = 1f;
        [field: SerializeField, Min(0f)] public float MoveSpeed { get; private set; } = 1f;
        [field: SerializeField, Min(1)] public int CoreCollisionDamage { get; private set; } = 1;
        [field: SerializeField, Min(0)] public int NucleotideReward { get; private set; } = 5;

        public EnemyDefinition EnemyFor(EnemyId enemyId)
        {
            EnemyDefinition enemy = Enemies?.FirstOrDefault(definition => definition.Id == enemyId);

            if (enemy == null)
                throw new InvalidOperationException($"{nameof(EnemySpawnerConfig)} has no enemy definition for {enemyId}.");

            return enemy;
        }
    }
}
