using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace _Project.Scripts.Gameplay.Talents
{
    [RequireComponent(typeof(CanvasGroup))]
    public class TalentNodeView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [field: SerializeField] public RectTransform RectTransform { get; private set; }
        [field: SerializeField] private Button Button { get; set; }
        [field: SerializeField] private Image Background { get; set; }
        [field: SerializeField] private Image IconImage { get; set; }
        [field: SerializeField] private Image NotificationImage { get; set; }
        [field: SerializeField] private TextMeshProUGUI TitleLabel { get; set; }
        [field: SerializeField] private TextMeshProUGUI LevelLabel { get; set; }
        [field: SerializeField] private TextMeshProUGUI CostLabel { get; set; }

        [field: Header("Colors")]
        [field: SerializeField] private Color LockedColor { get; set; } = new(0.11f, 0.08f, 0.11f, 1f);
        [field: SerializeField] private Color NotEnoughCurrencyColor { get; set; } = new(0.22f, 0.12f, 0.1f, 1f);
        [field: SerializeField] private Color AvailableColor { get; set; } = new(0.18f, 0.25f, 0.18f, 1f);
        [field: SerializeField] private Color BoughtColor { get; set; } = new(0.12f, 0.2f, 0.32f, 1f);

        private TalentId _talentId;
        private TalentDefinition _talent;
        private TalentTreeWindow _window;
        private TalentTreeAnimationConfig _animationConfig;
        private TalentNodeViewState _state;
        private CanvasGroup _canvasGroup;
        private Vector3 _baseScale;
        private Tween _scaleTween;
        private Tween _shakeTween;
        private Tween _revealTween;

        public void Initialize(TalentTreeWindow window, TalentDefinition talent, TalentTreeAnimationConfig animationConfig)
        {
            _window = window;
            _talent = talent;
            _talentId = talent.Id;
            _animationConfig = animationConfig;
            EnsureCanvasGroup();
            _baseScale = RectTransform.localScale;

            if (IconImage != null && talent.Icon != null)
                IconImage.sprite = talent.Icon;

            Button.onClick.AddListener(OnClicked);
        }

        public void Refresh(TalentDefinition talent, int level, TalentNodeViewState state, string priceText)
        {
            _state = state;

            if (Background != null)
                Background.color = ColorFor(state);

            if (LevelLabel != null)
                LevelLabel.text = $"{level}/{talent.MaxLevel}";

            if (CostLabel != null)
                CostLabel.text = state == TalentNodeViewState.Maxed ? "MAX" : priceText;

            if (Button != null)
                Button.interactable = state is TalentNodeViewState.Available
                    or TalentNodeViewState.NotEnoughCurrency
                    or TalentNodeViewState.Maxed;

            RefreshNotificationDot(state);
        }

        public void SetVisible(bool visible)
        {
            EnsureCanvasGroup();

            _scaleTween?.Kill();
            _revealTween?.Kill();
            RectTransform.localScale = _baseScale;
            _canvasGroup.alpha = visible ? 1f : 0f;
            _canvasGroup.interactable = visible;
            _canvasGroup.blocksRaycasts = visible;
        }

        public void PlayReveal(TweenCallback onComplete = null)
        {
            EnsureCanvasGroup();
            SetVisible(true);
            _scaleTween?.Kill();
            _revealTween?.Kill();

            RectTransform.localScale = _baseScale * _animationConfig.NodeRevealStartScale;
            _canvasGroup.alpha = 0f;
            _revealTween = DOTween.Sequence()
                .Append(_canvasGroup.DOFade(1f, _animationConfig.NodeRevealFadeDuration))
                .Join(RectTransform.DOScale(_baseScale * _animationConfig.NodeRevealPulseScale, _animationConfig.NodeRevealDuration).SetEase(Ease.OutBack))
                .Append(RectTransform.DOScale(_baseScale, _animationConfig.NodeRevealSettleDuration).SetEase(Ease.OutSine))
                .OnComplete(onComplete);
        }

        public void PlayUnlockPulse()
        {
            _scaleTween?.Kill();
            _scaleTween = DOTween.Sequence()
                .Append(RectTransform.DOScale(_baseScale * _animationConfig.NodeUnlockPulseScale, _animationConfig.NodeUnlockPulseDuration).SetEase(Ease.OutSine))
                .Append(RectTransform.DOScale(_baseScale, _animationConfig.NodeUnlockPulseDuration).SetEase(Ease.InSine));
        }

        public void PlayCannotBuyFeedback()
        {
            Vector2 anchoredPosition = RectTransform.anchoredPosition;
            _shakeTween?.Kill();
            _shakeTween = RectTransform
                .DOShakeAnchorPos(
                    _animationConfig.NodeShakeDuration,
                    _animationConfig.NodeShakeStrength,
                    _animationConfig.NodeShakeVibrato,
                    _animationConfig.NodeShakeRandomness,
                    false,
                    true)
                .OnComplete(() => RectTransform.anchoredPosition = anchoredPosition);

            // TODO: Проиграть SFX нехватки валюты/ошибки, например AudioService.PlaySound(Sounds.error).
        }

        private void OnDestroy()
        {
            if (Button != null)
                Button.onClick.RemoveListener(OnClicked);

            _scaleTween?.Kill();
            _shakeTween?.Kill();
            _revealTween?.Kill();
        }

        private void OnClicked()
        {
            switch (_state)
            {
                case TalentNodeViewState.Available:
                    TalentNodePurchaseResult result = _window.TryBuy(_talentId);

                    if (result == TalentNodePurchaseResult.Ignored)
                        return;

                    if (result == TalentNodePurchaseResult.Purchased)
                    {
                        // TODO: Проиграть SFX успешной покупки ноды, например AudioService.PlaySound(Sounds.talentBuy).
                        return;
                    }

                    PlayCannotBuyFeedback();
                    return;
                case TalentNodeViewState.NotEnoughCurrency:
                    PlayCannotBuyFeedback();
                    return;
                case TalentNodeViewState.Maxed:
                    // TODO: Проиграть мягкий SFX "нода уже замакшена", без shake/error.
                    return;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _scaleTween?.Kill();
            _scaleTween = RectTransform
                .DOScale(_baseScale * _animationConfig.NodeHoverScale, _animationConfig.NodeHoverDuration)
                .SetEase(Ease.OutSine);
            _window.ShowTooltip(_talent, RectTransform);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _scaleTween?.Kill();
            _scaleTween = RectTransform
                .DOScale(_baseScale, _animationConfig.NodeHoverDuration)
                .SetEase(Ease.OutSine);
            _window.HideTooltip();
        }

        private void EnsureCanvasGroup()
        {
            if (_canvasGroup != null)
                return;

            _canvasGroup = GetComponent<CanvasGroup>();

            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        private void RefreshNotificationDot(TalentNodeViewState state)
        {
            if (NotificationImage == null)
                return;

            NotificationImage.gameObject.SetActive(state == TalentNodeViewState.Available);
        }

        private Color ColorFor(TalentNodeViewState state)
        {
            return state switch
            {
                TalentNodeViewState.Maxed => BoughtColor,
                TalentNodeViewState.Available => AvailableColor,
                TalentNodeViewState.NotEnoughCurrency => NotEnoughCurrencyColor,
                _ => LockedColor
            };
        }
    }

    public enum TalentNodeViewState
    {
        Locked = 0,
        NotEnoughCurrency = 1,
        Available = 2,
        Maxed = 3
    }

    public enum TalentNodePurchaseResult
    {
        Ignored = 0,
        Failed = 1,
        Purchased = 2
    }
}
