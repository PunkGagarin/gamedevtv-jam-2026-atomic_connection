using System;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Enemies
{
    public interface IEnemyService
    {
        event Action BossKilled;
        void Start(Transform target);
        void Update();
        bool TryGetNearestEnemyPosition(Vector3 origin, out Vector3 position);
        int PushEnemiesFrom(Vector3 origin, float radius, float distance, float duration);
        void Cleanup();
    }
}
