using System.Collections.Generic;
using _Project.Scripts.Gameplay.Common.Physics;
using _Project.Scripts.Gameplay.Common.Time;
using _Project.Scripts.Gameplay.Enemies;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules
{
    public class BattleMoleculeService : IBattleMoleculeService
    {
        private static readonly RaycastHit2D[] ShotHits = new RaycastHit2D[64];

        private readonly List<BattleMolecule> _trackedMolecules = new();
        private bool _isStarted;

        [Inject] private IBattleMoleculeFactory _battleMoleculeFactory;
        [Inject] private IPhysicsService _physicsService;
        [Inject] private ITimeService _time;

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
        }

        private void TrackMolecule(BattleMolecule molecule)
        {
            if (molecule == null || _trackedMolecules.Contains(molecule))
                return;

            _trackedMolecules.Add(molecule);
            molecule.ShotRequested += ResolveShot;
        }

        private void ResolveShot(Vector3 origin, Vector3 direction)
        {
            EnemyUnit target = FindFirstEnemy(origin, direction);
            if (target == null)
                return;

            Debug.DrawLine(origin, target.transform.position, Color.yellow, 0.5f);
            target.Kill();
        }

        private EnemyUnit FindFirstEnemy(Vector3 origin, Vector3 direction)
        {
            int hitCount = _physicsService.RaycastNonAlloc(origin, direction, ~0, ShotHits);
            EnemyUnit closestEnemy = null;
            float closestDistance = float.PositiveInfinity;

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit2D hit = ShotHits[i];
                if (hit.collider == null)
                    continue;

                EnemyUnit enemy = hit.collider.GetComponentInParent<EnemyUnit>();
                if (enemy == null || !enemy.IsAlive || hit.distance >= closestDistance)
                    continue;

                closestEnemy = enemy;
                closestDistance = hit.distance;
            }

            return closestEnemy;
        }
    }
}
