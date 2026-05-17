using _Project.Scripts.Gameplay.Drag;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    [RequireComponent(typeof(BattleMoleculeCharge))]
    [RequireComponent(typeof(BattleMoleculeAimLineView))]
    [RequireComponent(typeof(BattleMoleculeShotQueue))]
    public class BattleMoleculeAiming : MonoBehaviour, IDraggable, IDragReleaseHandler
    {
        [field: SerializeField] private BattleMoleculeCharge Charge { get; set; }
        [field: SerializeField] private BattleMoleculeAimLineView AimLine { get; set; }
        [field: SerializeField] private BattleMoleculeShotQueue ShotQueue { get; set; }

        private Vector3 _aimOrigin;
        private bool _isAiming;

        public bool CanStartDrag => Charge.IsCharged;
        public Transform Transform => transform;

        private void Awake()
        {
            if (Charge == null)
                Charge = GetComponent<BattleMoleculeCharge>();

            if (AimLine == null)
                AimLine = GetComponent<BattleMoleculeAimLineView>();

            if (ShotQueue == null)
                ShotQueue = GetComponent<BattleMoleculeShotQueue>();
        }

        public void OnDragStart()
        {
            _isAiming = true;
            _aimOrigin = transform.position;
            AimLine.Show(_aimOrigin);
        }

        public void OnDragMove(Vector3 worldPosition)
        {
            if (!_isAiming)
                return;

            AimLine.SetEnd(GetShotEnd(worldPosition));
        }

        public void OnDragEnd()
        {
            StopAiming();
        }

        public void OnDragCancel()
        {
            StopAiming();
        }

        public bool TryHandleDragRelease(Vector3 worldPosition)
        {
            if (!Charge.IsCharged)
                return false;

            Vector3 direction = GetShotDirection(worldPosition);
            if (direction.sqrMagnitude <= Mathf.Epsilon)
                return false;

            return ShotQueue.TryRequestShot(direction.normalized);
        }

        private void StopAiming()
        {
            _isAiming = false;
            AimLine.Hide();
        }

        private Vector3 GetShotEnd(Vector3 dragPosition)
        {
            Vector3 direction = GetShotDirection(dragPosition);
            return _aimOrigin + direction;
        }

        private Vector3 GetShotDirection(Vector3 dragPosition)
        {
            dragPosition.z = _aimOrigin.z;
            return _aimOrigin - dragPosition;
        }
    }
}
