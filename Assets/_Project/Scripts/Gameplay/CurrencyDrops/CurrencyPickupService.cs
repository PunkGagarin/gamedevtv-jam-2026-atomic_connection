using System.Collections.Generic;
using UnityEngine;
using Zenject;
using _Project.Scripts.Audio.Domain;
using _Project.Scripts.Gameplay.Cameras.Provider;
using _Project.Scripts.Gameplay.Common.Random;
using _Project.Scripts.Gameplay.Currencies;
using _Project.Scripts.Gameplay.Drag;
using _Project.Scripts.Gameplay.Input.Service;
using _Project.Scripts.Gameplay.Level;
using _Project.Scripts.Gameplay.Talents;
using _Project.Scripts.Infrastructure.AssetManagement;

namespace _Project.Scripts.Gameplay.CurrencyDrops
{
    public class CurrencyPickupService : ICurrencyPickupService
    {
        private const string PICKUP_CONTAINER_NAME = "CurrencyPickups";
        private const string PICKUP_AREA_OBJECT_NAME = "CurrencyPickupArea";
        private const Sounds PICKUP_SOUND = Sounds.singlePop;

        private readonly List<CurrencyPickup> _pickups = new();
        private CurrencyPickupAreaIndicator _pickupAreaIndicator;

        [Inject] private IAssetProvider _assetProvider;
        [Inject] private ICameraProvider _cameraProvider;
        [Inject] private ICurrencyService _currencyService;
        [Inject] private IGameplayRuntimeHierarchy _runtimeHierarchy;
        [Inject] private IDragService _dragService;
        [Inject] private IInputService _inputService;
        [Inject] private IInstantiator _instantiator;
        [Inject] private IRandomService _random;
        [Inject] private CurrencyPickupConfig _config;
        [Inject] private ITalentService _talentService;
        [Inject] private AudioService _audioService;

        public void Start()
        {
            Cleanup();
        }

        public void Spawn(CurrencyAmount amount, Vector3 worldPosition)
        {
            if (amount.Amount <= 0)
                return;

            CurrencyPickup prefab = _assetProvider.LoadAsset<CurrencyPickup>(_config.PickupPrefabResourcePath);

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

        private void SpawnPickup(CurrencyPickup prefab, CurrencyAmount amount, Vector3 worldPosition)
        {
            CurrencyPickup pickup = _instantiator.InstantiatePrefabForComponent<CurrencyPickup>(
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
            {
                HidePickupArea();
                return;
            }

            Camera camera = _cameraProvider.MainCamera;
            if (camera == null)
            {
                HidePickupArea();
                return;
            }

            Vector3 cursorWorldPosition = CursorWorldPosition(camera);
            float pickupAreaHalfSize = CurrentPickupAreaHalfSize();
            ShowPickupArea(cursorWorldPosition, pickupAreaHalfSize);

            for (int i = _pickups.Count - 1; i >= 0; i--)
            {
                CurrencyPickup pickup = _pickups[i];
                if (pickup == null)
                {
                    _pickups.RemoveAt(i);
                    continue;
                }

                if (!pickup.IsInsidePickupArea(cursorWorldPosition, pickupAreaHalfSize))
                    continue;

                CollectAt(i, pickup);
            }
        }

        public void Cleanup()
        {
            foreach (CurrencyPickup pickup in _pickups)
            {
                if (pickup != null)
                    Object.Destroy(pickup.gameObject);
            }

            _pickups.Clear();
            DestroyPickupArea();
        }

        private Vector3 SpawnPositionNear(Vector3 worldPosition)
        {
            float jitterRadius = Mathf.Max(0f, _config.SpawnJitterRadius);
            if (Mathf.Approximately(jitterRadius, 0f))
                return worldPosition;

            Vector2 offset = RandomGeometry.PointInCircle(_random, jitterRadius);
            return worldPosition + new Vector3(offset.x, offset.y, 0f);
        }

        private void CollectAt(int index, CurrencyPickup pickup)
        {
            _pickups.RemoveAt(index);
            PlayCollectSound();
            _currencyService.Add(pickup.Amount);
            pickup.PlayCollected(_config);
        }

        private void PlayCollectSound()
        {
            _audioService?.PlaySfxWithRandomPitch(PICKUP_SOUND);
        }

        private Vector3 CursorWorldPosition(Camera camera)
        {
            Vector3 screenPosition = _inputService.GetScreenMousePosition();
            screenPosition.z = Mathf.Abs(camera.transform.position.z);
            Vector3 worldPosition = camera.ScreenToWorldPoint(screenPosition);
            worldPosition.z = 0f;
            return worldPosition;
        }

        private void ShowPickupArea(Vector3 center, float halfSize)
        {
            if (!_config.ShowPickupAreaIndicator)
            {
                HidePickupArea();
                return;
            }

            EnsurePickupArea();
            _pickupAreaIndicator.Configure(
                _config.PickupAreaLineWidth,
                _config.PickupAreaSortingOrder,
                _config.PickupAreaColor);
            _pickupAreaIndicator.Show(center, halfSize);
        }

        private void HidePickupArea()
        {
            if (_pickupAreaIndicator != null)
                _pickupAreaIndicator.Hide();
        }

        private void EnsurePickupArea()
        {
            if (_pickupAreaIndicator != null)
                return;

            GameObject areaObject = new(PICKUP_AREA_OBJECT_NAME);
            areaObject.transform.SetParent(_runtimeHierarchy.GetOrCreateContainer(PICKUP_CONTAINER_NAME), false);
            _pickupAreaIndicator = areaObject.AddComponent<CurrencyPickupAreaIndicator>();
        }

        private void DestroyPickupArea()
        {
            if (_pickupAreaIndicator == null)
                return;

            Object.Destroy(_pickupAreaIndicator.gameObject);
            _pickupAreaIndicator = null;
        }

        private float CurrentPickupAreaHalfSize()
        {
            float talentBonus = _talentService != null
                ? _talentService.BonusOf(TalentType.CurrencyPickupArea)
                : 0f;

            return Mathf.Max(0f, _config.PickupAreaHalfSize + talentBonus);
        }
    }
}
