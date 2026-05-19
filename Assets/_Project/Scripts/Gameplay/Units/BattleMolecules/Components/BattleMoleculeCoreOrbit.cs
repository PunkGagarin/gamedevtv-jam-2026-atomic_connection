using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class BattleMoleculeCoreOrbit : MonoBehaviour
    {
        [field: SerializeField] private Rigidbody2D Body { get; set; }

        private Transform _center;
        private float _radius;
        private float _angle;
        private float _degreesPerSecond;

        private void Awake()
        {
            if (Body == null)
                Body = GetComponent<Rigidbody2D>();
        }

        public void Configure(Transform center, float degreesPerSecond)
        {
            _center = center;
            _degreesPerSecond = degreesPerSecond;

            if (_center == null || Body == null)
                return;

            Vector3 offset = transform.position - _center.position;
            _radius = new Vector2(offset.x, offset.y).magnitude;
            _angle = _radius > Mathf.Epsilon ? Mathf.Atan2(offset.y, offset.x) : 0f;
            SnapToOrbit();
        }

        public void FixedTick(float fixedDeltaTime)
        {
            if (_center == null || Body == null || _degreesPerSecond <= 0f)
                return;

            float angleDelta = _degreesPerSecond * Mathf.Deg2Rad * fixedDeltaTime;
            _angle += angleDelta;
            SnapToOrbit();
        }

        private void SnapToOrbit()
        {
            Vector2 offset = new(Mathf.Cos(_angle), Mathf.Sin(_angle));
            Vector2 position = (Vector2)_center.position + offset * _radius;
            Body.MovePosition(position);
        }
    }
}
