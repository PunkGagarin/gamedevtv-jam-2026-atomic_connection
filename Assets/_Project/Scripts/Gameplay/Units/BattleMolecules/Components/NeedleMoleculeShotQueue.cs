using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    public class NeedleMoleculeShotQueue : BattleMoleculeShotQueue
    {
        public override bool TryRequestShot(Vector3 direction)
        {
            return TryRequestShot(direction, RequestNeedleShot);
        }

        private void RequestNeedleShot(Vector3 origin, Vector3 direction)
        {
            RequestShot(origin, direction, BattleMoleculeShotKind.Needle);
        }
    }
}
