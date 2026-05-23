using UnityEngine;

namespace _Project.Scripts.Gameplay.Enemies.Components
{
    [RequireComponent(typeof(EnemyMovement))]
    [RequireComponent(typeof(EnemyCoreCollision))]
    [RequireComponent(typeof(EnemyRuntimeBehaviors))]
    [RequireComponent(typeof(EnemyVitality))]
    [RequireComponent(typeof(EnemyMergeState))]
    public class EnemyLifecycle : MonoBehaviour
    {
        [field: SerializeField] private EnemyMovement Movement { get; set; }
        [field: SerializeField] private EnemyCoreCollision CoreCollision { get; set; }
        [field: SerializeField] private EnemyRuntimeBehaviors RuntimeBehaviors { get; set; }
        [field: SerializeField] private EnemyVitality Vitality { get; set; }
        [field: SerializeField] private EnemyMergeState Merge { get; set; }

        private void Awake()
        {
            if (Movement == null)
                Movement = GetComponent<EnemyMovement>();

            if (CoreCollision == null)
                CoreCollision = GetComponent<EnemyCoreCollision>();

            if (RuntimeBehaviors == null)
                RuntimeBehaviors = GetComponent<EnemyRuntimeBehaviors>();

            if (Vitality == null)
                Vitality = GetComponent<EnemyVitality>();

            if (Merge == null)
                Merge = GetComponent<EnemyMergeState>();
        }

        public void PrepareForSpawn()
        {
            ClearRuntimeState();
            Vitality.ResetHealth();
            gameObject.SetActive(true);
        }

        public void PrepareForPool()
        {
            ClearRuntimeState();
            gameObject.SetActive(false);
        }

        private void ClearRuntimeState()
        {
            Merge.Clear(false);
            Movement.Clear();
            CoreCollision.Clear();
            RuntimeBehaviors.Clear();
        }
    }
}
