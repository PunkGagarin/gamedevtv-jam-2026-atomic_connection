using System;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Enemies
{
    [Serializable]
    public class EnemyDefinition
    {
        [field: SerializeField] public EnemyId Id { get; private set; }
        [field: SerializeField] public string PrefabResourcePath { get; private set; } = "Gameplay/Enemies/EnemyUnit";
        [field: SerializeField, Min(1)] public int MaxHealth { get; private set; } = 1;
        [field: SerializeField, Min(0f)] public float MoveSpeed { get; private set; } = 1f;
        [field: SerializeField, Min(1)] public int CoreCollisionDamage { get; private set; } = 1;
        [field: SerializeField, Min(0)] public int DnaReward { get; private set; } = 1;
    }
}
