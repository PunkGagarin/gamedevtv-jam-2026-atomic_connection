using System.Collections.Generic;
using UnityEngine;
using Zenject;
using _Project.Scripts.Gameplay.Cameras.Provider;
using _Project.Scripts.Gameplay.Common.Random;
using _Project.Scripts.Gameplay.Currencies;
using _Project.Scripts.Gameplay.Drag;
using _Project.Scripts.Gameplay.Input.Service;
using _Project.Scripts.Gameplay.Level;
using _Project.Scripts.Infrastructure.AssetManagement;

namespace _Project.Scripts.Gameplay.CurrencyDrops
{
    public class CurrencyPickupService : ICurrencyPickupService
    {
        private const string PICKUP_CONTAINER_NAME = "CurrencyPickups";

        private readonly List<CurrencyPickupView> _pickups = new();

        [Inject] private IAssetProvider _assetProvider;
        [Inject] private ICameraProvider _cameraProvider;
        [Inject] private ICurrencyService _currencyService;
        [Inject] private IGameplayRuntimeHierarchy _runtimeHierarchy;
        [Inject] private IDragService _dragService;
        [Inject] private IInputService _inputService;
        [Inject] private IInstantiator _instantiator;
        [Inject] private IRandomService _random;
        [Inject] private CurrencyPickupConfig _config;

        public void Start()
        {
            Cleanup();
        }

        public void Spawn(CurrencyAmount amount, Vector3 worldPosition)
        {
            if (amount.Amount <= 0)
                return;

            CurrencyPickupView prefab = _assetProvider.LoadAsset<CurrencyPickupView>(_config.PickupPrefabResourcePath);

            if (prefab == null)
            {
                Debug.LogError($"Currency pickup prefab is missing at Resources path '{_config.PickupPrefabResourcePath}'.");
                return;
            }

            if (_config.DisplayMode == CurrencyPickupDisplayMode.OnePickupPerUnit)
            {
                for (int i = 0; i < amount.Amount; i++)
                    SpawnPickup(prefab, new CurrencyAmount(amount.CurrencyId, 1), worldPosition);

                return;
            }

            SpawnPickup(prefab, amount, worldPosition);
        }

        private void SpawnPickup(CurrencyPickupView prefab, CurrencyAmount amount, Vector3 worldPosition)
        {
            CurrencyPickupView pickup = _instantiator.InstantiatePrefabForComponent<CurrencyPickupView>(
                prefab,
                SpawnPositionNear(worldPosition),
                Quaternion.identity,
                _runtimeHierarchy.GetOrCreateContainer(PICKUP_CONTAINER_NAME));

            pickup.Initialize(amount, _config);
            _pickups.Add(pickup);
        }

        public void Update()
        {
            if (_pickups.Count == 0 || _dragService.IsDragActive)
                return;

            Camera camera = _cameraProvider.MainCamera;
            if (camera == null)
                return;

            Vector3 cursorWorldPosition = camera.ScreenToWorldPoint(_inputService.GetScreenMousePosition());

            for (int i = _pickups.Count - 1; i >= 0; i--)
            {
                CurrencyPickupView pickup = _pickups[i];
                if (pickup == null)
                {
                    _pickups.RemoveAt(i);
                    continue;
                }

                cursorWorldPosition.z = pickup.transform.position.z;

                if (!pickup.Contains(cursorWorldPosition, _config.PickupRadius))
                    continue;

                CollectAt(i, pickup);
            }
        }

        public void Cleanup()
        {
            foreach (CurrencyPickupView pickup in _pickups)
            {
                if (pickup != null)
                    Object.Destroy(pickup.gameObject);
            }

            _pickups.Clear();
        }

        private Vector3 SpawnPositionNear(Vector3 worldPosition)
        {
            float jitterRadius = Mathf.Max(0f, _config.SpawnJitterRadius);
            if (Mathf.Approximately(jitterRadius, 0f))
                return worldPosition;

            float angle = _random.Range(0f, Mathf.PI * 2f);
            float distance = Mathf.Sqrt(_random.Range(0f, 1f)) * jitterRadius;
            Vector2 offset = new(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance);
            return worldPosition + new Vector3(offset.x, offset.y, 0f);
        }

        private void CollectAt(int index, CurrencyPickupView pickup)
        {
            _pickups.RemoveAt(index);
            PlayCollectSound();
            _currencyService.Add(pickup.Amount);
            pickup.PlayCollected(_config);
        }

        private void PlayCollectSound()
        {
            // TODO: inject AudioService and play Sounds.currencyPickup when the pickup sound asset is ready.
        }
    }
}
