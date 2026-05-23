using UnityEngine;

namespace _Project.Scripts.Gameplay.Common.Physics
{
    public interface IPhysicsService
    {
        int RaycastNonAlloc(Vector2 worldPosition, Vector2 direction, float maxDistance, int layerMask, RaycastHit2D[] hitBuffer);
        Collider2D OverlapPoint(Vector2 worldPosition, int layerMask);
        int OverlapPointNonAlloc(Vector2 worldPosition, Collider2D[] hits, int layerMask);
        void SyncTransforms();
    }
}
