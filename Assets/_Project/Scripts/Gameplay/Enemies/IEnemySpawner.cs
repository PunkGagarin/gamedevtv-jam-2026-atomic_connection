using UnityEngine;

namespace _Project.Scripts.Gameplay.Enemies
{
    public interface IEnemySpawner
    {
        EnemyUnit Spawn(EnemyDefinition definition, Transform target, float offscreenPadding);
    }
}
