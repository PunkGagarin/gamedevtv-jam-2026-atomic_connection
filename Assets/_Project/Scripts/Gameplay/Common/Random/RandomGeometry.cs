using UnityEngine;

namespace _Project.Scripts.Gameplay.Common.Random
{
    public static class RandomGeometry
    {
        public static Vector2 PointInCircle(IRandomService random, float radius)
        {
            radius = Mathf.Max(0f, radius);
            if (random == null || Mathf.Approximately(radius, 0f))
                return Vector2.zero;

            float angle = random.Range(0f, Mathf.PI * 2f);
            float distance = Mathf.Sqrt(random.Range(0f, 1f)) * radius;
            return new Vector2(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance);
        }
    }
}
