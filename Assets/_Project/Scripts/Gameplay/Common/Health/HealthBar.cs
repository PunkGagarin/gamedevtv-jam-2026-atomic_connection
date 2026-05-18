using _Project.Scripts.Gameplay.UI;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Common.Health
{
    public class HealthBar : MonoBehaviour
    {
        [field: SerializeField] private Health Health { get; set; }
        [field: SerializeField] private ProgressBar ProgressBar { get; set; }
        [field: SerializeField] private bool HideWhenSinglePoint { get; set; }

        private void Awake()
        {
            if (Health == null)
                Health = GetComponentInParent<Health>();
        }

        private void OnEnable()
        {
            if (Health != null)
                Health.Changed += Refresh;

            Refresh();
        }

        private void OnDisable()
        {
            if (Health != null)
                Health.Changed -= Refresh;
        }

        private void Refresh()
        {
            if (Health == null || ProgressBar == null)
                return;

            ProgressBar.gameObject.SetActive(!HideWhenSinglePoint || Health.MaxHealth > 1);
            ProgressBar.SetProgress((float)Health.CurrentHealth / Health.MaxHealth);
        }
    }
}
