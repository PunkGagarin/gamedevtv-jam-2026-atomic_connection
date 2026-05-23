using UnityEngine;

namespace _Project.Scripts.Gameplay.Enemies.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyProjectileTargetHit))]
    [RequireComponent(typeof(EnemyProjectileDamage))]
    public class EnemyProjectileImpact : MonoBehaviour
    {
        [field: SerializeField] private EnemyProjectileTargetHit TargetHit { get; set; }
        [field: SerializeField] private EnemyProjectileDamage Damage { get; set; }

        private void Awake()
        {
            if (TargetHit == null)
                TargetHit = GetComponent<EnemyProjectileTargetHit>();

            if (Damage == null)
                Damage = GetComponent<EnemyProjectileDamage>();
        }

        public bool TryApply()
        {
            if (!TargetHit.IsOverlappingTarget())
                return false;

            Damage.Apply(TargetHit.Target);
            return true;
        }
    }
}
