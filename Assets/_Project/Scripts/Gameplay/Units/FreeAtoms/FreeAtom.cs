using System;
using _Project.Scripts.Gameplay.Common.Movement;
using _Project.Scripts.Gameplay.Common.Physics;
using _Project.Scripts.Gameplay.Drag;
using _Project.Scripts.Gameplay.Units.FreeAtoms.Components;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.FreeAtoms
{
    [RequireComponent(typeof(OrbitMotion))]
    [RequireComponent(typeof(ColliderSet))]
    [RequireComponent(typeof(InitialLocalScale))]
    [RequireComponent(typeof(FreeAtomOwnership))]
    [RequireComponent(typeof(FreeAtomState))]
    [RequireComponent(typeof(FreeAtomDrag))]
    [RequireComponent(typeof(FreeAtomLifetime))]
    [RequireComponent(typeof(FreeAtomLifecycle))]
    public class FreeAtom : MonoBehaviour
    {
        [field: SerializeField] private OrbitMotion OrbitMotion { get; set; }
        [field: SerializeField] private ColliderSet ColliderSet { get; set; }
        [field: SerializeField] private FreeAtomOwnership Ownership { get; set; }
        [field: SerializeField] private FreeAtomState State { get; set; }
        [field: SerializeField] private FreeAtomDrag Drag { get; set; }
        [field: SerializeField] private FreeAtomLifetime Lifetime { get; set; }
        [field: SerializeField] private FreeAtomLifecycle Lifecycle { get; set; }

        public IDraggable Draggable => Drag;
        public Transform Transform => transform;
        public FreeAtomOwnerKind OwnerKind => Ownership.OwnerKind;
        public Transform Owner => Ownership.Owner;
        public bool CanOrbit => State.CanOrbit;
        public bool IsInConnectionFlow => State.IsInConnectionFlow;
        public bool CanArrangeInOrbit => State.CanArrangeInOrbit;

        public event Action<FreeAtom> Destroyed
        {
            add => Lifetime.Destroyed += value;
            remove => Lifetime.Destroyed -= value;
        }

        public event Action<FreeAtom> DespawnRequested
        {
            add => Lifetime.DespawnRequested += value;
            remove => Lifetime.DespawnRequested -= value;
        }

        public event Action<FreeAtom, FreeAtomOwnerKind> OwnerChanged
        {
            add => Ownership.OwnerChanged += value;
            remove => Ownership.OwnerChanged -= value;
        }

        private void Awake()
        {
            OrbitMotion = GetComponent<OrbitMotion>();
            ColliderSet = GetComponent<ColliderSet>();
            Ownership = GetComponent<FreeAtomOwnership>();
            State = GetComponent<FreeAtomState>();
            Drag = GetComponent<FreeAtomDrag>();
            Lifetime = GetComponent<FreeAtomLifetime>();
            Lifecycle = GetComponent<FreeAtomLifecycle>();
        }

        public void AssignOwner(FreeAtomOwnerKind ownerKind, Transform owner)
        {
            Ownership.AssignOwner(ownerKind, owner);
        }

        public void ClearOwner()
        {
            Ownership.ClearOwner();
        }

        public void PrepareForSpawn()
        {
            Lifecycle.PrepareForSpawn();
        }

        public void PrepareForPool()
        {
            Lifecycle.PrepareForPool();
        }

        public void RequestDespawn()
        {
            Lifetime.RequestDespawn();
        }

        public void BeginConnectionFlow()
        {
            State.BeginConnectionFlow();
        }

        public void EndConnectionFlow()
        {
            State.EndConnectionFlow();
        }

        public void SetCollisionEnabled(bool isEnabled)
        {
            ColliderSet.SetEnabled(isEnabled);
        }

        public void ConfigureOrbit(Transform center, float radius, float angle)
        {
            OrbitMotion.Configure(center, radius, angle);
        }

        public void TickOrbit(float angleDelta)
        {
            OrbitMotion.Tick(angleDelta);
        }
    }
}
