using UnityEngine;
using Zenject;
using _Project.Scripts.Gameplay.Currencies;
using _Project.Scripts.Gameplay.Level;
using _Project.Scripts.Infrastructure.AssetManagement;

namespace _Project.Scripts.Gameplay.Feedback
{
    public class CurrencyDropPopupView : MonoBehaviour
    {
        private const string CURRENCY_DROP_POPUP_PREFAB_PATH = "Gameplay/Feedback/CurrencyDropPopup";
        private const string FEEDBACK_CONTAINER_NAME = "Feedback";

        [Inject] private IAssetProvider _assetProvider;
        [Inject] private GameplayFeedbackAnimationConfig _animationConfig;
        [Inject] private IGameplayRuntimeHierarchy _runtimeHierarchy;
        [Inject] private IInstantiator _instantiator;

        public void Show(CurrencyAmount amount)
        {
            if (amount.CurrencyId != CurrencyId.Isotopes || amount.Amount <= 0 || this == null || _animationConfig == null)
                return;

            Vector3 startPosition = transform.position + _animationConfig.CurrencyDropPopupWorldOffset;

            CurrencyDropPopup prefab = _assetProvider.LoadAsset<CurrencyDropPopup>(CURRENCY_DROP_POPUP_PREFAB_PATH);

            if (prefab == null)
            {
                Debug.LogError($"Currency drop popup prefab is missing at Resources path '{CURRENCY_DROP_POPUP_PREFAB_PATH}'.");
                return;
            }

            CurrencyDropPopup popup = _instantiator.InstantiatePrefabForComponent<CurrencyDropPopup>(
                prefab,
                startPosition,
                Quaternion.identity,
                _runtimeHierarchy.GetOrCreateContainer(FEEDBACK_CONTAINER_NAME));

            popup.Play(amount, _animationConfig);
        }
    }
}
