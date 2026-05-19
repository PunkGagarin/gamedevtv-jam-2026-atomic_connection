using UnityEngine;

namespace _Project.Scripts.Gameplay.Input.Service
{
    public interface IInputService
    {
        float GetVerticalAxis();
        float GetHorizontalAxis();
        bool HasAxisInput();

        bool GetLeftMouseButtonDown();
        bool GetLeftMouseButtonRaw();
        Vector2 GetScreenMousePosition();
        Vector2 GetWorldMousePosition();
        bool GetLeftMouseButtonUp();
        bool GetLeftMouseButtonUpRaw();
    }
}
