using UnityEngine;

namespace _Project.Scripts.Gameplay.Drag
{
    public interface IDraggable
    {
        Transform Transform { get; }
        void OnDragStart();
        void OnDragMove(Vector3 worldPosition);
        void OnDragEnd();
        void OnDragCancel();
    }
}
