using _Project.Scripts.Gameplay.Units.AtomCores;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Enemies.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyProjectileRuntime))]
    [RequireComponent(typeof(EnemyProjectileMotion))]
    [RequireComponent(typeof(EnemyProjectileLifetime))]
    [RequireComponent(typeof(EnemyProjectileTargetHit))]
    [RequireComponent(typeof(EnemyProjectileDamage))]
    public class EnemyProjectileLaunch : MonoBehaviour
    {
        [field: SerializeField] private EnemyProjectileRuntime Runtime { get; set; }
        [field: SerializeField] private EnemyProjectileMotion Motion { get; set; }
        [field: SerializeField] private EnemyProjectileLifetime Lifetime { get; set; }
        [field: SerializeField] private EnemyProjectileTargetHit TargetHit { get; set; }
        [field: SerializeField] private EnemyProjectileDamage Damage { get; set; }

        private void Awake()
        {
            if (Runtime == null)
                Runtime = GetComponent<EnemyProjectileRuntime>();

            if (Motion == null)
                Motion = GetComponent<EnemyProjectileMotion>();

            if (Lifetime == null)
                Lifetime = GetComponent<EnemyProjectileLifetime>();

            if (TargetHit == null)
                TargetHit = GetComponent<EnemyProjectileTargetHit>();

            if (Damage == null)
                Damage = GetComponent<EnemyProjectileDamage>();
        }

        public bool Launch(AtomCore target, Vector3 direction, float speed, int damage, float lifetime)
        {
            TargetHit.Configure(target);
            Motion.Configure(direction, speed);
            Damage.Configure(damage);
            Lifetime.Configure(lifetime);

            if (TargetHit.HasLiveTarget && Motion.CanMove && Lifetime.HasTime)
            {
                Runtime.Activate();
                return true;
            }

            Runtime.DestroySelf();
            return false;
        }
    }
}
