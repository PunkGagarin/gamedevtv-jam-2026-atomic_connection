using _Project.Scripts.Audio.Domain;
using _Project.Scripts.Gameplay.Drag;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    [RequireComponent(typeof(BattleMoleculeCharge))]
    [RequireComponent(typeof(BattleMoleculeAimLineVisual))]
    [RequireComponent(typeof(BattleMoleculeShotQueue))]
    [RequireComponent(typeof(BattleMoleculePullVisual))]
    public class BattleMoleculeAiming : MonoBehaviour, IDraggable, IDragReleaseHandler
    {
        [field: SerializeField] protected BattleMoleculeCharge Charge { get; private set; }
        [field: SerializeField] protected BattleMoleculeAimLineVisual AimLineVisual { get; private set; }
        [field: SerializeField] protected BattleMoleculeShotQueue ShotQueue { get; private set; }
        [field: SerializeField] private BattleMoleculePullVisual PullVisual { get; set; }
        [field: SerializeField, Min(0.001f)] private float AimSmoothTime { get; set; } = 0.08f;
        [field: SerializeField] private Sounds AimPullSound { get; set; } = Sounds.pew;

        [Inject] private AudioService _audioService;

        private bool _isAiming;
        private Vector3 _smoothDragPosition;
        private Vector3 _smoothDragVelocity;
        private bool _smoothPositionInitialized;

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

            if (PullVisual == null)
                PullVisual = GetComponent<BattleMoleculePullVisual>();
        }

        public virtual void OnDragStart()
        {
            _isAiming = true;
            _smoothPositionInitialized = false;
            PullVisual?.Show();
            _audioService?.PlaySfxWithRandomPitch(AimPullSound);
            OnAimingStarted();
        }

        public virtual void OnDragMove(Vector3 worldPosition)
        {
            if (!_isAiming)
                return;

            if (!_smoothPositionInitialized)
            {
                _smoothDragPosition = worldPosition;
                _smoothDragVelocity = Vector3.zero;
                _smoothPositionInitialized = true;
            }
            else
            {
                _smoothDragPosition = Vector3.SmoothDamp(
                    _smoothDragPosition, worldPosition, ref _smoothDragVelocity, AimSmoothTime);
            }

            Vector3 origin = CurrentAimOrigin();
            Vector3 dragEnd = GetDragEnd(_smoothDragPosition);

            PullVisual?.SetMouseWorldPosition(worldPosition);
            OnAimingMoved(origin, dragEnd, GetShotDirection(_smoothDragPosition));
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

            Vector3 dragPosition = _smoothPositionInitialized ? _smoothDragPosition : worldPosition;
            Vector3 direction = GetShotDirection(dragPosition);
            if (direction.sqrMagnitude <= Mathf.Epsilon)
                return false;

            return ShotQueue.TryRequestShot(direction.normalized);
        }

        protected virtual void StopAiming()
        {
            _isAiming = false;
            PullVisual?.Hide();
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
