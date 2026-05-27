using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    public abstract class TalentPreviewBattleMoleculeAiming : BattleMoleculeAiming
    {
        protected override void OnAimingStarted()
        {
            AimLineVisual?.HideAimPreview();
        }

        protected override void OnAimingMoved(Vector3 origin, Vector3 dragEnd, Vector3 shotDirection)
        {
            if (!CanShowAimPreview(shotDirection))
            {
                AimLineVisual?.HideAimPreview();
                return;
            }

            AimLineVisual?.ShowAimPreview(origin, shotDirection);
        }

        protected override void OnAimingStopped()
        {
            AimLineVisual?.HideAimPreview();
        }

        private bool CanShowAimPreview(Vector3 shotDirection)
        {
            return IsAiming
                   && Charge != null
                   && Charge.IsCharged
                   && AimLineVisual != null
                   && shotDirection.sqrMagnitude > Mathf.Epsilon;
        }
    }
}
