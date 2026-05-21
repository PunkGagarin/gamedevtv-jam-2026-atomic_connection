using _Project.Scripts.Gameplay.Drag;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    [RequireComponent(typeof(BattleMoleculeCharge))]
    [RequireComponent(typeof(BattleMoleculeAimLineView))]
    [RequireComponent(typeof(BattleMoleculeShotQueue))]
    public class BattleMoleculeAiming : MonoBehaviour, IDraggable, IDragReleaseHandler
    {
        [field: SerializeField] protected BattleMoleculeCharge Charge { get; private set; }
        [field: SerializeField] protected BattleMoleculeAimLineView AimLine { get; private set; }
        [field: SerializeField] protected BattleMoleculeShotQueue ShotQueue { get; private set; }

        private bool _isAiming;

        protected bool IsAiming => _isAiming;
        protected BattleMoleculeAimLineView AimLineView => AimLine;
        public bool CanStartDrag => Charge != null && Charge.IsCharged;
        public Transform Transform => transform;

        protected virtual void Awake()
        {
            if (Charge == null)
                Charge = GetComponent<BattleMoleculeCharge>();

            if (AimLine == null)
                AimLine = GetComponent<BattleMoleculeAimLineView>();

            if (ShotQueue == null)
                ShotQueue = GetComponent<BattleMoleculeShotQueue>();
        }

        public virtual void OnDragStart()
        {
            _isAiming = true;
            AimLine?.Show(CurrentAimOrigin());
            OnAimingStarted();
        }

        public virtual void OnDragMove(Vector3 worldPosition)
        {
            if (!_isAiming)
                return;

            Vector3 origin = CurrentAimOrigin();
            Vector3 dragEnd = GetDragEnd(worldPosition);

            AimLine?.SetSegment(origin, dragEnd);
            OnAimingMoved(origin, dragEnd, GetShotDirection(worldPosition));
        }

        public virtual void OnDragEnd()
        {
            StopAiming();
        }

        public virtual void OnDragCancel()
        {
            StopAiming();
        }

        public virtual bool TryHandleDragRelease(Vector3 worldPosition)
        {
            if (Charge == null || ShotQueue == null || !Charge.IsCharged)
                return false;

            Vector3 direction = GetShotDirection(worldPosition);
            if (direction.sqrMagnitude <= Mathf.Epsilon)
                return false;

            return ShotQueue.TryRequestShot(direction.normalized);
        }

        protected virtual void StopAiming()
        {
            _isAiming = false;
            AimLine?.Hide();
            OnAimingStopped();
        }

        protected virtual void OnAimingStarted()
        {
        }

        protected virtual void OnAimingMoved(Vector3 origin, Vector3 dragEnd, Vector3 shotDirection)
        {
        }

        protected virtual void OnAimingStopped()
        {
        }

        protected Vector3 GetDragEnd(Vector3 dragPosition)
        {
            dragPosition.z = CurrentAimOrigin().z;
            return dragPosition;
        }

        protected Vector3 GetShotDirection(Vector3 dragPosition)
        {
            Vector3 origin = CurrentAimOrigin();
            dragPosition.z = origin.z;
            return origin - dragPosition;
        }

        protected Vector3 CurrentAimOrigin()
        {
            return transform.position;
        }
    }
}
