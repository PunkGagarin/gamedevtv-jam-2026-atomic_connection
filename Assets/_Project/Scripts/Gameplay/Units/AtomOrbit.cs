using _Project.Scripts.Gameplay.Common.Movement;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(OwnedAtoms))]
    public class AtomOrbit : MonoBehaviour
    {
        [field: SerializeField] private OwnedAtoms OwnedAtoms { get; set; }

        private float _degreesPerSecond;

        private void Awake()
        {
            if (OwnedAtoms == null)
                OwnedAtoms = GetComponent<OwnedAtoms>();
        }

        public void Configure(float degreesPerSecond)
        {
            _degreesPerSecond = degreesPerSecond;
        }

        public void Tick(float deltaTime)
        {
            if (OwnedAtoms == null)
                return;

            OwnedAtoms.TickOrbit(OrbitMath.AngleDelta(_degreesPerSecond, deltaTime));
        }
    }
}
