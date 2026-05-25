using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Zenject;
using DG.Tweening;
using _Project.Scripts.Audio.Domain;
using _Project.Scripts.Gameplay.Currencies;
using _Project.Scripts.Gameplay.CurrencyDrops;
using _Project.Scripts.Gameplay.Windows;
using _Project.Scripts.GameplayData;
using _Project.Scripts.Localization;
using UnityEngine.Serialization;

namespace _Project.Scripts.Gameplay.Talents
{
    public class TalentTreeWindow : BaseWindow, IDragHandler
    {
        private const float PAN_PADDING = 160f;
        private const Sounds TALENT_NODE_REVEAL_SOUND = Sounds.BubbleClick;
        private const Sounds TALENT_PURCHASE_SOUND = Sounds.talnetBought;
        private const Sounds TALENT_MAXED_SOUND = Sounds.talentMaxed;
        private const Sounds TALENT_CANNOT_BUY_SOUND = Sounds.error;

        [field: SerializeField] private Button CloseButton { get; set; }
        [field: SerializeField, FormerlySerializedAs("<GoldLabel>k__BackingField")]
        private TextMeshProUGUI LegacyCurrencyLabel { get; set; }
        [field: SerializeField] private RectTransform NodesRoot { get; set; }
        [field: SerializeField] private RectTransform ConnectionsRoot { get; set; }
        [field: SerializeField] private RectTransform TooltipPanel { get; set; }
        [field: SerializeField] private TextMeshProUGUI TooltipTitleLabel { get; set; }
        [field: SerializeField] private TextMeshProUGUI TooltipDescriptionLabel { get; set; }
        [field: SerializeField] private TalentNodeView NodePrefab { get; set; }
        [field: SerializeField] private TalentConnectionView ConnectionPrefab { get; set; }
        [field: SerializeField, Min(0.1f)] private float MinZoom { get; set; } = 0.55f;
        [field: SerializeField, Min(0.1f)] private float MaxZoom { get; set; } = 1.6f;
        [field: SerializeField, Min(0.01f)] private float ZoomStep { get; set; } = 0.12f;
        [field: SerializeField, Min(0f)] private float TooltipViewportPadding { get; set; } = 12f;

        [Inject] private ITalentService _talentService;
        [Inject] private TalentConfig _talentConfig;
        [Inject] private ICurrencyService _currencyService;
        [Inject] private CurrencyPickupConfig _currencyPickupConfig;
        [Inject] private IWindowService _windowService;
        [Inject] private TalentTreeAnimationConfig _animationConfig;
        [Inject] private UiThemeConfig _themeConfig;
        [Inject] private LanguageService _languageService;
        [Inject] private LocalizationTool _localizationTool;
        [Inject] private AudioService _audio;

        private readonly Dictionary<TalentId, TalentNodeView> _nodesById = new();
        private readonly Dictionary<TalentId, TalentDefinition> _talentsById = new();
        private readonly List<TalentConnectionBinding> _connections = new();
        private readonly HashSet<TalentId> _visibleNodes = new();
        private readonly HashSet<TalentId> _revealingNodes = new();
        private static readonly HashSet<TalentId> _revealedTalentIds = new();
        private static bool _lastRefreshHadBoughtTalents;
        private Vector2 _graphContentHalfSize;
        private float _zoom = 1f;
        private bool _isRevealQueuePlaying;
        private Tween _tooltipTween;
        private IEnumerable<TalentDefinition> DefinedTalents =>
            _talentService?.Talents?.Where(talent => talent != null) ?? Enumerable.Empty<TalentDefinition>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            _revealedTalentIds.Clear();
            _lastRefreshHadBoughtTalents = false;
        }

        protected override void OnAwake()
        {
            Id = WindowId.TalentTreeWindow;
        }

        protected override void Initialize()
        {
            if (LegacyCurrencyLabel != null)
                LegacyCurrencyLabel.enabled = false;

            ApplyTheme();
            HideTooltip();
            BuildGraph();
            Refresh(false);
        }

        protected override void SubscribeUpdates()
        {
            CloseButton.onClick.AddListener(Close);
            _talentService.Changed += Refresh;
            _currencyService.Changed += Refresh;
            _languageService.OnSwitchLanguage += RefreshOnLanguageChanged;
        }

        protected override void UnsubscribeUpdates()
        {
            if (CloseButton != null)
                CloseButton.onClick.RemoveListener(Close);

            _talentService.Changed -= Refresh;
            _currencyService.Changed -= Refresh;
            _languageService.OnSwitchLanguage -= RefreshOnLanguageChanged;
        }

        public TalentNodePurchaseResult TryBuy(TalentId talentId)
        {
            if (_isRevealQueuePlaying)
                return TalentNodePurchaseResult.Ignored;

            TalentDefinition talent = _talentsById[talentId];

            if (!_talentService.Buy(talentId))
                return TalentNodePurchaseResult.Failed;

            return _talentService.LevelOf(talentId) >= talent.MaxLevel
                ? TalentNodePurchaseResult.PurchasedFinalLevel
                : TalentNodePurchaseResult.Purchased;
        }

        internal void PlayButtonClickSound() =>
            _audio.PlaySound(Sounds.buttonClick);

        internal void PlayTalentNodeRevealSound() =>
            _audio.PlaySfxWithRandomPitch(TALENT_NODE_REVEAL_SOUND);

        internal void PlayTalentPurchaseSound() =>
            _audio.PlaySound(TALENT_PURCHASE_SOUND);

        internal void PlayTalentMaxedSound() =>
            _audio.PlaySoundInSingleAudioSource(TALENT_MAXED_SOUND.ToString());

        internal void PlayTalentCannotBuySound() =>
            _audio.PlaySoundInSingleAudioSource(TALENT_CANNOT_BUY_SOUND.ToString());

        public void ShowTooltip(TalentDefinition talent, RectTransform nodeTransform)
        {
            if (TooltipPanel == null)
                return;

            TooltipTitleLabel.text = Localize(talent.Title);
            TooltipDescriptionLabel.text = Localize(talent.Description);
            TooltipPanel.gameObject.SetActive(true);
            TooltipPanel.anchoredPosition = ClampedTooltipPosition(
                NodesRoot.anchoredPosition +
                nodeTransform.anchoredPosition * _zoom +
                _animationConfig.TooltipOffset);

            _tooltipTween?.Kill();
            TooltipPanel.localScale = new Vector3(
                _animationConfig.TooltipStartScaleX,
                _animationConfig.TooltipStartScaleY,
                1f);
            _tooltipTween = TooltipPanel
                .DOScale(Vector3.one, _animationConfig.TooltipRevealDuration)
                .SetEase(Ease.OutBack);
        }

        public void HideTooltip()
        {
            if (TooltipPanel != null)
            {
                _tooltipTween?.Kill();
                TooltipPanel.gameObject.SetActive(false);
            }
        }

        private void ApplyTheme()
        {
            ApplyTooltipFontSize(TooltipTitleLabel);
            ApplyTooltipFontSize(TooltipDescriptionLabel);
        }

        private void ApplyTooltipFontSize(TextMeshProUGUI label)
        {
            if (label == null || _themeConfig == null)
                return;

            label.fontSize = _themeConfig.CurrencyTextFontSize;
            label.fontSizeMax = _themeConfig.CurrencyTextFontSize;
        }

        private void Update()
        {
            float scrollDelta = UnityEngine.Input.mouseScrollDelta.y;

            if (Mathf.Approximately(scrollDelta, 0f))
                return;

            _zoom = Mathf.Clamp(_zoom + scrollDelta * ZoomStep, MinZoom, MaxZoom);
            Vector3 scale = Vector3.one * _zoom;
            NodesRoot.localScale = scale;
            ConnectionsRoot.localScale = scale;
            SetGraphPanPosition(ClampPanPosition(NodesRoot.anchoredPosition));
            HideTooltip();
        }

        private void BuildGraph()
        {
            _talentsById.Clear();

            foreach (TalentDefinition talent in DefinedTalents)
                _talentsById[talent.Id] = talent;

            foreach (TalentDefinition talent in DefinedTalents)
                CreateNode(talent);

            foreach (TalentDefinition child in DefinedTalents)
            {
                if (child.Prerequisites == null)
                    continue;

                foreach (TalentId parentId in child.Prerequisites)
                {
                    if (!_talentsById.TryGetValue(parentId, out TalentDefinition parent))
                        continue;

                    CreateConnection(parent, child);
                }
            }

            CalculateGraphContentHalfSize();
        }

        private void CreateNode(TalentDefinition talent)
        {
            TalentNodeView node = Instantiate(NodePrefab, NodesRoot);
            node.RectTransform.anchoredPosition = ScaledGraphPosition(talent);
            node.Initialize(this, talent, _animationConfig);
            node.SetVisible(false);
            _nodesById[talent.Id] = node;
        }

        private void CreateConnection(TalentDefinition parent, TalentDefinition child)
        {
            TalentConnectionView connection = Instantiate(ConnectionPrefab, ConnectionsRoot);
            Vector2 from = ScaledGraphPosition(parent);
            Vector2 to = ScaledGraphPosition(child);
            connection.Initialize(from, to, _animationConfig);
            connection.SetVisible(false);
            _connections.Add(new TalentConnectionBinding(parent.Id, child.Id, connection));
        }

        private Vector2 ScaledGraphPosition(TalentDefinition talent)
        {
            Vector2 position = talent.GraphPosition * NodePositionScale();
            position.y = -position.y;
            return position;
        }

        private float NodePositionScale()
        {
            return _talentConfig != null ? Mathf.Max(0.1f, _talentConfig.NodePositionScale) : 1f;
        }

        private void CalculateGraphContentHalfSize()
        {
            if (_nodesById.Count == 0)
            {
                _graphContentHalfSize = NodesRoot.rect.size * 0.5f;
                return;
            }

            Vector2 min = new(float.PositiveInfinity, float.PositiveInfinity);
            Vector2 max = new(float.NegativeInfinity, float.NegativeInfinity);

            foreach (TalentNodeView node in _nodesById.Values)
            {
                RectTransform rectTransform = node.RectTransform;
                Vector2 position = rectTransform.anchoredPosition;
                Vector2 halfSize = rectTransform.rect.size * 0.5f;
                min = Vector2.Min(min, position - halfSize);
                max = Vector2.Max(max, position + halfSize);
            }

            Vector2 contentHalfSize = Vector2.Max(-min, max);
            _graphContentHalfSize = Vector2.Max(contentHalfSize, NodesRoot.rect.size * 0.5f);
        }

        private void Refresh() =>
            Refresh(true);

        private void Refresh(bool animateNewVisibleNodes)
        {
            bool hasBoughtTalents = DefinedTalents.Any(talent => _talentService.LevelOf(talent.Id) > 0);
            ResetRevealCacheIfProgressWasReset(hasBoughtTalents);

            List<TalentDefinition> newlyVisibleTalents = RefreshTalentNodes(animateNewVisibleNodes);
            RefreshConnections();
            PlayNodeRevealQueue(newlyVisibleTalents);
            _lastRefreshHadBoughtTalents = hasBoughtTalents;
        }

        private List<TalentDefinition> RefreshTalentNodes(bool animateNewVisibleNodes)
        {
            List<TalentDefinition> newlyVisibleTalents = new();

            foreach (TalentDefinition talent in DefinedTalents)
                if (RefreshTalentNode(talent, animateNewVisibleNodes))
                    newlyVisibleTalents.Add(talent);

            return newlyVisibleTalents;
        }

        private bool RefreshTalentNode(TalentDefinition talent, bool animateNewVisibleNodes)
        {
            int level = _talentService.LevelOf(talent.Id);
            bool prerequisitesBought = PrerequisitesBought(talent);
            bool canBuy = _talentService.CanBuy(talent.Id);
            TalentNodeViewState viewState = ViewStateFor(talent, level, prerequisitesBought, canBuy);
            bool shouldBeVisible = ShouldBeVisible(talent, level, prerequisitesBought);
            bool wasVisible = _visibleNodes.Contains(talent.Id);

            SetTalentVisibility(talent.Id, shouldBeVisible);
            CurrencyAmount price = talent.PriceForLevel(level);
            Sprite priceIcon = _currencyPickupConfig != null
                ? _currencyPickupConfig.IconFor(price.CurrencyId)
                : null;

            _nodesById[talent.Id].Refresh(
                talent,
                level,
                viewState,
                Localize(talent.Title),
                price,
                priceIcon);

            if (ShouldAnimateReveal(talent.Id, shouldBeVisible, wasVisible, animateNewVisibleNodes))
            {
                PrepareNodeReveal(talent.Id);
                return true;
            }

            if (!_revealingNodes.Contains(talent.Id))
                _nodesById[talent.Id].SetVisible(shouldBeVisible);

            return false;
        }

        private void SetTalentVisibility(TalentId talentId, bool shouldBeVisible)
        {
            if (shouldBeVisible)
                _visibleNodes.Add(talentId);
            else
                _visibleNodes.Remove(talentId);
        }

        private bool ShouldAnimateReveal(
            TalentId talentId,
            bool shouldBeVisible,
            bool wasVisible,
            bool animateNewVisibleNodes)
        {
            return shouldBeVisible &&
                   !wasVisible &&
                   animateNewVisibleNodes &&
                   !_revealedTalentIds.Contains(talentId);
        }

        private void PrepareNodeReveal(TalentId talentId)
        {
            _revealingNodes.Add(talentId);
            _revealedTalentIds.Add(talentId);
            _nodesById[talentId].SetVisible(false);
        }

        private void RefreshConnections()
        {
            foreach (TalentConnectionBinding connection in _connections)
            {
                bool parentBought = _talentService.LevelOf(connection.ParentId) > 0;
                bool childBought = _talentService.LevelOf(connection.ChildId) > 0;
                bool childVisible = _visibleNodes.Contains(connection.ChildId);
                bool parentVisible = _visibleNodes.Contains(connection.ParentId);
                bool childIsRevealing = _revealingNodes.Contains(connection.ChildId);

                if (!childIsRevealing)
                    connection.View.SetVisible(parentVisible && childVisible);

                connection.View.Refresh(parentBought, childBought);
            }
        }

        private bool ShouldBeVisible(TalentDefinition talent, int level, bool prerequisitesBought)
        {
            if (talent.Prerequisites == null || talent.Prerequisites.Count == 0)
                return true;

            return level > 0 || prerequisitesBought;
        }

        private TalentNodeViewState ViewStateFor(
            TalentDefinition talent,
            int level,
            bool prerequisitesBought,
            bool canBuy)
        {
            if (level >= talent.MaxLevel)
                return TalentNodeViewState.Maxed;

            if (!prerequisitesBought)
                return TalentNodeViewState.Locked;

            return canBuy ? TalentNodeViewState.Available : TalentNodeViewState.NotEnoughCurrency;
        }

        private bool PrerequisitesBought(TalentDefinition talent)
        {
            return talent.Prerequisites == null ||
                   talent.Prerequisites.All(prerequisite => _talentService.LevelOf(prerequisite) > 0);
        }

        private void RefreshOnLanguageChanged()
        {
            HideTooltip();
            Refresh(false);
        }

        public string Localize(string key) =>
            string.IsNullOrWhiteSpace(key) ? string.Empty : _localizationTool.GetText(key);

        private void ResetRevealCacheIfProgressWasReset(bool hasBoughtTalents)
        {
            if (_lastRefreshHadBoughtTalents && !hasBoughtTalents)
                _revealedTalentIds.Clear();
        }

        private void PlayNodeRevealQueue(List<TalentDefinition> talents)
        {
            if (talents.Count == 0)
                return;

            _isRevealQueuePlaying = true;
            talents.Sort((left, right) => DepthOf(left).CompareTo(DepthOf(right)));
            PlayNodeRevealAt(talents, 0);
        }

        private void PlayNodeRevealAt(IReadOnlyList<TalentDefinition> talents, int index)
        {
            if (index >= talents.Count)
            {
                _isRevealQueuePlaying = false;
                return;
            }

            TalentDefinition talent = talents[index];
            PlayNodeReveal(talent, () => PlayNodeRevealAt(talents, index + 1));
        }

        private int DepthOf(TalentDefinition talent)
        {
            if (talent.Prerequisites == null || talent.Prerequisites.Count == 0)
                return 0;

            int maxParentDepth = 0;

            foreach (TalentId parentId in talent.Prerequisites)
            {
                if (!_talentsById.TryGetValue(parentId, out TalentDefinition parent))
                    continue;

                maxParentDepth = Mathf.Max(maxParentDepth, DepthOf(parent) + 1);
            }

            return maxParentDepth;
        }

        private void PlayNodeReveal(TalentDefinition talent, TweenCallback onComplete)
        {
            TalentNodeView node = _nodesById[talent.Id];

            if (talent.Prerequisites == null || talent.Prerequisites.Count != 1)
            {
                node.PlayReveal(() => CompleteNodeReveal(talent, onComplete));
                return;
            }

            TalentId parentId = talent.Prerequisites[0];

            if (!_nodesById.TryGetValue(parentId, out TalentNodeView parent))
            {
                node.PlayReveal(() => CompleteNodeReveal(talent, onComplete));
                return;
            }

            parent.PlayUnlockPulse();
            TalentConnectionBinding connection = _connections.FirstOrDefault(binding =>
                binding.ParentId == parentId && binding.ChildId == talent.Id);

            if (connection.View == null)
            {
                node.PlayReveal(() => CompleteNodeReveal(talent, onComplete));
                return;
            }

            connection.View.SetVisible(true);
            connection.View.PlayUnlockPulse(() => node.PlayReveal(() => CompleteNodeReveal(talent, onComplete)));
        }

        private void CompleteNodeReveal(TalentDefinition talent, TweenCallback onComplete)
        {
            _revealingNodes.Remove(talent.Id);
            ShowVisibleConnectionsFor(talent.Id);
            PlayTalentNodeRevealSound();
            onComplete?.Invoke();
        }

        private void ShowVisibleConnectionsFor(TalentId childId)
        {
            foreach (TalentConnectionBinding connection in _connections)
            {
                if (connection.ChildId != childId)
                    continue;

                bool parentVisible = _visibleNodes.Contains(connection.ParentId);
                bool childVisible = _visibleNodes.Contains(connection.ChildId);
                connection.View.SetVisible(parentVisible && childVisible);
            }
        }

        private void Close()
        {
            PlayButtonClickSound();
            _windowService.Close(Id);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 newPos = NodesRoot.anchoredPosition + eventData.delta / _zoom;
            SetGraphPanPosition(ClampPanPosition(newPos));
        }

        private Vector2 ClampPanPosition(Vector2 position)
        {
            Vector2 viewportHalfSize = ParentViewportHalfSize();
            float maxPanX = Mathf.Max(0f, _graphContentHalfSize.x * _zoom - viewportHalfSize.x + PAN_PADDING);
            float maxPanY = Mathf.Max(0f, _graphContentHalfSize.y * _zoom - viewportHalfSize.y + PAN_PADDING);
            position.x = Mathf.Clamp(position.x, -maxPanX, maxPanX);
            position.y = Mathf.Clamp(position.y, -maxPanY, maxPanY);

            return position;
        }

        private Vector2 ParentViewportHalfSize()
        {
            RectTransform parent = NodesRoot.parent as RectTransform;
            return parent != null ? parent.rect.size * 0.5f : Vector2.zero;
        }

        private Vector2 ClampedTooltipPosition(Vector2 position)
        {
            RectTransform parent = TooltipPanel.parent as RectTransform;
            if (parent == null)
                return position;

            Vector2 parentSize = parent.rect.size;
            Vector2 tooltipSize = TooltipPanel.rect.size;
            Vector2 pivot = TooltipPanel.pivot;
            float padding = Mathf.Max(0f, TooltipViewportPadding);
            float minX = -parentSize.x * 0.5f + tooltipSize.x * pivot.x + padding;
            float maxX = parentSize.x * 0.5f - tooltipSize.x * (1f - pivot.x) - padding;
            float minY = -parentSize.y * 0.5f + tooltipSize.y * pivot.y + padding;
            float maxY = parentSize.y * 0.5f - tooltipSize.y * (1f - pivot.y) - padding;

            if (minX <= maxX)
                position.x = Mathf.Clamp(position.x, minX, maxX);

            if (minY <= maxY)
                position.y = Mathf.Clamp(position.y, minY, maxY);

            return position;
        }

        private void SetGraphPanPosition(Vector2 position)
        {
            NodesRoot.anchoredPosition = position;
            ConnectionsRoot.anchoredPosition = position;
        }

        protected override void Cleanup()
        {
            _tooltipTween?.Kill();
            _isRevealQueuePlaying = false;
            base.Cleanup();
        }
    }

}
