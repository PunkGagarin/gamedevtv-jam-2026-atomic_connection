using _Project.Scripts.Gameplay.Common.Physics;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Common.Movement
{
    public class OrbitMotion : MonoBehaviour
    {
        private Transform _center;
        private float _radius;
        private float _angle;

        private bool IsConfigured => _center != null;

        private void Awake()
        {
            Rigidbody2DUtility.EnsureKinematicForMovingCollider(gameObject);
        }

        public void Configure(Transform center, float radius, float angle)
        {
            _center = center;
            _radius = radius;
            _angle = angle;
            SnapToOrbit();
        }

        public void Tick(float angleDelta)
        {
            if (!IsConfigured)
                return;

            _angle += angleDelta;
            SnapToOrbit();
        }

        public void SnapToOrbit()
        {
            if (!IsConfigured)
                return;

            transform.position = OrbitMath.PositionOnCircle(_center.position, _radius, _angle, transform.position.z);
        }

        public void Clear()
        {
            _center = null;
            _radius = 0;
            _angle = 0;
        }
    }
}
