using UnityEngine;

namespace _Project.Scripts.Gameplay.Enemies
{
    public interface IEnemySpawner
    {
        EnemyUnit Spawn(EnemyDefinition definition, int maxHealth, int coreCollisionDamage, Transform target, float offscreenPadding);
    }
}
