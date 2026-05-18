using UnityEngine;

namespace _Project.Scripts.Gameplay.Enemies
{
    public interface IEnemyFactory
    {
        EnemyUnit Create(EnemyDefinition definition, int maxHealth, int coreCollisionDamage, Vector3 at);
        void Cleanup();
    }
}
