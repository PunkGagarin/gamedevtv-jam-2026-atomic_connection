using _Project.Scripts.Gameplay.Common.Health;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Enemies
{
    [RequireComponent(typeof(Health))]
    public class EnemyUnit : MonoBehaviour
    {
        [field: SerializeField] private Health Health { get; set; }

        public bool IsAlive => Health == null || Health.IsAlive;

        private void Awake()
        {
            if (Health == null)
                Health = GetComponent<Health>();
        }

        public void Kill()
        {
            if (Health != null)
                Health.Kill();
            else
                Destroy(gameObject);
        }
    }
}
