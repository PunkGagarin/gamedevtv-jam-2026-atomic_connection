using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Enemies.Components
{
    public class EnemyRuntimeBehaviors : MonoBehaviour
    {
        private readonly List<IEnemyRuntimeBehavior> _behaviors = new();

        private void Awake()
        {
            _behaviors.Clear();
            _behaviors.AddRange(GetComponents<IEnemyRuntimeBehavior>());
        }

        public void Configure(Transform target)
        {
            foreach (IEnemyRuntimeBehavior behavior in _behaviors)
                behavior.Configure(target);
        }

        public void Tick(float deltaTime)
        {
            foreach (IEnemyRuntimeBehavior behavior in _behaviors)
                behavior.Tick(deltaTime);
        }

        public void Clear()
        {
            foreach (IEnemyRuntimeBehavior behavior in _behaviors)
                behavior.Clear();
        }
    }
}
