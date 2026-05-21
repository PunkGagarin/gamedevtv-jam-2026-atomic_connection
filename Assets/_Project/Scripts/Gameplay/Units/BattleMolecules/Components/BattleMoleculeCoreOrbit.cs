using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    public class BattleMoleculeCoreOrbit : MonoBehaviour
    {
        private Transform _center;
        private float _radius;
        private float _angle;
        private float _degreesPerSecond;

        public void Configure(Transform center, float degreesPerSecond)
        {
            _center = center;
            _degreesPerSecond = degreesPerSecond;

            if (_center == null)
                return;

            Vector3 offset = transform.position - _center.position;
            _radius = new Vector2(offset.x, offset.y).magnitude;
            _angle = _radius > Mathf.Epsilon ? Mathf.Atan2(offset.y, offset.x) : 0f;
            SnapToOrbit();
        }

        public void Tick(float deltaTime)
        {
            if (_center == null || _degreesPerSecond <= 0f || deltaTime <= 0f)
                return;

            float angleDelta = _degreesPerSecond * Mathf.Deg2Rad * deltaTime;
            _angle += angleDelta;
            SnapToOrbit();
        }

        private void SnapToOrbit()
        {
            Vector2 offset = new(Mathf.Cos(_angle), Mathf.Sin(_angle));
            Vector2 position = (Vector2)_center.position + offset * _radius;
            transform.position = new Vector3(position.x, position.y, transform.position.z);
        }
    }
}
