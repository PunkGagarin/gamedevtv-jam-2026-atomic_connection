using _Project.Scripts.Gameplay.Drag;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    [RequireComponent(typeof(BattleMoleculeCharge))]
    [RequireComponent(typeof(BattleMoleculeAimLineView))]
    [RequireComponent(typeof(BattleMoleculeShotLogger))]
    public class BattleMoleculeAiming : MonoBehaviour, IDraggable, IDragReleaseHandler
    {
        [field: SerializeField] private BattleMoleculeCharge Charge { get; set; }
        [field: SerializeField] private BattleMoleculeAimLineView AimLine { get; set; }
        [field: SerializeField] private BattleMoleculeShotLogger ShotLogger { get; set; }

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

            if (ShotLogger == null)
                ShotLogger = GetComponent<BattleMoleculeShotLogger>();
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

            AimLine.SetEnd(worldPosition);
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

            Vector3 direction = worldPosition - _aimOrigin;
            if (direction.sqrMagnitude <= Mathf.Epsilon)
                return false;

            ShotLogger.LogShot(direction.normalized);
            return true;
        }

        private void StopAiming()
        {
            _isAiming = false;
            AimLine.Hide();
        }
    }
}
