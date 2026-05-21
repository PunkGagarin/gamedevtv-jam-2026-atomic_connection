using _Project.Scripts.Gameplay.Cameras.Provider;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace _Project.Scripts.Gameplay.Input.Service
{
    public class StandaloneInputService : IInputService
    {
        [Inject] private ICameraProvider _cameraProvider;

        public Vector2 GetScreenMousePosition() =>
            (Vector2)UnityEngine.Input.mousePosition;

        public Vector2 GetWorldMousePosition()
        {
            Camera camera = _cameraProvider.MainCamera;
            if (camera == null)
                return Vector2.zero;

            Vector3 screenPos = UnityEngine.Input.mousePosition;
            return camera.ScreenToWorldPoint(screenPos);
        }

        public bool HasAxisInput() => GetHorizontalAxis() != 0 || GetVerticalAxis() != 0;

        public float GetVerticalAxis() => UnityEngine.Input.GetAxis("Vertical");
        public float GetHorizontalAxis() => UnityEngine.Input.GetAxis("Horizontal");

        public bool GetLeftMouseButtonDown() =>
            UnityEngine.Input.GetMouseButtonDown(0) && !IsPointerOverGameObject();

        public bool GetLeftMouseButtonRaw() =>
            UnityEngine.Input.GetMouseButton(0);

        public bool GetLeftMouseButtonUp() =>
            UnityEngine.Input.GetMouseButtonUp(0) && !IsPointerOverGameObject();

        public bool GetLeftMouseButtonUpRaw() =>
            UnityEngine.Input.GetMouseButtonUp(0);

        private static bool IsPointerOverGameObject() =>
            EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}
