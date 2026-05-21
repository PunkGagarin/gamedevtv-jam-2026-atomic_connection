using System;
using System.Collections.Generic;
using _Project.Scripts.Gameplay.Common.Movement;
using _Project.Scripts.Gameplay.Drag;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.FreeAtoms
{
    [RequireComponent(typeof(OrbitMotion))]
    public class FreeAtom : MonoBehaviour, IDraggable
    {
        private readonly List<Collider2D> _colliders = new();
        private bool _isDragging;

        [field: SerializeField] public OrbitMotion OrbitMotion { get; private set; }
        [field: SerializeField] private HoverBehaviour HoverBehaviour { get; set; }

        public bool CanStartDrag => false;
        public Transform Transform => transform;
        public FreeAtomOwnerKind OwnerKind { get; private set; }
        public Transform Owner { get; private set; }
        public bool CanOrbit => !_isDragging;

        public event Action<FreeAtom> Destroyed;
        public event Action<FreeAtom> DespawnRequested;
        public event Action<FreeAtom, FreeAtomOwnerKind> OwnerChanged;

        private void Awake()
        {
            if (OrbitMotion == null)
                OrbitMotion = GetComponent<OrbitMotion>();

            if (HoverBehaviour == null)
                HoverBehaviour = GetComponent<HoverBehaviour>();
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
            RefreshHoverState();
        }

        public void ClearOwner()
        {
            OwnerKind = FreeAtomOwnerKind.None;
            Owner = null;
            OrbitMotion?.Clear();
            OwnerChanged?.Invoke(this, OwnerKind);
            RefreshHoverState();
        }

        public void PrepareForSpawn()
        {
            _isDragging = false;
            SetCollidersEnabled(true);
            gameObject.SetActive(true);
            ClearOwner();
            RefreshHoverState();
        }

        public void PrepareForPool()
        {
            _isDragging = false;
            ClearOwner();
            SetCollidersEnabled(false);
            gameObject.SetActive(false);
            SetHoverEnabled(false);
        }

        public void RequestDespawn()
        {
            DespawnRequested?.Invoke(this);
        }

        public void OnDragStart()
        {
            _isDragging = true;
            RefreshHoverState();
        }

        public void OnDragMove(Vector3 worldPosition)
        {
            transform.position = worldPosition;
        }

        public void OnDragEnd()
        {
            _isDragging = false;
            RefreshHoverState();
        }

        public void OnDragCancel()
        {
            _isDragging = false;
            OrbitMotion?.SnapToOrbit();
            RefreshHoverState();
        }

        private void SetCollidersEnabled(bool isEnabled)
        {
            GetComponentsInChildren(true, _colliders);

            foreach (Collider2D col in _colliders)
                col.enabled = isEnabled;
        }

        private void SetHoverEnabled(bool isEnabled)
        {
            if (HoverBehaviour != null)
                HoverBehaviour.enabled = isEnabled;
        }

        private void RefreshHoverState()
        {
            SetHoverEnabled(CanStartDrag);
        }
    }
}
