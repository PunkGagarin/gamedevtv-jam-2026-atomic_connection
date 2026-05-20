using System.Collections.Generic;
using UnityEngine;
using Zenject;
using _Project.Scripts.Gameplay.Level;
using _Project.Scripts.Gameplay.Units.AtomCores;
using _Project.Scripts.Infrastructure.AssetManagement;

namespace _Project.Scripts.Gameplay.Enemies.Components
{
    public class RangedEnemyAttack : MonoBehaviour, IEnemyRuntimeBehavior
    {
        private const string PROJECTILE_CONTAINER_NAME = "EnemyProjectiles";

        [field: SerializeField] private Transform TelegraphVisualRoot { get; set; }
        [field: SerializeField] private string ProjectilePrefabResourcePath { get; set; } = "Gameplay/Enemies/EnemyProjectile";
        [field: SerializeField, Min(0f)] private float AttackRange { get; set; } = 3.05f;
        [field: SerializeField, Min(0.01f)] private float AttackInterval { get; set; } = 2.2f;
        [field: SerializeField, Min(0f)] private float InitialAttackDelay { get; set; } = 0.8f;
        [field: SerializeField, Min(0f)] private float TelegraphDuration { get; set; } = 0.18f;
        [field: SerializeField, Min(1f)] private float TelegraphScaleMultiplier { get; set; } = 1.18f;
        [field: SerializeField, Min(0f)] private float ProjectileSpeed { get; set; } = 4f;
        [field: SerializeField, Min(0)] private int ProjectileDamage { get; set; } = 1;
        [field: SerializeField, Min(0f)] private float ProjectileLifetime { get; set; } = 4f;

        private readonly List<EnemyProjectile> _projectiles = new();

        private AtomCore _target;
        private Vector3 _baseTelegraphScale;
        private float _timeToAttack;
        private float _telegraphTimeRemaining;
        private bool _isTelegraphing;

        [Inject] private IAssetProvider _assetProvider;
        [Inject] private IGameplayRuntimeHierarchy _runtimeHierarchy;
        [Inject] private IInstantiator _instantiator;

        private void Awake()
        {
            if (TelegraphVisualRoot == null)
                TelegraphVisualRoot = transform;

            _baseTelegraphScale = TelegraphVisualRoot.localScale;
        }

        private void OnDisable()
        {
            CleanupProjectiles();
            ResetTelegraph();
        }

        private void OnDestroy()
        {
            CleanupProjectiles();
        }

        public void Configure(Transform target)
        {
            _target = target != null && target.TryGetComponent(out AtomCore core) ? core : null;
            ResetAttackTimer(InitialAttackDelay);
            ResetTelegraph();
        }

        public void Tick(float deltaTime)
        {
            TickProjectiles(deltaTime);

            if (_target == null || !_target.IsAlive || deltaTime <= 0f)
                return;

            if (_isTelegraphing)
            {
                TickTelegraph(deltaTime);
                return;
            }

            if (!IsInAttackRange())
                return;

            _timeToAttack -= deltaTime;
            if (_timeToAttack <= 0f)
                BeginTelegraph();
        }

        public void Clear()
        {
            _target = null;
            ResetAttackTimer(0f);
            CleanupProjectiles();
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

            EnemyProjectile prefab = _assetProvider.LoadAsset<EnemyProjectile>(ProjectilePrefabResourcePath);
            if (prefab == null)
            {
                Debug.LogError($"Enemy projectile prefab is missing at Resources path '{ProjectilePrefabResourcePath}'.");
                return;
            }

            EnemyProjectile projectile = _instantiator.InstantiatePrefabForComponent<EnemyProjectile>(
                prefab,
                transform.position,
                Quaternion.identity,
                _runtimeHierarchy.GetOrCreateContainer(PROJECTILE_CONTAINER_NAME));

            if (projectile.Launch(_target, direction, ProjectileSpeed, ProjectileDamage, ProjectileLifetime))
                _projectiles.Add(projectile);
        }

        private bool IsInAttackRange()
        {
            float attackRange = Mathf.Max(0f, AttackRange);
            return Vector2.Distance(transform.position, _target.transform.position) <= attackRange;
        }

        private void TickProjectiles(float deltaTime)
        {
            for (int i = _projectiles.Count - 1; i >= 0; i--)
            {
                EnemyProjectile projectile = _projectiles[i];
                if (projectile == null || !projectile.IsActive)
                {
                    _projectiles.RemoveAt(i);
                    continue;
                }

                projectile.Tick(deltaTime);
            }
        }

        private void CleanupProjectiles()
        {
            foreach (EnemyProjectile projectile in _projectiles)
            {
                if (projectile != null)
                    projectile.DestroySelf();
            }

            _projectiles.Clear();
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
