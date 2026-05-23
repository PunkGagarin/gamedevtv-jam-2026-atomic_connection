using UnityEngine;

namespace _Project.Scripts.Gameplay.Enemies
{
    public interface IEnemyProjectileService
    {
        void Shoot(EnemyProjectileShot shot);
        void Update();
        void CleanupOwner(Component owner);
        void Cleanup();
    }
}
