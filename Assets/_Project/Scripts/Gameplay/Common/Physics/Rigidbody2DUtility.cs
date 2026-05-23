using UnityEngine;

namespace _Project.Scripts.Gameplay.Common.Physics
{
    public static class Rigidbody2DUtility
    {
        public static void EnsureKinematicForMovingCollider(GameObject owner)
        {
            if (owner == null || !HasCollider(owner))
                return;

            Rigidbody2D body = owner.GetComponent<Rigidbody2D>();
            if (body != null)
            {
                if (body.bodyType == RigidbodyType2D.Static)
                    body.bodyType = RigidbodyType2D.Kinematic;

                return;
            }

            body = owner.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.interpolation = RigidbodyInterpolation2D.None;
            body.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
        }

        private static bool HasCollider(GameObject owner)
        {
            return owner.GetComponent<Collider2D>() != null
                || owner.GetComponentInChildren<Collider2D>(true) != null;
        }
    }
}
