using UnityEngine;

using _Project.Scripts.Gameplay.Enemies.Components;

namespace _Project.Scripts.Gameplay.Enemies
{
    internal sealed class ActiveEnemyMergeLink
    {
        private const float MIN_TETHER_DISTANCE = 0.001f;
        private const float TETHER_PAIR_OFFSET_SHARE = 0.5f;
        private const float NOISE_HASH_SCALE = 43758.5453f;

        private readonly EnemyUnit _first;
        private readonly EnemyUnit _second;
        private readonly EnemyMergeLinkVisual _visual;
        private readonly float _tugPhase;
        private readonly float _tugFrequency01;

        public ActiveEnemyMergeLink(EnemyUnit first, EnemyUnit second, EnemyMergeLinkVisual visual)
        {
            _first = first;
            _second = second;
            _visual = visual;
            _tugPhase = PairNoise(13.37f) * Mathf.PI * 2f;
            _tugFrequency01 = PairNoise(91.73f);
        }

        public bool IsAlive => _first != null && _second != null && _first.IsMergeLinked && _second.IsMergeLinked && _visual != null;

        public bool Contains(EnemyUnit enemy)
        {
            return _first == enemy || _second == enemy;
        }

        public void Tick(float deltaTime, float elapsedSeconds, EnemyMergeConfig config)
        {
            ApplyTetherMotion(deltaTime, elapsedSeconds, config);
            _visual.Tick();
        }

        public void DestroyVisual()
        {
            if (_visual != null)
                Object.Destroy(_visual.gameObject);
        }

        private void ApplyTetherMotion(float deltaTime, float elapsedSeconds, EnemyMergeConfig config)
        {
            if (deltaTime <= 0f || config == null)
                return;

            Vector3 delta = _second.transform.position - _first.transform.position;
            float distance = delta.magnitude;

            if (distance <= MIN_TETHER_DISTANCE)
                return;

            Vector3 direction = delta / distance;
            float tugSignal = CalculateTugSignal(config, elapsedSeconds);
            float targetDistance = CalculateTargetDistance(distance, tugSignal, config);
            ApplyDistanceCorrection(direction, distance, targetDistance, deltaTime, config);
            ApplyContestTug(direction, tugSignal, deltaTime, config);
        }

        private float CalculateTugSignal(EnemyMergeConfig config, float elapsedSeconds)
        {
            float tugFrequency = Mathf.Lerp(
                config.MergeTetherTugMinFrequency,
                Mathf.Max(config.MergeTetherTugMinFrequency, config.MergeTetherTugMaxFrequency),
                _tugFrequency01);

            return Mathf.Sin(elapsedSeconds * tugFrequency + _tugPhase);
        }

        private float CalculateTargetDistance(float distance, float tugSignal, EnemyMergeConfig config)
        {
            float minimumDistance = Mathf.Max(0f, config.MergeTetherMinimumDistance);
            float preferredDistance = Mathf.Max(minimumDistance, config.MergeTetherPreferredDistance);
            float deadZone = Mathf.Max(0f, config.MergeTetherDeadZone);
            float tug = tugSignal * config.MergeTetherTugAmplitude;
            float tuggedPreferredDistance = Mathf.Max(minimumDistance, preferredDistance + tug);

            if (distance > tuggedPreferredDistance + deadZone)
                return tuggedPreferredDistance;

            if (distance < minimumDistance - deadZone)
                return minimumDistance;

            return distance;
        }

        private void ApplyDistanceCorrection(Vector3 direction, float distance, float targetDistance, float deltaTime, EnemyMergeConfig config)
        {
            float distanceDelta = distance - targetDistance;

            if (Mathf.Approximately(distanceDelta, 0f))
                return;

            Vector3 pull = direction * (distanceDelta * config.MergeTetherPullStrength * deltaTime * TETHER_PAIR_OFFSET_SHARE);
            _first.transform.position += pull;
            _second.transform.position -= pull;
        }

        private void ApplyContestTug(Vector3 direction, float tugSignal, float deltaTime, EnemyMergeConfig config)
        {
            float tugAmount = Mathf.Abs(tugSignal) * config.MergeTetherTugAmplitude * deltaTime;

            if (tugAmount <= 0f)
                return;

            Vector3 winnerDrift = direction * (tugAmount * config.MergeTetherContestWinnerDrift);
            Vector3 loserPull = direction * (tugAmount * config.MergeTetherContestLoserPull);

            if (tugSignal >= 0f)
            {
                _first.transform.position -= winnerDrift;
                _second.transform.position -= loserPull;
            }
            else
            {
                _first.transform.position += loserPull;
                _second.transform.position += winnerDrift;
            }
        }

        private float PairNoise(float salt)
        {
            int firstId = _first != null ? _first.GetInstanceID() : 0;
            int secondId = _second != null ? _second.GetInstanceID() : 0;
            float value = Mathf.Sin(firstId * 12.9898f + secondId * 78.233f + salt) * NOISE_HASH_SCALE;
            return value - Mathf.Floor(value);
        }
    }
}
