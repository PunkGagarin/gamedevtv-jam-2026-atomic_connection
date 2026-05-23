using _Project.Scripts.Gameplay.Common.Progress;
using _Project.Scripts.Gameplay.UI;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.AtomCores
{
    public class AtomProductionProgress : MonoBehaviour
    {
        [field: SerializeField] private ProgressBar ProgressBar { get; set; }

        private readonly CompletionThreshold _clicks = new();

        public void Configure(int clicksRequired)
        {
            _clicks.Configure(clicksRequired);
            ResetProgress();
        }

        public bool RegisterClick()
        {
            if (!_clicks.HasRequirement)
                return false;

            bool isComplete = _clicks.Advance();
            UpdateProgressBar();

            if (!isComplete)
                return false;

            ResetProgress();
            return true;
        }

        public void ResetProgress()
        {
            _clicks.Reset();
            UpdateProgressBar();
        }

        private void UpdateProgressBar()
        {
            if (ProgressBar == null || !_clicks.HasRequirement)
                return;

            ProgressBar.SetProgress(_clicks.Normalized);
        }
    }
}
