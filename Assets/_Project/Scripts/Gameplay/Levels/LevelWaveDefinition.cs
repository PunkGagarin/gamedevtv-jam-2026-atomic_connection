using System;
using _Project.Scripts.Gameplay.Enemies;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Levels
{
    [Serializable]
    public class LevelWaveDefinition
    {
        [field: SerializeField, Min(0f)] public float StartTimeSeconds { get; private set; }
        [field: SerializeField, Min(0f)] public float EndTimeSeconds { get; private set; }
        [field: SerializeField, Min(0.01f)] public float SpawnIntervalSeconds { get; private set; } = 6f;
        [field: SerializeField] public EnemyId EnemyId { get; private set; }
        [field: SerializeField, Min(0)] public int MaxHealthOverride { get; private set; }
        [field: SerializeField, Min(0)] public int CoreCollisionDamageOverride { get; private set; }
        [field: SerializeField, Min(1)] public int SpawnCount { get; private set; } = 1;
        [field: SerializeField, Min(0)] public int SpawnLimit { get; private set; }

        public bool HasEndTime => EndTimeSeconds > StartTimeSeconds;
        public bool HasSpawnLimit => SpawnLimit > 0;
        public int MaxHealthFor(EnemyDefinition enemy) => MaxHealthOverride > 0 ? MaxHealthOverride : enemy.MaxHealth;
        public int CoreCollisionDamageFor(EnemyDefinition enemy) => CoreCollisionDamageOverride > 0 ? CoreCollisionDamageOverride : enemy.CoreCollisionDamage;
    }
}
