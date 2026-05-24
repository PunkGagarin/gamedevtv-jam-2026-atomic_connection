using UnityEngine;

namespace _Project.Scripts.Utils
{
    public class Screenshot : MonoBehaviour
    {
        [field: SerializeField] private KeyCode CaptureKey { get; set; } = KeyCode.W;
        [field: SerializeField] private bool RequireControlOrCommand { get; set; } = true;
        [field: SerializeField] private string FileName { get; set; } = "screenshot.png";
        [field: SerializeField, Min(1)] private int SuperSize { get; set; } = 1;

        private void Update()
        {
            if (Input.GetKeyDown(CaptureKey) && CanCapture())
                ScreenCapture.CaptureScreenshot(FileName, SuperSize);
        }

        private bool CanCapture() =>
            RequireControlOrCommand == false ||
            Input.GetKey(KeyCode.LeftControl) ||
            Input.GetKey(KeyCode.RightControl) ||
            Input.GetKey(KeyCode.LeftCommand) ||
            Input.GetKey(KeyCode.RightCommand);
    }
}
