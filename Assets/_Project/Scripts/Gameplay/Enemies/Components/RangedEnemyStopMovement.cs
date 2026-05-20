using UnityEngine;

namespace _Project.Scripts.Gameplay.Enemies.Components
{
    public class RangedEnemyStopMovement : EnemyMovement
    {
        [field: SerializeField, Min(0f)] private float StopDistance { get; set; } = 3f;
        [field: SerializeField, Min(0f)] private float StopDistanceJitter { get; set; } = 1.1f;

        private float _currentStopDistance;

        public override void Configure(Transform target, float speed)
        {
            base.Configure(target, speed);
            _currentStopDistance = Random.Range(
                Mathf.Max(0f, StopDistance - StopDistanceJitter),
                StopDistance + StopDistanceJitter);
        }

        public override void Tick(float deltaTime)
        {
            if (Target == null || Speed <= 0 || deltaTime <= 0)
                return;

            Vector3 toTarget = Target.position - transform.position;
            float distance = toTarget.magnitude;
            float stopDistance = Mathf.Max(0f, _currentStopDistance);

            if (distance <= stopDistance || distance <= Mathf.Epsilon)
                return;

            float step = Mathf.Min(Speed * deltaTime, distance - stopDistance);
            transform.position += toTarget / distance * step;
        }

        public override void Clear()
        {
            base.Clear();
            _currentStopDistance = 0f;
        }
    }
}
