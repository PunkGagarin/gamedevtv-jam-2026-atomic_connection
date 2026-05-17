using _Project.Scripts.Gameplay.Units;
using _Project.Scripts.Gameplay.Units.FreeAtoms;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.AtomCores
{
    [RequireComponent(typeof(OwnedAtoms))]
    [RequireComponent(typeof(AtomProductionProgress))]
    public class AtomCore : MonoBehaviour
    {
        [field: SerializeField] public OwnedAtoms OwnedAtoms { get; private set; }
        [field: SerializeField] private AtomProductionProgress ProductionProgress { get; set; }

        private float _spawnRadiusOffset;
        private float _atomOrbitDegreesPerSecond;
        private float _orbitRadius;

        private void Awake()
        {
            if (OwnedAtoms == null)
                OwnedAtoms = GetComponent<OwnedAtoms>();

            if (ProductionProgress == null)
                ProductionProgress = GetComponent<AtomProductionProgress>();
        }

        public void Configure(UnitClickConfig config)
        {
            _spawnRadiusOffset = config.SpawnRadiusOffset;
            _atomOrbitDegreesPerSecond = config.FreeAtomOrbitDegreesPerSecond;
            _orbitRadius = GetColliderRadius(transform);

            ProductionProgress.Configure(config.ClicksToGenerateFreeAtom);
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

            Vector3 offset = freeAtom.transform.position - transform.position;
            float angle = offset.sqrMagnitude > 0.001f
                ? Mathf.Atan2(offset.y, offset.x)
                : 0;

            OwnedAtoms.TakeOwnership(freeAtom, FreeAtomOwnerKind.Core);
            freeAtom.OrbitMotion.Configure(transform, _orbitRadius + GetColliderRadius(freeAtom.transform), angle);
        }

        public void Tick(float deltaTime)
        {
            float angleDelta = _atomOrbitDegreesPerSecond * Mathf.Deg2Rad * deltaTime;
            OwnedAtoms.TickOrbit(angleDelta);
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
