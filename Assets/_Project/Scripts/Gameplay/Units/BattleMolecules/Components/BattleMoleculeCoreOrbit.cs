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

        public void Configure(Transform center, float degreesPerSecond)
        {
            _degreesPerSecond = degreesPerSecond;
            OrbitMotion.ConfigureFromCurrentOffset(center);
        }

        public void Tick(float deltaTime)
        {
            if (_degreesPerSecond <= 0f || deltaTime <= 0f)
                return;

            OrbitMotion.Tick(OrbitMath.AngleDelta(_degreesPerSecond, deltaTime));
        }
    }
}
