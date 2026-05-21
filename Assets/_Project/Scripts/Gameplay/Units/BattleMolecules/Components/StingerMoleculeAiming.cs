using UnityEngine;
using Zenject;
using _Project.Scripts.Gameplay.Talents;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    public class StingerMoleculeAiming : BattleMoleculeAiming
    {
        [Inject] private ITalentService _talentService;

        protected override void OnAimingStarted()
        {
            AimLineView?.HideAimPreview();
        }

        protected override void OnAimingMoved(Vector3 origin, Vector3 dragEnd, Vector3 shotDirection)
        {
            if (!CanShowAimPreview(shotDirection))
            {
                AimLineView?.HideAimPreview();
                return;
            }

            AimLineView?.ShowAimPreview(origin, shotDirection);
        }

        protected override void OnAimingStopped()
        {
            AimLineView?.HideAimPreview();
        }

        private bool CanShowAimPreview(Vector3 shotDirection)
        {
            return IsAiming
                   && Charge != null
                   && Charge.IsCharged
                   && AimLineView != null
                   && shotDirection.sqrMagnitude > Mathf.Epsilon
                   && _talentService != null
                   && _talentService.IsUnlocked(TalentType.StingerMoleculeAim);
        }
    }
}
