using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Zenject;
using _Project.Scripts.Gameplay.Windows;

namespace _Project.Scripts.Gameplay.Talents
{
    public class TalentTreeWindow : BaseWindow, IDragHandler
    {
        [field: SerializeField] private Button CloseButton { get; set; }
        [field: SerializeField] private TextMeshProUGUI GoldLabel { get; set; }
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

        [Inject] private ITalentService _talentService;
        [Inject] private IWindowService _windowService;

        private readonly Dictionary<TalentId, TalentNodeView> _nodesById = new();
        private readonly List<TalentConnectionBinding> _connections = new();
        private float _zoom = 1f;

        protected override void OnAwake()
        {
            Id = WindowId.TalentTreeWindow;
        }

        protected override void Initialize()
        {
            HideTooltip();
            BuildGraph();
            Refresh();
        }

        protected override void SubscribeUpdates()
        {
            CloseButton.onClick.AddListener(Close);
            _talentService.Changed += Refresh;
        }

        protected override void UnsubscribeUpdates()
        {
            if (CloseButton != null)
                CloseButton.onClick.RemoveListener(Close);

            _talentService.Changed -= Refresh;
        }

        public void Buy(TalentId talentId)
        {
            _talentService.Buy(talentId);
        }

        public void ShowTooltip(TalentDefinition talent, RectTransform nodeTransform)
        {
            if (TooltipPanel == null)
                return;

            TooltipTitleLabel.text = talent.Title;
            TooltipDescriptionLabel.text = talent.Description;
            TooltipPanel.gameObject.SetActive(true);
            TooltipPanel.anchoredPosition = NodesRoot.anchoredPosition + nodeTransform.anchoredPosition * _zoom + new Vector2(0f, 94f);
        }

        public void HideTooltip()
        {
            if (TooltipPanel != null)
                TooltipPanel.gameObject.SetActive(false);
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
            HideTooltip();
        }

        private void BuildGraph()
        {
            IReadOnlyList<TalentDefinition> talents = _talentService.Talents;
            Dictionary<TalentId, TalentDefinition> talentsById = talents.ToDictionary(talent => talent.Id);

            foreach (TalentDefinition talent in talents)
                CreateNode(talent);

            foreach (TalentDefinition child in talents)
            {
                foreach (TalentId parentId in child.Prerequisites)
                {
                    if (!talentsById.TryGetValue(parentId, out TalentDefinition parent))
                        continue;

                    CreateConnection(parent, child);
                }
            }
        }

        private void CreateNode(TalentDefinition talent)
        {
            TalentNodeView node = Instantiate(NodePrefab, NodesRoot);
            Vector2 pos = talent.GraphPosition;
            pos.y = -pos.y;
            node.RectTransform.anchoredPosition = pos;
            node.Initialize(this, talent);
            _nodesById[talent.Id] = node;
        }

        private void CreateConnection(TalentDefinition parent, TalentDefinition child)
        {
            TalentConnectionView connection = Instantiate(ConnectionPrefab, ConnectionsRoot);
            Vector2 from = parent.GraphPosition;
            Vector2 to = child.GraphPosition;
            from.y = -from.y;
            to.y = -to.y;
            connection.Initialize(from, to);
            _connections.Add(new TalentConnectionBinding(parent.Id, child.Id, connection));
        }

        private void Refresh()
        {
            GoldLabel.text = $"{_talentService.Gold} золота";

            foreach (TalentDefinition talent in _talentService.Talents)
            {
                int level = _talentService.LevelOf(talent.Id);
                _nodesById[talent.Id].Refresh(talent, level, _talentService.CanBuy(talent.Id));
            }

            foreach (TalentConnectionBinding connection in _connections)
            {
                bool parentBought = _talentService.LevelOf(connection.ParentId) > 0;
                bool childBought = _talentService.LevelOf(connection.ChildId) > 0;
                connection.View.Refresh(parentBought, childBought);
            }
        }

        private void Close()
        {
            _windowService.Close(Id);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 newPos = NodesRoot.anchoredPosition + eventData.delta / _zoom;

            float maxPanX = NodesRoot.rect.width * 0.5f;
            float maxPanY = NodesRoot.rect.height * 0.5f;
            newPos.x = Mathf.Clamp(newPos.x, -maxPanX, maxPanX);
            newPos.y = Mathf.Clamp(newPos.y, -maxPanY, maxPanY);

            NodesRoot.anchoredPosition = newPos;
            ConnectionsRoot.anchoredPosition = newPos;
        }
    }
}
