using UnityEngine;

namespace _Project.Scripts.Gameplay.Enemies.Components
{
    public interface IEnemyRuntimeBehavior
    {
        void Configure(Transform target);
        void Tick(float deltaTime);
        void Clear();
    }
}
