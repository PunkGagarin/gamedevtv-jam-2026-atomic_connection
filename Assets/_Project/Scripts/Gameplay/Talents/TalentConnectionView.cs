using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Gameplay.Talents
{
    public class TalentConnectionView : MonoBehaviour
    {
        private static readonly Color LockedColor = new(0.25f, 0.06f, 0.05f, 1f);
        private static readonly Color AvailableColor = new(0.95f, 0.82f, 0.28f, 1f);
        private static readonly Color BoughtColor = new(0.55f, 0.95f, 0.45f, 1f);

        [field: SerializeField] private RectTransform RectTransform { get; set; }
        [field: SerializeField] private Image LineImage { get; set; }

        public void Initialize(Vector2 from, Vector2 to)
        {
            Vector2 direction = to - from;
            RectTransform.anchoredPosition = from + direction * 0.5f;
            RectTransform.sizeDelta = new Vector2(direction.magnitude, RectTransform.sizeDelta.y);
            RectTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        }

        public void Refresh(bool parentBought, bool childBought)
        {
            if (LineImage == null)
                return;

            LineImage.color = childBought ? BoughtColor : parentBought ? AvailableColor : LockedColor;
        }
    }
}
