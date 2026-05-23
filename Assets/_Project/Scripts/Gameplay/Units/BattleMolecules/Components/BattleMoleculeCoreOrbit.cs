using _Project.Scripts.Gameplay.Common.Movement;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    [RequireComponent(typeof(OrbitMotion))]
    public class BattleMoleculeCoreOrbit : MonoBehaviour
    {
        [field: SerializeField] private OrbitMotion OrbitMotion { get; set; }

        private float _degreesPerSecond;

        private void Awake()
        {
            if (OrbitMotion == null)
                OrbitMotion = GetComponent<OrbitMotion>();
        }

        public void Configure(Transform center, float radius, float degreesPerSecond)
        {
            _degreesPerSecond = degreesPerSecond;

            if (center == null)
            {
                OrbitMotion.Clear();
                return;
            }

            float angle = OrbitMath.AngleFromCenter(center.position, transform.position);
            OrbitMotion.Configure(center, Mathf.Max(0f, radius), angle);
        }

        public void Tick(float deltaTime)
        {
            if (_degreesPerSecond <= 0f || deltaTime <= 0f)
                return;

            OrbitMotion.Tick(OrbitMath.AngleDelta(_degreesPerSecond, deltaTime));
        }
    }
}
