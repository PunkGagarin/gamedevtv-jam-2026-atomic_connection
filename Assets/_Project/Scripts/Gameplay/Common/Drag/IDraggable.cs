using UnityEngine;

namespace _Project.Scripts.Gameplay.Drag
{
    public interface IDraggable
    {
        bool CanStartDrag { get; }
        Transform Transform { get; }
        void OnDragStart();
        void OnDragMove(Vector3 worldPosition);
        void OnDragEnd();
        void OnDragCancel();
    }

    public interface IDragReleaseHandler
    {
        bool TryHandleDragRelease(Vector3 worldPosition);
    }
}
