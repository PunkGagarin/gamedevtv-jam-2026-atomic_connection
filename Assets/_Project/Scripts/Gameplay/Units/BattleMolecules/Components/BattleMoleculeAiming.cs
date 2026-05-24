using _Project.Scripts.Audio.Domain;
using _Project.Scripts.Gameplay.Drag;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    [RequireComponent(typeof(BattleMoleculeCharge))]
    [RequireComponent(typeof(BattleMoleculeAimLineVisual))]
    [RequireComponent(typeof(BattleMoleculeShotQueue))]
    public class BattleMoleculeAiming : MonoBehaviour, IDraggable, IDragReleaseHandler
    {
        [field: SerializeField] protected BattleMoleculeCharge Charge { get; private set; }
        [field: SerializeField] protected BattleMoleculeAimLineVisual AimLineVisual { get; private set; }
        [field: SerializeField] protected BattleMoleculeShotQueue ShotQueue { get; private set; }
        [field: SerializeField] private Sounds AimPullSound { get; set; } = Sounds.pew;

        [Inject] private AudioService _audioService;

        private bool _isAiming;

        protected bool IsAiming => _isAiming;
        public bool CanStartDrag => Charge != null && Charge.IsCharged;
        public Transform Transform => transform;

        protected virtual void Awake()
        {
            if (Charge == null)
                Charge = GetComponent<BattleMoleculeCharge>();

            if (AimLineVisual == null)
                AimLineVisual = GetComponent<BattleMoleculeAimLineVisual>();

            if (ShotQueue == null)
                ShotQueue = GetComponent<BattleMoleculeShotQueue>();
        }

        public virtual void OnDragStart()
        {
            _isAiming = true;
            AimLineVisual?.Show(CurrentAimOrigin());
            _audioService?.PlaySfxWithRandomPitch(AimPullSound);
            OnAimingStarted();
        }

        public virtual void OnDragMove(Vector3 worldPosition)
        {
            if (!_isAiming)
                return;

            Vector3 origin = CurrentAimOrigin();
            Vector3 dragEnd = GetDragEnd(worldPosition);

            AimLineVisual?.SetSegment(origin, dragEnd);
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
            AimLineVisual?.Hide();
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
