using System.Collections.Generic;
using _Project.Scripts.Gameplay.Drag;
using _Project.Scripts.Gameplay.Common.Physics;
using _Project.Scripts.Gameplay.Common.Time;
using _Project.Scripts.Gameplay.Enemies;
using _Project.Scripts.Gameplay.Input.Service;
using _Project.Scripts.Gameplay.Talents;
using _Project.Scripts.Gameplay.Units.AtomCores;
using _Project.Scripts.Gameplay.Units.FreeAtoms;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules
{
    public class BattleMoleculeService : IBattleMoleculeService
    {
        private const float AUTO_LOAD_INTERVAL_SECONDS = 2f;
        private static readonly RaycastHit2D[] ShotHits = new RaycastHit2D[64];

        private readonly List<BattleMolecule> _trackedMolecules = new();
        private readonly List<EnemyHit> _enemyHits = new();
        private bool _isStarted;
        private float _autoLoadTimer;

        [Inject] private IBattleMoleculeFactory _battleMoleculeFactory;
        [Inject] private IPhysicsService _physicsService;
        [Inject] private ITimeService _time;
        [Inject] private IAtomCoreService _atomCoreService;
        [Inject] private BattleMoleculeConfig _config;
        [Inject] private ITalentService _talentService;
        [Inject] private IInputService _inputService;
        [Inject] private IDragService _dragService;

        public void Start()
        {
            if (_isStarted)
                return;

            _isStarted = true;
            _battleMoleculeFactory.MoleculeCreated += TrackMolecule;

            foreach (BattleMolecule molecule in _battleMoleculeFactory.CreatedMolecules)
                TrackMolecule(molecule);
        }

        public void Update()
        {
            foreach (BattleMolecule molecule in _battleMoleculeFactory.CreatedMolecules)
            {
                if (molecule == null)
                    continue;

                molecule.Tick(_time.DeltaTime);
            }

            TickAutoLoad(_time.DeltaTime);
        }

        public void FixedUpdate()
        {
            foreach (BattleMolecule molecule in _battleMoleculeFactory.CreatedMolecules)
            {
                if (molecule == null)
                    continue;

                molecule.FixedTick(Time.fixedDeltaTime);
            }
        }

        public void Cleanup()
        {
            if (!_isStarted)
                return;

            _isStarted = false;
            _battleMoleculeFactory.MoleculeCreated -= TrackMolecule;

            foreach (BattleMolecule molecule in _trackedMolecules)
            {
                if (molecule != null)
                    molecule.ShotRequested -= ResolveShot;
            }

            _trackedMolecules.Clear();
            _autoLoadTimer = 0f;
        }

        private void TrackMolecule(BattleMolecule molecule)
        {
            if (molecule == null || _trackedMolecules.Contains(molecule))
                return;

            _trackedMolecules.Add(molecule);
            molecule.ConfigureCoreOrbit(_atomCoreService.CurrentCoreTransform, _config);
            molecule.ConfigureShield(CurrentCore(), CurrentShieldDuration(), _config.ShieldSecondsLostPerDamage);
            molecule.ShotRequested += ResolveShot;
        }

        private void ResolveShot(BattleMoleculeShotRequest request)
        {
            int targetCount = TargetCountFor(request);
            FindEnemies(request.Origin, request.Direction);

            for (int i = 0; i < _enemyHits.Count && i < targetCount; i++)
            {
                EnemyUnit target = _enemyHits[i].Enemy;
                if (target == null)
                    continue;

                Debug.DrawLine(request.Origin, target.transform.position, Color.yellow, 0.5f);
                target.TakeDamage(CurrentShotDamage(request.Kind));
            }
        }

        private void FindEnemies(Vector3 origin, Vector3 direction)
        {
            _enemyHits.Clear();

            int hitCount = _physicsService.RaycastNonAlloc(origin, direction, ~0, ShotHits);

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit2D hit = ShotHits[i];
                if (hit.collider == null)
                    continue;

                EnemyUnit enemy = hit.collider.GetComponentInParent<EnemyUnit>();
                if (enemy == null || !enemy.IsAlive || ContainsEnemy(enemy))
                    continue;

                _enemyHits.Add(new EnemyHit(enemy, hit.distance));
            }

            _enemyHits.Sort((a, b) => a.Distance.CompareTo(b.Distance));
        }

        private bool ContainsEnemy(EnemyUnit enemy)
        {
            foreach (EnemyHit hit in _enemyHits)
            {
                if (hit.Enemy == enemy)
                    return true;
            }

            return false;
        }

        private int TargetCountFor(BattleMoleculeShotRequest request)
        {
            if (request.Kind != BattleMoleculeShotKind.Regular)
                return 1;

            int pierce = Mathf.RoundToInt(_talentService.BonusOf(TalentType.PrimaryMoleculePierce));
            return Mathf.Max(1, 1 + pierce);
        }

        private int CurrentShotDamage(BattleMoleculeShotKind kind)
        {
            TalentType damageType = kind == BattleMoleculeShotKind.Mass
                ? TalentType.AreaMoleculeDamage
                : TalentType.PrimaryMoleculeDamage;
            float bonusDamage = _talentService.BonusOf(damageType);
            return Mathf.Max(1, _config.BaseShotDamage + Mathf.RoundToInt(bonusDamage));
        }

        private void TickAutoLoad(float deltaTime)
        {
            if (deltaTime <= 0f || !CanAutoLoad())
            {
                _autoLoadTimer = 0f;
                return;
            }

            _autoLoadTimer += deltaTime;
            if (_autoLoadTimer < AUTO_LOAD_INTERVAL_SECONDS)
                return;

            _autoLoadTimer = 0f;
            AutoLoadMolecules();
        }

        private bool CanAutoLoad()
        {
            if (_inputService != null && _inputService.GetLeftMouseButtonRaw())
                return false;

            if (_dragService != null && _dragService.IsDragActive)
                return false;

            return _talentService.IsUnlocked(TalentType.PrimaryMoleculeAutoLoad) ||
                   _talentService.IsUnlocked(TalentType.ShieldAutoLoad) ||
                   _talentService.IsUnlocked(TalentType.AreaMoleculeAutoLoad);
        }

        private void AutoLoadMolecules()
        {
            AtomCore core = CurrentCore();

            if (core == null || core.OwnedAtoms == null)
                return;

            foreach (BattleMolecule molecule in _battleMoleculeFactory.CreatedMolecules)
            {
                if (molecule == null)
                    continue;

                if (!CanAutoLoadMolecule(molecule))
                    continue;

                if (!core.OwnedAtoms.TryGetFirstOwned(FreeAtomOwnerKind.Core, out FreeAtom atom))
                    return;

                molecule.TryAutoLoadAtom(atom);
            }
        }

        private bool CanAutoLoadMolecule(BattleMolecule molecule)
        {
            TalentType autoLoadType = molecule.Kind switch
            {
                BattleMoleculeKind.Mass => TalentType.AreaMoleculeAutoLoad,
                BattleMoleculeKind.Shield => TalentType.ShieldAutoLoad,
                _ => TalentType.PrimaryMoleculeAutoLoad
            };

            return _talentService.IsUnlocked(autoLoadType);
        }

        private AtomCore CurrentCore()
        {
            return _atomCoreService.CurrentCoreTransform != null
                ? _atomCoreService.CurrentCoreTransform.GetComponent<AtomCore>()
                : null;
        }

        private float CurrentShieldDuration()
        {
            return Mathf.Max(0.1f, _config.ShieldDurationSeconds + _talentService.BonusOf(TalentType.ShieldDuration));
        }

        private readonly struct EnemyHit
        {
            public EnemyHit(EnemyUnit enemy, float distance)
            {
                Enemy = enemy;
                Distance = distance;
            }

            public EnemyUnit Enemy { get; }
            public float Distance { get; }
        }
    }
}
