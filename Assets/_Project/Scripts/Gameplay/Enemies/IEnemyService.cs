using System;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Enemies
{
    public interface IEnemyService
    {
        event Action BossKilled;
        void Start(Transform target);
        void Update();
        void Cleanup();
    }
}
