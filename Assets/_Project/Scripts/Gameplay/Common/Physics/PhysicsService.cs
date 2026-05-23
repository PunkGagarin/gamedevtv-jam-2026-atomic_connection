using UnityEngine;

namespace _Project.Scripts.Gameplay.Common.Physics
{
    public class PhysicsService : IPhysicsService
    {
        private static readonly Collider2D[] OverlapHits = new Collider2D[128];

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            System.Array.Clear(OverlapHits, 0, OverlapHits.Length);
        }

        public int RaycastNonAlloc(Vector2 worldPosition, Vector2 direction, float maxDistance, int layerMask, RaycastHit2D[] hitBuffer) =>
            Physics2D.RaycastNonAlloc(worldPosition, direction, hitBuffer, maxDistance, layerMask);

        public Collider2D OverlapPoint(Vector2 worldPosition, int layerMask)
        {
            int hitCount = Physics2D.OverlapPointNonAlloc(worldPosition, OverlapHits, layerMask);

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hit = OverlapHits[i];
                if (hit != null)
                    return hit;
            }

            return null;
        }

        public int OverlapPointNonAlloc(Vector2 worldPosition, Collider2D[] hits, int layerMask) =>
            Physics2D.OverlapPointNonAlloc(worldPosition, hits, layerMask);

        public void SyncTransforms() =>
            Physics2D.SyncTransforms();
    }
}
