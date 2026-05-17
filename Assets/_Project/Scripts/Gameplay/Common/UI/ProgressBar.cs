using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Gameplay.UI
{
    public class ProgressBar : MonoBehaviour
    {
        [field: SerializeField] private Image Fill { get; set; }

        public void SetProgress(float value)
        {
            if (Fill == null)
                return;

            value = Mathf.Clamp01(value);
            Vector2 anchorMax = Fill.rectTransform.anchorMax;
            anchorMax.x = value;
            Fill.rectTransform.anchorMax = anchorMax;
        }
    }
}
