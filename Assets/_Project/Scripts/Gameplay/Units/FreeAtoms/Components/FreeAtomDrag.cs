using _Project.Scripts.Gameplay.Common.Movement;
using _Project.Scripts.Gameplay.Drag;
using _Project.Scripts.Gameplay.Units.FreeAtoms;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.FreeAtoms.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(FreeAtomState))]
    [RequireComponent(typeof(OrbitMotion))]
    public class FreeAtomDrag : MonoBehaviour, IDraggable
    {
        [field: SerializeField] public FreeAtom Atom { get; private set; }
        [field: SerializeField] private FreeAtomState State { get; set; }
        [field: SerializeField] private OrbitMotion OrbitMotion { get; set; }

        public bool CanStartDrag => State != null && State.CanStartDrag;
        public Transform Transform => transform;

        private void Awake()
        {
            if (Atom == null)
                Atom = GetComponent<FreeAtom>();

            if (State == null)
                State = GetComponent<FreeAtomState>();

            if (OrbitMotion == null)
                OrbitMotion = GetComponent<OrbitMotion>();
        }

        public void OnDragStart()
        {
            State?.BeginDrag();
        }

        public void OnDragMove(Vector3 worldPosition)
        {
            transform.position = worldPosition;
        }

        public void OnDragEnd()
        {
            State?.EndDrag();
        }

        public void OnDragCancel()
        {
            State?.EndDrag();
            OrbitMotion?.SnapToOrbit();
        }
    }
}
