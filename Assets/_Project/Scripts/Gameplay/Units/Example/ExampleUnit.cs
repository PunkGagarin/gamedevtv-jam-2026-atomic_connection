using _Project.Scripts.Gameplay.Cameras.Provider;
using _Project.Scripts.Gameplay.Common.Physics;
using _Project.Scripts.Gameplay.Input.Service;
using _Project.Scripts.Gameplay.Units.Atom;
using _Project.Scripts.Gameplay.UI;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Units.Example
{
    public class ExampleUnit : MonoBehaviour
    {
        [Inject] private IInputService _inputService;
        [Inject] private ICameraProvider _cameraProvider;
        [Inject] private IPhysicsService _physicsService;
        [Inject] private UnitClickConfig _config;
        [Inject] private IAtomFactory _atomFactory;

        [field: SerializeField] private ProgressBar ProgressBar { get; set; }

        private int _clicksRemaining;

        private void Start()
        {
            _clicksRemaining = _config.ClicksToGenerateAtom;
            ProgressBar.SetProgress(1f);
        }

        private void Update()
        {
            if (!_inputService.GetLeftMouseButtonDown())
                return;

            Camera camera = _cameraProvider.MainCamera;
            if (camera == null)
                return;

            Vector2 worldPos = _inputService.GetWorldMousePosition();
            Collider2D hit = _physicsService.OverlapPoint(worldPos, ~0);

            if (hit != null && hit.gameObject == gameObject)
                HandleClick();
        }

        private void HandleClick()
        {
            _clicksRemaining--;
            UpdateProgressBar();

            if (_clicksRemaining <= 0)
            {
                Vector2 offset = Random.insideUnitCircle * _config.SpawnRadiusOffset;
                _atomFactory.Create(transform.position + (Vector3)offset);
                _clicksRemaining = _config.ClicksToGenerateAtom;
                UpdateProgressBar();
            }
        }

        private void UpdateProgressBar()
        {
            ProgressBar.SetProgress((float)_clicksRemaining / _config.ClicksToGenerateAtom);
        }
    }
}
