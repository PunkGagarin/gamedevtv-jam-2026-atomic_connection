using UnityEngine;
using Zenject;
using _Project.Scripts.Gameplay.Enemies;
using _Project.Scripts.Gameplay.Units.AtomCores;

namespace _Project.Scripts.Gameplay.Enemies.Components
{
    [RequireComponent(typeof(RangedEnemyStopMovement))]
    public class RangedEnemyAttack : MonoBehaviour, IEnemyRuntimeBehavior
    {
        [field: SerializeField] private Transform TelegraphVisualRoot { get; set; }
        [field: SerializeField] private string ProjectilePrefabResourcePath { get; set; } = "Gameplay/Enemies/EnemyProjectile";
        [field: SerializeField, Min(0.01f)] private float AttackInterval { get; set; } = 2.2f;
        [field: SerializeField, Min(0f)] private float InitialAttackDelay { get; set; } = 0.8f;
        [field: SerializeField, Min(0f)] private float TelegraphDuration { get; set; } = 0.18f;
        [field: SerializeField, Min(1f)] private float TelegraphScaleMultiplier { get; set; } = 1.18f;
        [field: SerializeField, Min(0f)] private float ProjectileSpeed { get; set; } = 4f;
        [field: SerializeField, Min(0)] private int ProjectileDamage { get; set; } = 1;
        [field: SerializeField, Min(0f)] private float ProjectileLifetime { get; set; } = 4f;

        private AtomCore _target;
        private Vector3 _baseTelegraphScale;
        private float _timeToAttack;
        private float _telegraphTimeRemaining;
        private bool _isTelegraphing;
        private RangedEnemyStopMovement _stopMovement;

        [Inject] private IEnemyProjectileService _projectileService;

        private void Awake()
        {
            if (TelegraphVisualRoot == null)
                TelegraphVisualRoot = transform;

            _stopMovement = GetComponent<RangedEnemyStopMovement>();
            _baseTelegraphScale = TelegraphVisualRoot.localScale;
        }

        private void OnDisable()
        {
            _projectileService?.CleanupOwner(this);
            ResetTelegraph();
        }

        private void OnDestroy()
        {
            _projectileService?.CleanupOwner(this);
        }

        public void Configure(Transform target)
        {
            _target = target != null && target.TryGetComponent(out AtomCore core) ? core : null;
            ResetAttackTimer(InitialAttackDelay);
            ResetTelegraph();
        }

        public void Tick(float deltaTime)
        {
            if (_target == null || !_target.IsAlive || deltaTime <= 0f)
                return;

            if (_isTelegraphing)
            {
                TickTelegraph(deltaTime);
                return;
            }

            if (_stopMovement == null || !_stopMovement.IsStopped)
                return;

            _timeToAttack -= deltaTime;
            if (_timeToAttack <= 0f)
                BeginTelegraph();
        }

        public void Clear()
        {
            _target = null;
            ResetAttackTimer(0f);
            _projectileService?.CleanupOwner(this);
            ResetTelegraph();
        }

        private void TickTelegraph(float deltaTime)
        {
            _telegraphTimeRemaining -= deltaTime;
            UpdateTelegraphScale();

            if (_telegraphTimeRemaining > 0f)
                return;

            ResetTelegraph();
            Shoot();
            ResetAttackTimer(AttackInterval);
        }

        private void BeginTelegraph()
        {
            if (TelegraphDuration <= 0f)
            {
                Shoot();
                ResetAttackTimer(AttackInterval);
                return;
            }

            _isTelegraphing = true;
            _telegraphTimeRemaining = TelegraphDuration;
            UpdateTelegraphScale();
        }

        private void Shoot()
        {
            if (_target == null || !_target.IsAlive)
                return;

            Vector3 direction = _target.transform.position - transform.position;
            if (direction.sqrMagnitude <= Mathf.Epsilon)
                return;

            _projectileService?.Shoot(new EnemyProjectileShot(
                this,
                ProjectilePrefabResourcePath,
                transform.position,
                _target,
                direction,
                ProjectileSpeed,
                ProjectileDamage,
                ProjectileLifetime));
        }

        private void UpdateTelegraphScale()
        {
            if (TelegraphVisualRoot == null)
                return;

            float duration = Mathf.Max(0.01f, TelegraphDuration);
            float normalizedTime = Mathf.Clamp01(1f - _telegraphTimeRemaining / duration);
            float pulse = Mathf.Sin(normalizedTime * Mathf.PI);
            TelegraphVisualRoot.localScale = Vector3.Lerp(
                _baseTelegraphScale,
                _baseTelegraphScale * TelegraphScaleMultiplier,
                pulse);
        }

        private void ResetTelegraph()
        {
            _isTelegraphing = false;
            _telegraphTimeRemaining = 0f;

            if (TelegraphVisualRoot != null)
                TelegraphVisualRoot.localScale = _baseTelegraphScale;
        }

        private void ResetAttackTimer(float seconds)
        {
            _timeToAttack = Mathf.Max(0f, seconds);
        }
    }
}
