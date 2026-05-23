using UnityEngine;
using _Project.Scripts.Gameplay.Units.AtomCores;

namespace _Project.Scripts.Gameplay.Enemies.Components
{
    public class EnemyProjectileDamage : MonoBehaviour
    {
        private int _damage;

        public void Configure(int damage)
        {
            _damage = Mathf.Max(0, damage);
        }

        public void Apply(AtomCore target)
        {
            target?.TakeDamage(_damage);
        }
    }
}
