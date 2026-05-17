using System;
using _Project.Scripts.Gameplay.Drag;
using _Project.Scripts.Gameplay.Common.Movement;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.FreeAtoms
{
    [RequireComponent(typeof(OrbitMotion))]
    public class FreeAtom : MonoBehaviour, IDraggable
    {
        private bool _isDragging;

        [field: SerializeField] public OrbitMotion OrbitMotion { get; private set; }

        public bool CanStartDrag => true;
        public Transform Transform => transform;
        public FreeAtomOwnerKind OwnerKind { get; private set; }
        public Transform Owner { get; private set; }
        public bool CanOrbit => !_isDragging;

        public event Action<FreeAtom> Destroyed;
        public event Action<FreeAtom, FreeAtomOwnerKind> OwnerChanged;

        private void Awake()
        {
            if (OrbitMotion == null)
                OrbitMotion = GetComponent<OrbitMotion>();
        }

        private void OnDestroy()
        {
            Destroyed?.Invoke(this);
        }

        public void AssignOwner(FreeAtomOwnerKind ownerKind, Transform owner)
        {
            OwnerKind = ownerKind;
            Owner = owner;
            OwnerChanged?.Invoke(this, OwnerKind);
        }

        public void ClearOwner()
        {
            OwnerKind = FreeAtomOwnerKind.None;
            Owner = null;
            OrbitMotion?.Clear();
            OwnerChanged?.Invoke(this, OwnerKind);
        }

        public void OnDragStart()
        {
            _isDragging = true;
        }

        public void OnDragMove(Vector3 worldPosition)
        {
            transform.position = worldPosition;
        }

        public void OnDragEnd()
        {
            _isDragging = false;
        }

        public void OnDragCancel()
        {
            _isDragging = false;
            OrbitMotion?.SnapToOrbit();
        }
    }
}
