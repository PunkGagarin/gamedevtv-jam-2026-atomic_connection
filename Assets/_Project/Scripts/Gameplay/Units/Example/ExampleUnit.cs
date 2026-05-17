using _Project.Scripts.Gameplay.UI;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.Example
{
    public class ExampleUnit : MonoBehaviour
    {
        [field: SerializeField] private ProgressBar ProgressBar { get; set; }

        public void SetClickProgress(int clicksRemaining, int clicksRequired)
        {
            if (ProgressBar == null)
                return;

            ProgressBar.SetProgress((float)clicksRemaining / clicksRequired);
        }
    }
}
