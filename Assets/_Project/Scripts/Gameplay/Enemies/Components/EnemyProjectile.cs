using _Project.Scripts.Gameplay.Units.AtomCores;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Enemies.Components
{
    [RequireComponent(typeof(EnemyProjectileRuntime))]
    [RequireComponent(typeof(EnemyProjectileLaunch))]
    [RequireComponent(typeof(EnemyProjectileMotion))]
    [RequireComponent(typeof(EnemyProjectileLifetime))]
    [RequireComponent(typeof(EnemyProjectileTargetHit))]
    [RequireComponent(typeof(EnemyProjectileImpact))]
    public class EnemyProjectile : MonoBehaviour
    {
        [field: SerializeField] private EnemyProjectileRuntime Runtime { get; set; }
        [field: SerializeField] private EnemyProjectileLaunch Launcher { get; set; }
        [field: SerializeField] private EnemyProjectileMotion Motion { get; set; }
        [field: SerializeField] private EnemyProjectileLifetime Lifetime { get; set; }
        [field: SerializeField] private EnemyProjectileTargetHit TargetHit { get; set; }
        [field: SerializeField] private EnemyProjectileImpact Impact { get; set; }

        public bool IsActive => Runtime.IsActive;
        public bool HasLiveTarget => TargetHit.HasLiveTarget;
        public bool IsExpired => Lifetime.IsExpired;

        private void Awake()
        {
            if (Runtime == null)
                Runtime = GetComponent<EnemyProjectileRuntime>();

            if (Launcher == null)
                Launcher = GetComponent<EnemyProjectileLaunch>();

            if (Motion == null)
                Motion = GetComponent<EnemyProjectileMotion>();

            if (Lifetime == null)
                Lifetime = GetComponent<EnemyProjectileLifetime>();

            if (TargetHit == null)
                TargetHit = GetComponent<EnemyProjectileTargetHit>();

            if (Impact == null)
                Impact = GetComponent<EnemyProjectileImpact>();
        }

        public bool Launch(AtomCore target, Vector3 direction, float speed, int damage, float lifetime)
        {
            return Launcher.Launch(target, direction, speed, damage, lifetime);
        }

        public void TickLifetime(float deltaTime)
        {
            Lifetime.Tick(deltaTime);
        }

        public void TickMotion(float deltaTime)
        {
            Motion.Tick(deltaTime);
        }

        public bool TryApplyImpact()
        {
            return Impact.TryApply(transform.position);
        }

        public void DestroySelf()
        {
            Runtime.DestroySelf();
        }
    }
}
