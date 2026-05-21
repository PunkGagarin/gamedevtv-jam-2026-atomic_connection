using System.Collections.Generic;
using _Project.Scripts.Gameplay.Common.Time;
using _Project.Scripts.Gameplay.Drag;
using _Project.Scripts.Gameplay.Talents;
using _Project.Scripts.Gameplay.Units.AtomCores;
using _Project.Scripts.Gameplay.Units.FreeAtoms;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules
{
    public class BattleMoleculeService : IBattleMoleculeService
    {
        private readonly List<IBattleMoleculeRuntimeBehavior> _runtimeBehaviors = new();
        private readonly List<AutoLoadEntry> _autoLoadEntries = new();
        private readonly List<FreeAtom> _coreAtoms = new();
        private bool _isStarted;
        private float _autoLoadTimer;
        private int _nextAutoLoadMoleculeIndex;

        [Inject] private IBattleMoleculeFactory _battleMoleculeFactory;
        [Inject] private ITimeService _time;
        [Inject] private IAtomCoreService _atomCoreService;
        [Inject] private BattleMoleculeConfig _config;
        [Inject] private ITalentService _talentService;
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

            foreach (IBattleMoleculeRuntimeBehavior runtimeBehavior in _runtimeBehaviors)
            {
                if (runtimeBehavior == null)
                    continue;

                if (runtimeBehavior is Object unityObject && unityObject == null)
                    continue;

                runtimeBehavior.Tick(_time.DeltaTime);
            }

            TickAutoLoad(_time.DeltaTime);
        }

        public void FixedUpdate()
        {
        }

        public void Cleanup()
        {
            if (!_isStarted)
                return;

            _isStarted = false;
            _battleMoleculeFactory.MoleculeCreated -= TrackMolecule;

            _runtimeBehaviors.Clear();
            _autoLoadEntries.Clear();
            _coreAtoms.Clear();
            _autoLoadTimer = 0f;
            _nextAutoLoadMoleculeIndex = 0;
        }

        private void TrackMolecule(BattleMolecule molecule)
        {
            if (molecule == null)
                return;

            molecule.ConfigureCoreOrbit(_atomCoreService.CurrentCoreTransform, _config);

            MonoBehaviour[] components = molecule.GetComponents<MonoBehaviour>();
            BattleMoleculeRuntimeContext context = new(CurrentCore(), _config, _talentService);

            foreach (MonoBehaviour component in components)
            {
                if (component is IBattleMoleculeRuntimeBehavior runtimeBehavior)
                {
                    runtimeBehavior.Configure(context);
                    _runtimeBehaviors.Add(runtimeBehavior);
                }

                if (component is IBattleMoleculeAutoLoadRule autoLoadRule)
                    _autoLoadEntries.Add(new AutoLoadEntry(molecule, autoLoadRule));
            }
        }

        private void TickAutoLoad(float deltaTime)
        {
            if (deltaTime <= 0f || _autoLoadEntries.Count == 0)
            {
                _autoLoadTimer = 0f;
                return;
            }

            _autoLoadTimer += deltaTime;
            if (_autoLoadTimer < Mathf.Max(0.01f, _config.AutoLoadIntervalSeconds))
                return;

            _autoLoadTimer = 0f;
            AutoLoadMolecules();
        }

        private void AutoLoadMolecules()
        {
            AtomCore core = CurrentCore();

            if (core == null || core.OwnedAtoms == null)
                return;

            int moleculeCount = _autoLoadEntries.Count;
            if (moleculeCount == 0)
                return;

            _nextAutoLoadMoleculeIndex %= moleculeCount;
            int startIndex = _nextAutoLoadMoleculeIndex;
            BattleMoleculeRuntimeContext context = new(core, _config, _talentService);

            for (int offset = 0; offset < moleculeCount; offset++)
            {
                int moleculeIndex = (startIndex + offset) % moleculeCount;
                AutoLoadEntry entry = _autoLoadEntries[moleculeIndex];
                BattleMolecule molecule = entry.Molecule;

                if (molecule == null)
                    continue;

                if (entry.Rule == null)
                    continue;

                if (entry.Rule is Object unityObject && unityObject == null)
                    continue;

                if (!entry.Rule.CanAutoLoad(context))
                    continue;

                if (!TryGetAutoLoadAtom(core, out FreeAtom atom))
                    return;

                if (molecule.TryAutoLoadAtom(atom))
                    _nextAutoLoadMoleculeIndex = (moleculeIndex + 1) % moleculeCount;
            }
        }

        private bool TryGetAutoLoadAtom(AtomCore core, out FreeAtom atom)
        {
            atom = null;

            if (core == null || core.OwnedAtoms == null)
                return false;

            core.OwnedAtoms.GetOwned(FreeAtomOwnerKind.Core, _coreAtoms);

            foreach (FreeAtom candidate in _coreAtoms)
            {
                if (candidate == null)
                    continue;

                if (_dragService != null && _dragService.IsReserved(candidate))
                    continue;

                atom = candidate;
                return true;
            }

            return false;
        }

        private readonly struct AutoLoadEntry
        {
            public AutoLoadEntry(BattleMolecule molecule, IBattleMoleculeAutoLoadRule rule)
            {
                Molecule = molecule;
                Rule = rule;
            }

            public BattleMolecule Molecule { get; }
            public IBattleMoleculeAutoLoadRule Rule { get; }
        }

        private AtomCore CurrentCore()
        {
            return _atomCoreService.CurrentCoreTransform != null
                ? _atomCoreService.CurrentCoreTransform.GetComponent<AtomCore>()
                : null;
        }
    }
}
