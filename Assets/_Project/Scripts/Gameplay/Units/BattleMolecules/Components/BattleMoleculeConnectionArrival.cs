using _Project.Scripts.Gameplay.Units;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ObjectRadius))]
    public class BattleMoleculeConnectionArrival : MonoBehaviour
    {
        [field: SerializeField] private ObjectRadius Radius { get; set; }

        private float _minimumArrivalRadius;

        private void Awake()
        {
            if (Radius == null)
                Radius = GetComponent<ObjectRadius>();
        }

        public void Configure(float minimumArrivalRadius)
        {
            _minimumArrivalRadius = Mathf.Max(0f, minimumArrivalRadius);
        }

        public Vector3 PositionFrom(Vector3 fromPosition, float incomingAtomRadius)
        {
            float arrivalRadius = ArrivalRadius(incomingAtomRadius);

            if (arrivalRadius <= 0f)
                return transform.position;

            Vector3 offset = fromPosition - transform.position;
            offset.z = 0f;
            if (offset.sqrMagnitude <= Mathf.Epsilon)
                return transform.position;

            return transform.position + offset.normalized * arrivalRadius;
        }

        public bool IsReached(Vector3 fromPosition, float incomingAtomRadius, float tolerance)
        {
            float arrivalRadius = ArrivalRadius(incomingAtomRadius) + Mathf.Max(0f, tolerance);
            if (arrivalRadius <= 0f)
                return false;

            Vector3 offset = fromPosition - transform.position;
            offset.z = 0f;
            return offset.sqrMagnitude <= arrivalRadius * arrivalRadius;
        }

        private float ArrivalRadius(float incomingAtomRadius)
        {
            float visualArrivalRadius = (Radius != null ? Radius.Radius : 0f) + Mathf.Max(0f, incomingAtomRadius);
            return Mathf.Max(_minimumArrivalRadius, visualArrivalRadius);
        }
    }
}
