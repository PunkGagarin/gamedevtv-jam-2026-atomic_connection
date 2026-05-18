using UnityEngine;

namespace _Project.Scripts.Gameplay.Enemies
{
    public interface IEnemyFactory
    {
        EnemyUnit Create(EnemyDefinition definition, Vector3 at);
        void Cleanup();
    }
}
