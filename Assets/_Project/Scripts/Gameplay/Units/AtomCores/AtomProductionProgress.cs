using _Project.Scripts.Gameplay.UI;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.AtomCores
{
    public class AtomProductionProgress : MonoBehaviour
    {
        [field: SerializeField] private ProgressBar ProgressBar { get; set; }

        private int _clicksRemaining;
        private int _clicksRequired;

        public void Configure(int clicksRequired)
        {
            _clicksRequired = clicksRequired;
            ResetProgress();
        }

        public bool RegisterClick()
        {
            if (_clicksRequired <= 0)
                return false;

            _clicksRemaining--;
            UpdateProgressBar();

            if (_clicksRemaining > 0)
                return false;

            ResetProgress();
            return true;
        }

        public void ResetProgress()
        {
            _clicksRemaining = _clicksRequired;
            UpdateProgressBar();
        }

        private void UpdateProgressBar()
        {
            if (ProgressBar == null || _clicksRequired <= 0)
                return;

            ProgressBar.SetProgress(1f - (float)_clicksRemaining / _clicksRequired);
        }
    }
}
