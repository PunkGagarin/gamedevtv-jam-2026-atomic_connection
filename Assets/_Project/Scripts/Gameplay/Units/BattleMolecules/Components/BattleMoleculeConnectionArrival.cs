using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    [DisallowMultipleComponent]
    public class BattleMoleculeConnectionArrival : MonoBehaviour
    {
        private float _arrivalRadius;

        public void Configure(float arrivalRadius)
        {
            _arrivalRadius = Mathf.Max(0f, arrivalRadius);
        }

        public Vector3 PositionFrom(Vector3 fromPosition)
        {
            if (_arrivalRadius <= 0f)
                return transform.position;

            Vector3 offset = fromPosition - transform.position;
            offset.z = 0f;
            if (offset.sqrMagnitude <= Mathf.Epsilon)
                return transform.position;

            return transform.position + offset.normalized * _arrivalRadius;
        }
    }
}
