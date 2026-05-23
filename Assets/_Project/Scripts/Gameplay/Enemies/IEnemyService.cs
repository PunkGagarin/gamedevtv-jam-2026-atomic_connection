using System;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Enemies
{
    public interface IEnemyService
    {
        event Action BossKilled;
        void Start(Transform target);
        void Update();
        int PushEnemiesFrom(Vector3 origin, float radius, float distance, float duration);
        void Cleanup();
    }
}
