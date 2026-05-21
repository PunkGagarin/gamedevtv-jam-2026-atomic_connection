using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Scripts.Gameplay.Windows
{
    public class WindowBackdrop : MonoBehaviour, IPointerClickHandler
    {
        private BaseWindow _window;

        public void Initialize(BaseWindow window) =>
            _window = window;

        public void OnPointerClick(PointerEventData eventData) =>
            _window?.OnBackdropClicked();
    }
}
