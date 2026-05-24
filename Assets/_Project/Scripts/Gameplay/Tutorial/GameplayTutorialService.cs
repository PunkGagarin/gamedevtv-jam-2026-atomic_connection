using _Project.Scripts.Gameplay.Cameras.Provider;
using _Project.Scripts.Gameplay.Common.Time;
using _Project.Scripts.Gameplay.Enemies;
using _Project.Scripts.Gameplay.Units.AtomCores;
using _Project.Scripts.Gameplay.Units.BattleMolecules;
using _Project.Scripts.Gameplay.Units.FreeAtoms;
using _Project.Scripts.Infrastructure.AssetManagement;
using _Project.Scripts.Infrastructure.UI;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Tutorial
{
    public class GameplayTutorialService : IGameplayTutorialService
    {
        private GameplayTutorialView _view;
        private TutorialStep _step;
        private bool _isStarted;

        [Inject] private GameplayTutorialConfig _config;
        [Inject] private ITutorialPreferencesService _preferences;
        [Inject] private IAssetProvider _assetProvider;
        [Inject] private IInstantiator _instantiator;
        [Inject] private IUiRootProvider _uiRootProvider;
        [Inject] private ICameraProvider _cameraProvider;
        [Inject] private ITimeService _time;
        [Inject] private IAtomCoreService _atomCoreService;
        [Inject] private IBattleMoleculeService _battleMoleculeService;
        [Inject] private IEnemyService _enemyService;

        public void Start()
        {
            Cleanup();

            if (_preferences.GameplayTutorialCompleted)
                return;

            Subscribe();
            _step = TutorialStep.GenerateAtom;
            _isStarted = true;
            TryCreateView();
        }

        public void Update()
        {
            if (!_isStarted)
                return;

            if (_view == null && !TryCreateView())
                return;

            _view.Tick(_time.DeltaTime);

            switch (_step)
            {
                case TutorialStep.GenerateAtom:
                    UpdateCoreHint();
                    break;
                case TutorialStep.DropAtomToMolecule:
                    UpdateDropHint();
                    break;
                case TutorialStep.WaitForCharge:
                    TryEnterAttackHint();
                    break;
                case TutorialStep.Shoot:
                    UpdateAttackHint();
                    break;
            }
        }

        public void Cleanup()
        {
            Unsubscribe();

            if (_view != null)
                UnityEngine.Object.Destroy(_view.gameObject);

            _view = null;
            _step = TutorialStep.None;
            _isStarted = false;
        }

        private bool TryCreateView()
        {
            RectTransform uiRoot = _uiRootProvider.UIRoot;
            if (uiRoot == null)
                return false;

            GameplayTutorialView prefab = _assetProvider.LoadAsset<GameplayTutorialView>(_config.PrefabResourcePath);
            if (prefab == null)
            {
                Debug.LogWarning($"Gameplay tutorial prefab was not found at Resources/{_config.PrefabResourcePath}.");
                return false;
            }

            _view = _instantiator.InstantiatePrefabForComponent<GameplayTutorialView>(prefab, uiRoot);
            _view.transform.SetAsLastSibling();
            return true;
        }

        private void Subscribe()
        {
            _atomCoreService.AtomGenerated += OnAtomGenerated;
            _battleMoleculeService.MoleculeBonded += OnMoleculeBonded;
            _battleMoleculeService.MoleculeCharged += OnMoleculeCharged;
            _battleMoleculeService.ShotRequested += OnShotRequested;
        }

        private void Unsubscribe()
        {
            _atomCoreService.AtomGenerated -= OnAtomGenerated;
            _battleMoleculeService.MoleculeBonded -= OnMoleculeBonded;
            _battleMoleculeService.MoleculeCharged -= OnMoleculeCharged;
            _battleMoleculeService.ShotRequested -= OnShotRequested;
        }

        private void UpdateCoreHint()
        {
            Transform core = _atomCoreService.CurrentCoreTransform;
            if (core == null || !TryWorldToScreen(core.position, out Vector2 screenPosition))
                return;

            _view.ShowCoreHint(screenPosition + _config.CoreCursorOffset, _config, _config.CoreClickVisual);
        }

        private void UpdateDropHint()
        {
            Transform core = _atomCoreService.CurrentCoreTransform;
            BattleMolecule molecule = _battleMoleculeService.FirstMolecule;
            if (core == null || molecule == null)
                return;

            if (!TryWorldToScreen(core.position, out Vector2 from) ||
                !TryWorldToScreen(molecule.transform.position, out Vector2 to))
                return;

            _view.ShowDragHint(
                from + _config.DragCursorOffset,
                to + _config.DragCursorOffset,
                _config,
                _config.AtomDragVisual);
        }

        private void TryEnterAttackHint()
        {
            BattleMolecule molecule = _battleMoleculeService.FirstMolecule;
            if (molecule == null || !molecule.IsCharged)
                return;

            _step = TutorialStep.Shoot;
        }

        private void UpdateAttackHint()
        {
            BattleMolecule molecule = _battleMoleculeService.FirstMolecule;
            if (molecule == null || !molecule.IsCharged)
                return;

            if (!TryWorldToScreen(molecule.transform.position, out Vector2 from))
                return;

            if (!_enemyService.TryGetNearestEnemyPosition(molecule.transform.position, out Vector3 enemyPosition) ||
                !TryWorldToScreen(enemyPosition, out Vector2 enemyScreenPosition))
                return;

            Vector2 awayFromEnemy = from - enemyScreenPosition;
            Vector2 pullDirection = awayFromEnemy.sqrMagnitude > Mathf.Epsilon
                ? awayFromEnemy.normalized
                : _config.FallbackAttackPullDirection.normalized;
            if (pullDirection.sqrMagnitude <= Mathf.Epsilon)
                pullDirection = Vector2.left;

            Vector2 to = from + pullDirection * _config.AttackPullDistancePixels;
            _view.ShowDragHint(
                from + _config.DragCursorOffset,
                to + _config.DragCursorOffset,
                _config,
                _config.AttackVisual,
                enemyScreenPosition);
        }

        private bool TryWorldToScreen(Vector3 worldPosition, out Vector2 screenPosition)
        {
            screenPosition = default;
            Camera camera = _cameraProvider.MainCamera;
            if (camera == null)
                return false;

            screenPosition = camera.WorldToScreenPoint(worldPosition);
            return true;
        }

        private void OnAtomGenerated(FreeAtom atom)
        {
            if (_step == TutorialStep.GenerateAtom)
                _step = TutorialStep.DropAtomToMolecule;
        }

        private void OnMoleculeBonded(BattleMolecule molecule)
        {
            if (_step == TutorialStep.DropAtomToMolecule)
            {
                _step = TutorialStep.WaitForCharge;
                _view?.Hide();
            }
        }

        private void OnMoleculeCharged(BattleMolecule molecule)
        {
            if (_step == TutorialStep.WaitForCharge)
                _step = TutorialStep.Shoot;
        }

        private void OnShotRequested(BattleMoleculeShotRequest request)
        {
            if (_step != TutorialStep.Shoot)
                return;

            CompleteTutorial();
        }

        private void CompleteTutorial()
        {
            _preferences.MarkGameplayTutorialCompleted();
            Cleanup();
        }

        private enum TutorialStep
        {
            None = 0,
            GenerateAtom = 1,
            DropAtomToMolecule = 2,
            WaitForCharge = 3,
            Shoot = 4
        }
    }
}
