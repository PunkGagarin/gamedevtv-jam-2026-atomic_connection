using _Project.Scripts.Gameplay.Units.BattleMolecules;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    public class MassBattleMoleculeShotQueue : BattleMoleculeShotQueue
    {
        [field: SerializeField, Min(1)] private int ShotCount { get; set; } = 5;
        [field: SerializeField, Min(0f)] private float SpreadDegrees { get; set; } = 30f;

        private static int _nextShotSequenceId = 1;

        public override void Configure(BattleMoleculeConfig config)
        {
            if (config == null)
                return;

            ShotCount = config.MassMoleculeShotCount;
            SpreadDegrees = config.MassMoleculeShotSpreadDegrees;
        }

        public override bool TryRequestShot(Vector3 direction)
        {
            if (!CanRequestShot(direction))
                return false;

            Vector3 origin = transform.position;
            Vector3 shotDirection = NormalizeShotDirection(direction);

            SpendCharge();
            RequestShotgun(origin, shotDirection);

            return true;
        }

        private void RequestShotgun(Vector3 origin, Vector3 centerDirection)
        {
            int shotSequenceId = NextShotSequenceId();
            int shotCount = Mathf.Max(1, ShotCount);
            float spreadDegrees = Mathf.Max(0f, SpreadDegrees);
            float step = shotCount <= 1 ? 0f : spreadDegrees / (shotCount - 1);
            float startAngle = -spreadDegrees * 0.5f;

            for (int i = 0; i < shotCount; i++)
            {
                float angle = startAngle + step * i;
                Vector3 shotDirection = Quaternion.Euler(0f, 0f, angle) * centerDirection;
                RequestShot(origin, shotDirection.normalized, BattleMoleculeShotKind.Mass, shotSequenceId);
            }
        }

        private static int NextShotSequenceId()
        {
            if (_nextShotSequenceId == int.MaxValue)
                _nextShotSequenceId = 1;

            return _nextShotSequenceId++;
        }
    }
}
