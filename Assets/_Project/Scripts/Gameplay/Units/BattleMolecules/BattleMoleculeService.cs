using System;
using System.Collections.Generic;
using _Project.Scripts.Gameplay.Common.Time;
using _Project.Scripts.Gameplay.Drag;
using _Project.Scripts.Gameplay.Input.Service;
using _Project.Scripts.Gameplay.Talents;
using _Project.Scripts.Gameplay.Units.AtomCores;
using _Project.Scripts.Gameplay.Units.FreeAtoms;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules
{
    public class BattleMoleculeService : IBattleMoleculeService,
        IBattleMoleculeFeedTargetProvider,
        IBattleMoleculeConnectionAtomSourceProvider
    {
        private readonly List<BattleMolecule> _molecules = new();
        private readonly List<FreeAtom> _moleculeAtomsBuffer = new();
        private bool _isStarted;
        private BattleMolecule _activeMolecule;
        private AtomCore _core;

        [Inject] private ITimeService _time;
        [Inject] private BattleMoleculeConfig _config;
        [Inject] private ITalentService _talentService;
        [Inject] private IDragService _dragService;
        [Inject] private IInputService _inputService;

        public BattleMolecule ActiveFeedTarget => IsValidActiveMolecule(_activeMolecule) && _activeMolecule.CanReceiveConnectionAtom
            ? _activeMolecule
            : null;
        public BattleMolecule FirstMolecule => _molecules.Count > 0 ? _molecules[0] : null;
        public BattleMolecule ActiveMolecule => _activeMolecule;
        public BattleMolecule FirstAdditionalMolecule => FindFirstAdditionalMolecule();

        public event Action<BattleMolecule> MoleculeRegistered;
        public event Action<BattleMolecule> MoleculeBonded;
        public event Action<BattleMolecule> MoleculeCharged;
        public event Action<BattleMolecule> ActiveMoleculeChanged;
        public event Action<BattleMoleculeShotRequest> ShotRequested;

        public void ConfigureCore(AtomCore core)
        {
            _core = core;

            foreach (BattleMolecule molecule in _molecules)
                ConfigureMoleculeCore(molecule);
        }

        public void Register(BattleMolecule molecule)
        {
            if (molecule == null || _molecules.Contains(molecule))
                return;

            ConfigureMoleculeCore(molecule);
            molecule.Bonded += OnMoleculeBonded;
            molecule.Charged += OnMoleculeCharged;
            molecule.ShotRequested += OnMoleculeShotRequested;
            _molecules.Add(molecule);

            if (molecule.IsBonded && !IsValidActiveMolecule(_activeMolecule))
                SetActiveMolecule(molecule);

            MoleculeRegistered?.Invoke(molecule);
        }

        public void Start()
        {
            if (_isStarted)
                return;

            _isStarted = true;
        }

        public void Update()
        {
            foreach (BattleMolecule molecule in _molecules)
            {
                if (molecule == null)
                    continue;

                molecule.Tick(_time.DeltaTime);
            }

            TickMoleculeSelection();
        }

        public void Cleanup()
        {
            if (!_isStarted && _molecules.Count == 0)
                return;

            _isStarted = false;

            foreach (BattleMolecule molecule in _molecules)
            {
                if (molecule == null)
                    continue;

                molecule.Bonded -= OnMoleculeBonded;
                molecule.Charged -= OnMoleculeCharged;
                molecule.ShotRequested -= OnMoleculeShotRequested;
                molecule.SetActiveFeedVisual(false);
            }

            foreach (BattleMolecule molecule in _molecules)
            {
                if (molecule != null)
                    UnityEngine.Object.Destroy(molecule.gameObject);
            }

            _molecules.Clear();
            _moleculeAtomsBuffer.Clear();
            _activeMolecule = null;
            _core = null;
        }

        public void CollectSupplementalConnectionAtoms(BattleMolecule target, List<FreeAtom> results)
        {
            results?.Clear();

            if (target == null || results == null)
                return;

            foreach (BattleMolecule molecule in _molecules)
            {
                if (molecule == null || molecule == target || !molecule.IsBonded)
                    continue;

                molecule.CollectConnectionAtoms(_moleculeAtomsBuffer);

                foreach (FreeAtom atom in _moleculeAtomsBuffer)
                {
                    if (atom != null)
                        results.Add(atom);
                }
            }
        }

        private void OnMoleculeBonded(BattleMolecule molecule)
        {
            if (molecule == null)
                return;

            if (!IsValidActiveMolecule(_activeMolecule))
                SetActiveMolecule(molecule);

            MoleculeBonded?.Invoke(molecule);
        }

        private void OnMoleculeCharged(BattleMolecule molecule)
        {
            MoleculeCharged?.Invoke(molecule);
        }

        private void OnMoleculeShotRequested(BattleMoleculeShotRequest request)
        {
            ShotRequested?.Invoke(request);
        }

        private void TickMoleculeSelection()
        {
            if (_inputService == null || !_inputService.GetLeftMouseButtonUp())
                return;

            if (_dragService != null && _dragService.DragWasStartedThisPress)
                return;

            Vector2 worldPosition = _inputService.GetWorldMousePosition();
            for (int i = _molecules.Count - 1; i >= 0; i--)
            {
                BattleMolecule molecule = _molecules[i];
                if (!IsValidActiveMolecule(molecule) || !molecule.ContainsPoint(worldPosition))
                    continue;

                SetActiveMolecule(molecule);
                return;
            }
        }

        private bool IsValidActiveMolecule(BattleMolecule molecule)
        {
            return molecule != null && molecule.IsBonded;
        }

        private void SetActiveMolecule(BattleMolecule molecule)
        {
            if (!IsValidActiveMolecule(molecule))
                molecule = null;

            if (_activeMolecule == molecule)
                return;

            _activeMolecule = molecule;

            foreach (BattleMolecule createdMolecule in _molecules)
            {
                if (createdMolecule == null)
                    continue;

                createdMolecule.SetActiveFeedVisual(createdMolecule == _activeMolecule);
            }

            ActiveMoleculeChanged?.Invoke(_activeMolecule);
        }

        private void ConfigureMoleculeCore(BattleMolecule molecule)
        {
            if (molecule == null)
                return;

            molecule.ConfigureCoreOrbit(_core != null ? _core.transform : null, _config);
            molecule.ConfigureCoreInteraction(_core, _config, _talentService);
        }

        private BattleMolecule FindFirstAdditionalMolecule()
        {
            for (int i = 1; i < _molecules.Count; i++)
            {
                if (_molecules[i] != null)
                    return _molecules[i];
            }

            return null;
        }
    }
}
