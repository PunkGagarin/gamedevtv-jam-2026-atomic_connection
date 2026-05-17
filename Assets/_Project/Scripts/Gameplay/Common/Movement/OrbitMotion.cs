using UnityEngine;

namespace _Project.Scripts.Gameplay.Common.Movement
{
    public class OrbitMotion : MonoBehaviour
    {
        private Transform _center;
        private float _radius;
        private float _angle;

        public bool IsConfigured => _center != null;

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

            Vector3 offset = new Vector3(Mathf.Cos(_angle), Mathf.Sin(_angle), 0) * _radius;
            transform.position = _center.position + offset;
        }

        public void Clear()
        {
            _center = null;
            _radius = 0;
            _angle = 0;
        }
    }
}
