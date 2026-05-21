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
    [RequireComponent(typeof(BattleMoleculeAimLineView))]
    public abstract class BattleMoleculeAttack : MonoBehaviour, IBattleMoleculeAutoLoadRule
    {
        private static readonly RaycastHit2D[] ShotHits = new RaycastHit2D[64];

        [field: SerializeField] private BattleMoleculeShotQueue ShotQueue { get; set; }
        [field: SerializeField] private BattleMoleculeAimLineView AimLine { get; set; }

        [Inject] private IPhysicsService _physicsService;
        [Inject] protected BattleMoleculeConfig Config;
        [Inject] protected ITalentService TalentService;

        protected BattleMoleculeAimLineView AimLineView => AimLine;
        protected abstract int BaseShotDamage { get; }
        protected abstract TalentType DamageTalentType { get; }
        protected abstract TalentType AutoLoadTalentType { get; }

        protected virtual void Awake()
        {
            if (ShotQueue == null)
                ShotQueue = GetComponent<BattleMoleculeShotQueue>();

            if (AimLine == null)
                AimLine = GetComponent<BattleMoleculeAimLineView>();
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

        public bool CanAutoLoad(BattleMoleculeRuntimeContext context)
        {
            return context.IsUnlocked(AutoLoadTalentType);
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
