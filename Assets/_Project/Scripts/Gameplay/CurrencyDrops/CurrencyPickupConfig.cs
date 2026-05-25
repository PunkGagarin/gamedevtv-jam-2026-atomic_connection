using System;
using System.Collections.Generic;
using UnityEngine;
using _Project.Scripts.Gameplay.Currencies;

namespace _Project.Scripts.Gameplay.CurrencyDrops
{
    public enum CurrencyPickupDisplayMode
    {
        SinglePickupWithAmount = 0,
        OnePickupPerUnit = 1,
    }

    [CreateAssetMenu(fileName = "CurrencyPickupConfig", menuName = "Game Resources/Configs/Currency Pickups")]
    public class CurrencyPickupConfig : ScriptableObject
    {
        [field: Header("Pickup")]
        [field: SerializeField] public string PickupPrefabResourcePath { get; private set; } = "Gameplay/CurrencyDrops/CurrencyPickup";
        [field: SerializeField] public CurrencyPickupDisplayMode DisplayMode { get; private set; } = CurrencyPickupDisplayMode.OnePickupPerUnit;
        [field: SerializeField, Min(0f)] public float PickupAreaHalfSize { get; private set; } = 0.35f;
        [field: SerializeField, Min(0f)] public float SpawnJitterRadius { get; private set; } = 0.3f;

        [field: Header("Pickup Area Indicator")]
        [field: SerializeField] public bool ShowPickupAreaIndicator { get; private set; } = true;
        [field: SerializeField, Min(0f)] public float PickupAreaLineWidth { get; private set; } = 0.04f;
        [field: SerializeField] public int PickupAreaSortingOrder { get; private set; } = 60;
        [field: SerializeField] public Color PickupAreaColor { get; private set; } = new(0.4f, 1f, 0.75f, 0.85f);

        [field: Header("Idle Animation")]
        [field: SerializeField] public bool EnableIdleScaleAnimation { get; private set; } = true;
        [field: SerializeField, Min(1f)] public float IdleScaleMultiplier { get; private set; } = 1.12f;
        [field: SerializeField, Min(0f)] public float IdleScaleDuration { get; private set; } = 0.55f;

        [field: Header("Collect Icon Animation")]
        [field: SerializeField, Min(1f)] public float CollectIconScaleMultiplier { get; private set; } = 1.35f;
        [field: SerializeField, Min(0f)] public float CollectIconScaleDuration { get; private set; } = 0.12f;

        [field: Header("Victory Auto Collect")]
        [field: SerializeField, Min(0f)] public float VictoryAutoCollectDuration { get; private set; } = 0.35f;
        [field: SerializeField, Min(0f)] public float VictoryAutoCollectEndScaleMultiplier { get; private set; } = 0.2f;

        [field: Header("Collect Popup")]
        [field: SerializeField] public Vector3 CollectPopupWorldOffset { get; private set; } = new(0f, 0.55f, 0f);
        [field: SerializeField, Min(0f)] public float CollectPopupRiseDistance { get; private set; } = 0.45f;
        [field: SerializeField, Min(0f)] public float CollectPopupDuration { get; private set; } = 0.45f;
        [field: SerializeField, Min(0f)] public float CollectPopupPulseScale { get; private set; } = 1.15f;

        [field: Header("Icons")]
        [field: SerializeField] private List<CurrencyPickupIcon> Icons { get; set; } = new();

        public bool ShowCollectPopup => DisplayMode == CurrencyPickupDisplayMode.SinglePickupWithAmount;

        public Sprite IconFor(CurrencyId currencyId)
        {
            foreach (CurrencyPickupIcon icon in Icons)
            {
                if (icon.CurrencyId == currencyId)
                    return icon.Icon;
            }

            return null;
        }
    }

    [Serializable]
    public class CurrencyPickupIcon
    {
        [field: SerializeField] public CurrencyId CurrencyId { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }
    }
}
