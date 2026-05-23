using UnityEngine;
using Zenject;

using _Project.Scripts.Gameplay.Talents;
using _Project.Scripts.Gameplay.Units.BattleMolecules;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    public class SwarmMoleculeShotQueue : BattleMoleculeShotQueue
    {
        [field: SerializeField, Min(1)] private int ShotCount { get; set; } = 5;
        [field: SerializeField, Min(0f)] private float SpreadDegrees { get; set; } = 30f;
        [field: SerializeField, Min(0f)] private float SpreadDegreesPerShotCountBonus { get; set; } = 10f;

        private static int _nextShotSequenceId = 1;

        [Inject] private ITalentService _talentService;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            _nextShotSequenceId = 1;
        }

        public override void Configure(BattleMoleculeConfig config)
        {
            if (config == null)
                return;

            ShotCount = config.SwarmMoleculeShotCount;
            SpreadDegrees = config.SwarmMoleculeShotSpreadDegrees;
            SpreadDegreesPerShotCountBonus = config.SwarmMoleculeShotSpreadDegreesPerShotCountBonus;
        }

        public override bool TryRequestShot(Vector3 direction)
        {
            return TryRequestShot(direction, RequestShotgun);
        }

        private void RequestShotgun(Vector3 origin, Vector3 centerDirection)
        {
            int shotSequenceId = NextShotSequenceId();
            int bonusShotCount = _talentService != null
                ? Mathf.Max(0, Mathf.RoundToInt(_talentService.BonusOf(TalentType.SwarmMoleculeShotCount)))
                : 0;
            int shotCount = Mathf.Max(1, ShotCount + bonusShotCount);
            float spreadDegrees = Mathf.Max(0f, SpreadDegrees + bonusShotCount * SpreadDegreesPerShotCountBonus);
            float step = shotCount <= 1 ? 0f : spreadDegrees / (shotCount - 1);
            float startAngle = -spreadDegrees * 0.5f;

            for (int i = 0; i < shotCount; i++)
            {
                float angle = startAngle + step * i;
                Vector3 shotDirection = Quaternion.Euler(0f, 0f, angle) * centerDirection;
                RequestShot(origin, shotDirection.normalized, BattleMoleculeShotKind.Swarm, shotSequenceId);
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
