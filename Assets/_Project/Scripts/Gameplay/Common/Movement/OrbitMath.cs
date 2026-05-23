using UnityEngine;

namespace _Project.Scripts.Gameplay.Common.Movement
{
    public static class OrbitMath
    {
        public static float AngleFromCenter(Vector3 center, Vector3 position, float fallbackAngle = 0f)
        {
            Vector3 offset = position - center;
            offset.z = 0f;
            return offset.sqrMagnitude > Mathf.Epsilon
                ? Mathf.Atan2(offset.y, offset.x)
                : fallbackAngle;
        }

        public static Vector3 PositionOnCircle(Vector3 center, float radius, float angle, float z)
        {
            Vector3 offset = new(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
            Vector3 position = center + offset * radius;
            position.z = z;
            return position;
        }

        public static float AngleDelta(float degreesPerSecond, float deltaTime)
        {
            return degreesPerSecond * Mathf.Deg2Rad * deltaTime;
        }
    }
}
