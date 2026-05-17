using UnityEngine;

namespace _Project.Scripts.Gameplay.Enemies
{
    [CreateAssetMenu(fileName = "EnemySpawnerConfig", menuName = "Game Resources/Configs/Enemy Spawner")]
    public class EnemySpawnerConfig : ScriptableObject
    {
        [field: SerializeField, Min(0.01f)] public float SpawnIntervalSeconds { get; private set; } = 6f;
        [field: SerializeField, Min(0f)] public float OffscreenSpawnPadding { get; private set; } = 1f;
        [field: SerializeField, Min(0f)] public float MoveSpeed { get; private set; } = 1f;
        [field: SerializeField, Min(1)] public int CoreCollisionDamage { get; private set; } = 1;
    }
}
