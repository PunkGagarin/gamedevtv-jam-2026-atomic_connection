using _Project.Scripts.Gameplay.Drag;
using _Project.Scripts.Gameplay.Units;
using _Project.Scripts.Gameplay.Units.FreeAtoms;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules
{
    [RequireComponent(typeof(OwnedAtoms))]
    public class BattleMolecule : MonoBehaviour, IDropTarget
    {
        [field: SerializeField] private OwnedAtoms OwnedAtoms { get; set; }

        private int _atomsRequired;
        private float _atomsPosCircleRadius;
        private float _depositedAtomsOrbitDegreesPerSecond;

        private void Awake()
        {
            if (OwnedAtoms == null)
                OwnedAtoms = GetComponent<OwnedAtoms>();
        }

        public void Configure(BattleMoleculeConfig config)
        {
            _atomsRequired = config.AtomsRequired;
            _atomsPosCircleRadius = config.AtomsPosCircleRadius;
            _depositedAtomsOrbitDegreesPerSecond = config.DepositedAtomsOrbitDegreesPerSecond;
        }

        public bool CanAcceptDrop(IDraggable draggable)
        {
            if (_atomsRequired <= 0)
                return false;

            return draggable is FreeAtom && OwnedAtoms.Count < _atomsRequired;
        }

        public void OnDropAccepted(IDraggable draggable)
        {
            if (draggable is not FreeAtom freeAtom)
                return;

            GameObject freeAtomObject = freeAtom.gameObject;

            Collider2D col = freeAtomObject.GetComponent<Collider2D>();
            if (col != null)
                col.enabled = false;

            float angle = GetCircleAngle(OwnedAtoms.Count, _atomsRequired);
            OwnedAtoms.TakeOwnership(freeAtom, FreeAtomOwnerKind.BattleMolecule);
            freeAtom.OrbitMotion?.Configure(transform, _atomsPosCircleRadius, angle);

            if (OwnedAtoms.Count >= _atomsRequired)
                Fire();
        }

        public void Tick(float deltaTime)
        {
            float angleDelta = _depositedAtomsOrbitDegreesPerSecond * Mathf.Deg2Rad * deltaTime;
            OwnedAtoms.TickOrbit(angleDelta);
        }

        private void Fire()
        {
            Debug.Log("Boom");
            OwnedAtoms.DestroyAll();
        }

        private float GetCircleAngle(int index, int total)
        {
            return (index / (float)total) * Mathf.PI * 2f;
        }

        public void OnDropRejected(IDraggable draggable)
        {
        }
    }
}
