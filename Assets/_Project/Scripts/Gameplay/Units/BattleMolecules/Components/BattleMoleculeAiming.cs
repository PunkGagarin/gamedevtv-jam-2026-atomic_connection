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
            AimLine.Show(CurrentAimOrigin());
        }

        public void OnDragMove(Vector3 worldPosition)
        {
            if (!_isAiming)
                return;

            AimLine.SetSegment(CurrentAimOrigin(), GetDragEnd(worldPosition));
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

        private Vector3 GetDragEnd(Vector3 dragPosition)
        {
            dragPosition.z = CurrentAimOrigin().z;
            return dragPosition;
        }

        private Vector3 GetShotDirection(Vector3 dragPosition)
        {
            Vector3 origin = CurrentAimOrigin();
            dragPosition.z = origin.z;
            return origin - dragPosition;
        }

        private Vector3 CurrentAimOrigin()
        {
            return transform.position;
        }
    }
}
