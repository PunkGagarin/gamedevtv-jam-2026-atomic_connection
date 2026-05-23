using UnityEngine;

namespace _Project.Scripts.Gameplay.Enemies.Components
{
    [DisallowMultipleComponent]
    public class EnemyProjectileRuntime : MonoBehaviour
    {
        public bool IsActive { get; private set; }

        public void Activate()
        {
            IsActive = true;
        }

        public void DestroySelf()
        {
            IsActive = false;
            Destroy(gameObject);
        }
    }
}
