using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace _Project.Scripts.Gameplay.Talents
{
    public class TalentNodeView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private static readonly Color LockedColor = new(0.11f, 0.08f, 0.11f, 1f);
        private static readonly Color AvailableColor = new(0.18f, 0.25f, 0.18f, 1f);
        private static readonly Color BoughtColor = new(0.12f, 0.2f, 0.32f, 1f);

        [field: SerializeField] public RectTransform RectTransform { get; private set; }
        [field: SerializeField] private Button Button { get; set; }
        [field: SerializeField] private Image Background { get; set; }
        [field: SerializeField] private Image IconImage { get; set; }
        [field: SerializeField] private TextMeshProUGUI TitleLabel { get; set; }
        [field: SerializeField] private TextMeshProUGUI LevelLabel { get; set; }
        [field: SerializeField] private TextMeshProUGUI CostLabel { get; set; }

        private TalentId _talentId;
        private TalentDefinition _talent;
        private TalentTreeWindow _window;

        public void Initialize(TalentTreeWindow window, TalentDefinition talent)
        {
            _window = window;
            _talent = talent;
            _talentId = talent.Id;

            if (IconImage != null && talent.Icon != null)
                IconImage.sprite = talent.Icon;

            Button.onClick.AddListener(OnClicked);
        }

        public void Refresh(TalentDefinition talent, int level, bool canBuy)
        {
            bool isMaxed = level >= talent.MaxLevel;

            if (Background != null)
                Background.color = isMaxed ? BoughtColor : canBuy ? AvailableColor : LockedColor;

            if (LevelLabel != null)
                LevelLabel.text = $"{level}/{talent.MaxLevel}";

            if (CostLabel != null)
                CostLabel.text = isMaxed ? "MAX" : talent.CostForLevel(level).ToString();

            if (Button != null)
                Button.interactable = canBuy;
        }

        private void OnDestroy()
        {
            if (Button != null)
                Button.onClick.RemoveListener(OnClicked);
        }

        private void OnClicked()
        {
            _window.Buy(_talentId);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _window.ShowTooltip(_talent, RectTransform);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _window.HideTooltip();
        }
    }
}
