using UnityEngine;

namespace _Project.Scripts.Gameplay.Enemies.Components
{
    public class RangedEnemyStopMovement : EnemyMovement
    {
        [field: SerializeField, Min(0f)] private float StopDistance { get; set; } = 3f;
        [field: SerializeField, Min(0f)] private float StopDistanceJitter { get; set; } = 1.1f;

        private float _currentStopDistance;

        public bool IsStopped { get; private set; }

        public override void Configure(Transform target, float speed)
        {
            base.Configure(target, speed);
            _currentStopDistance = Random.Range(
                Mathf.Max(0f, StopDistance - StopDistanceJitter),
                StopDistance + StopDistanceJitter);
            IsStopped = false;
        }

        public override void Tick(float deltaTime)
        {
            if (Target == null || Speed <= 0 || deltaTime <= 0)
                return;

            Vector3 toTarget = Target.position - transform.position;
            float distance = toTarget.magnitude;
            float stopDistance = Mathf.Max(0f, _currentStopDistance);

            if (distance <= stopDistance || distance <= Mathf.Epsilon)
            {
                IsStopped = true;
                return;
            }

            IsStopped = false;

            float distanceToStop = distance - stopDistance;
            float step = Mathf.Min(Speed * deltaTime, distanceToStop);
            transform.position += toTarget / distance * step;
            IsStopped = step >= distanceToStop;
        }

        public override void Clear()
        {
            base.Clear();
            _currentStopDistance = 0f;
            IsStopped = false;
        }
    }
}
