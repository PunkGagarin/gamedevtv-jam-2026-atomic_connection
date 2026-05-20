using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Enemies
{
    public interface IEnemySpawner
    {
        IReadOnlyList<EnemyUnit> SpawnGroup(EnemyDefinition definition, int maxHealth, int coreCollisionDamage, Transform target, float offscreenPadding, int count);
    }
}
