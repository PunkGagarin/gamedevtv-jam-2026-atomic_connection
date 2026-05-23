using System.Collections.Generic;
using _Project.Scripts.Gameplay.Common.Physics;
using _Project.Scripts.Gameplay.Enemies;
using _Project.Scripts.Gameplay.Talents;
using _Project.Scripts.Gameplay.Units.BattleMolecules;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    [RequireComponent(typeof(BattleMoleculeShotQueue))]
    [RequireComponent(typeof(BattleMoleculeAimLineVisual))]
    public abstract class BattleMoleculeAttack : MonoBehaviour
    {
        private static readonly RaycastHit2D[] ShotHits = new RaycastHit2D[64];

        private readonly List<EnemyHit> _enemyHits = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            System.Array.Clear(ShotHits, 0, ShotHits.Length);
        }

        [field: SerializeField] private BattleMoleculeShotQueue ShotQueue { get; set; }
        [field: SerializeField] private BattleMoleculeAimLineVisual AimLineVisual { get; set; }

        [Inject] private IPhysicsService _physicsService;
        [Inject] protected BattleMoleculeConfig Config;
        [Inject] protected ITalentService TalentService;

        protected BattleMoleculeAimLineVisual AimLine => AimLineVisual;
        protected List<EnemyHit> EnemyHits => _enemyHits;
        protected abstract int BaseShotDamage { get; }
        protected abstract TalentType DamageTalentType { get; }

        protected virtual void Awake()
        {
            if (ShotQueue == null)
                ShotQueue = GetComponent<BattleMoleculeShotQueue>();

            if (AimLineVisual == null)
                AimLineVisual = GetComponent<BattleMoleculeAimLineVisual>();
        }

        protected virtual void OnEnable()
        {
            if (ShotQueue != null)
                ShotQueue.ShotRequested += ResolveShot;
        }

        protected virtual void OnDisable()
        {
            if (ShotQueue != null)
                ShotQueue.ShotRequested -= ResolveShot;

            _enemyHits.Clear();
        }

        protected abstract void ResolveShot(BattleMoleculeShotRequest request);

        protected void FindEnemies(Vector3 origin, Vector3 direction, float maxDistance, List<EnemyHit> enemyHits)
        {
            enemyHits.Clear();

            if (_physicsService == null || direction.sqrMagnitude <= Mathf.Epsilon || maxDistance < 0f)
                return;

            int hitCount = _physicsService.RaycastNonAlloc(origin, direction, maxDistance, ~0, ShotHits);

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit2D hit = ShotHits[i];
                if (hit.collider == null)
                    continue;

                EnemyUnit enemy = hit.collider.GetComponentInParent<EnemyUnit>();
                if (enemy == null || !enemy.IsAlive || ContainsEnemy(enemyHits, enemy))
                    continue;

                Vector3 hitPoint = hit.point;
                hitPoint.z = origin.z;
                enemyHits.Add(new EnemyHit(enemy, hit.distance, hitPoint));
            }

            enemyHits.Sort((a, b) => a.Distance.CompareTo(b.Distance));
        }

        protected int CurrentShotDamage()
        {
            float bonusDamage = TalentService != null ? TalentService.BonusOf(DamageTalentType) : 0f;
            return Mathf.Max(1, BaseShotDamage + Mathf.RoundToInt(bonusDamage));
        }

        protected void Damage(EnemyUnit target, Vector3 origin)
        {
            Debug.DrawLine(origin, target.transform.position, Color.yellow, 0.5f);
            target.TakeDamage(CurrentShotDamage());
        }

        protected int DamageEnemies(
            Vector3 origin,
            int maxTargets,
            HashSet<EnemyUnit> ignoredTargets,
            out Vector3? lastDamagedHitPoint)
        {
            int damagedTargets = 0;
            lastDamagedHitPoint = null;

            if (maxTargets <= 0)
                return damagedTargets;

            foreach (EnemyHit hit in _enemyHits)
            {
                EnemyUnit target = hit.Enemy;
                if (target == null || (ignoredTargets != null && ignoredTargets.Contains(target)))
                    continue;

                lastDamagedHitPoint = hit.Point;
                Damage(target, origin);
                ignoredTargets?.Add(target);
                damagedTargets++;

                if (damagedTargets >= maxTargets)
                    return damagedTargets;
            }

            return damagedTargets;
        }

        private static bool ContainsEnemy(List<EnemyHit> enemyHits, EnemyUnit enemy)
        {
            foreach (EnemyHit hit in enemyHits)
            {
                if (hit.Enemy == enemy)
                    return true;
            }

            return false;
        }

        protected readonly struct EnemyHit
        {
            public EnemyHit(EnemyUnit enemy, float distance, Vector3 point)
            {
                Enemy = enemy;
                Distance = distance;
                Point = point;
            }

            public EnemyUnit Enemy { get; }
            public float Distance { get; }
            public Vector3 Point { get; }
        }
    }
}
