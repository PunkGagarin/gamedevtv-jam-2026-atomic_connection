using UnityEngine;

namespace _Project.Scripts.Gameplay.Common.Health
{
    public class Health : MonoBehaviour
    {
        public bool IsAlive { get; private set; } = true;

        public void Kill()
        {
            if (!IsAlive)
                return;

            IsAlive = false;
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }
}
