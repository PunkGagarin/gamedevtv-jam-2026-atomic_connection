using _Project.Scripts.Gameplay.Units;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    [RequireComponent(typeof(OwnedAtoms))]
    public class BattleMoleculeAtomOrbit : MonoBehaviour
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
            float angleDelta = _degreesPerSecond * Mathf.Deg2Rad * deltaTime;
            OwnedAtoms.TickOrbit(angleDelta);
        }
    }
}
