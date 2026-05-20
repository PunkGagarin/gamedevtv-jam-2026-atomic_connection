using UnityEngine;

namespace _Project.Scripts.Gameplay.Enemies.Components
{
    public class MassEnemyArcMovement : EnemyMovement
    {
        [field: SerializeField, Min(0f)] private float ArcStrength { get; set; } = 0.55f;
        [field: SerializeField, Min(0f)] private float ArcFadeDistance { get; set; } = 1.25f;

        private float _arcSign = 1f;
        public override void ConfigureGroupMovement(float movementSign)
        {
            _arcSign = movementSign < 0f ? -1f : 1f;
        }

        public override void Tick(float deltaTime)
        {
            if (Target == null || Speed <= 0 || deltaTime <= 0)
                return;

            Vector3 toTarget = Target.position - transform.position;
            if (toTarget.sqrMagnitude <= Mathf.Epsilon)
                return;

            float step = Speed * deltaTime;
            if (toTarget.magnitude <= step)
            {
                transform.position = Target.position;
                return;
            }

            Vector3 direction = ArcDirection(toTarget);
            transform.position += direction * step;
        }

        public override void Clear()
        {
            base.Clear();
            _arcSign = 1f;
        }

        private Vector3 ArcDirection(Vector3 toTarget)
        {
            float distance = toTarget.magnitude;
            Vector3 direct = toTarget / distance;
            Vector3 tangent = new Vector3(-direct.y, direct.x, 0f) * _arcSign;
            float fade = ArcFadeDistance <= 0f ? 1f : Mathf.Clamp01(distance / ArcFadeDistance);

            return (direct + tangent * (ArcStrength * fade)).normalized;
        }
    }
}
