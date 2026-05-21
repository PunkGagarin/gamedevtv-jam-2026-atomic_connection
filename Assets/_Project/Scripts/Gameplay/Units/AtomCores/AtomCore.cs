using System;
using _Project.Scripts.Gameplay.Common.Health;
using _Project.Scripts.Gameplay.Units.AtomCores.Components;
using _Project.Scripts.Gameplay.Units.FreeAtoms;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.AtomCores
{
    [RequireComponent(typeof(OwnedAtoms))]
    [RequireComponent(typeof(OwnedAtomOrbitLayout))]
    [RequireComponent(typeof(AtomProductionProgress))]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(AtomCoreClickInteraction))]
    public class AtomCore : MonoBehaviour
    {
        [field: SerializeField] public OwnedAtoms OwnedAtoms { get; private set; }
        [field: SerializeField] private OwnedAtomOrbitLayout AtomOrbitLayout { get; set; }
        [field: SerializeField] private AtomProductionProgress ProductionProgress { get; set; }
        [field: SerializeField] private Health Health { get; set; }
        [field: SerializeField] private AtomCoreClickInteraction ClickInteraction { get; set; }
        [field: SerializeField] private AtomCoreShield Shield { get; set; }

        private float _spawnRadiusOffset;
        private float _atomOrbitDegreesPerSecond;
        private float _orbitRadius;

        public bool IsAlive => Health == null || Health.IsAlive;

        public event Action Died
        {
            add
            {
                if (Health != null)
                    Health.Died += value;
            }
            remove
            {
                if (Health != null)
                    Health.Died -= value;
            }
        }

        private void Awake()
        {
            if (OwnedAtoms == null)
                OwnedAtoms = GetComponent<OwnedAtoms>();

            if (AtomOrbitLayout == null)
                AtomOrbitLayout = GetComponent<OwnedAtomOrbitLayout>();

            if (ProductionProgress == null)
                ProductionProgress = GetComponent<AtomProductionProgress>();

            if (Health == null)
                Health = GetComponent<Health>();

            if (ClickInteraction == null)
                ClickInteraction = GetComponent<AtomCoreClickInteraction>();

            if (Shield == null)
                Shield = GetComponent<AtomCoreShield>();
        }

        public void Configure(AtomCoreConfig config, int clicksRequired, int maxHealth)
        {
            _spawnRadiusOffset = config.SpawnRadiusOffset;
            _atomOrbitDegreesPerSecond = config.FreeAtomOrbitDegreesPerSecond;
            _orbitRadius = GetColliderRadius(transform);

            Health?.Configure(maxHealth);
            ProductionProgress.Configure(clicksRequired);
            AtomOrbitLayout?.ConfigureOwnerPlusAtomRadius(FreeAtomOwnerKind.Core, _orbitRadius);
        }

        public bool RegisterAtomClick()
        {
            return ProductionProgress.RegisterClick();
        }

        public Vector3 GetAtomSpawnPosition(float angle, float radius01)
        {
            Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * (radius01 * _spawnRadiusOffset);
            return transform.position + offset;
        }

        public void TakeGeneratedAtom(FreeAtom freeAtom)
        {
            if (freeAtom == null || freeAtom.OrbitMotion == null)
                return;

            OwnedAtoms.TakeOwnership(freeAtom, FreeAtomOwnerKind.Core);
        }

        public void Tick(float deltaTime)
        {
            ClickInteraction?.Tick(deltaTime);
            Shield?.Tick(deltaTime);

            float angleDelta = _atomOrbitDegreesPerSecond * Mathf.Deg2Rad * deltaTime;
            OwnedAtoms.TickOrbit(angleDelta);
        }

        public void TakeDamage(int amount)
        {
            if (Shield != null && Shield.TryAbsorbDamage(amount))
                return;

            Health?.TakeDamage(amount);
            AtomCoreEventBus.RiseOnDamageEvent(amount);
        }

        public void Kill()
        {
            Health?.Kill();
        }

        public void CleanupAtoms()
        {
            OwnedAtoms.ReleaseAll();
        }

        private float GetColliderRadius(Transform target)
        {
            Collider2D col = target.GetComponent<Collider2D>();

            if (col == null)
                return 0;

            Vector3 extents = col.bounds.extents;
            return Mathf.Max(extents.x, extents.y);
        }
    }
}
