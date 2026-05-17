using UnityEngine;

namespace _Project.Scripts.Gameplay.Drag
{
    public interface IDragService
    {
        bool IsDragging { get; }
        IDraggable CurrentDraggable { get; }

        bool TryStartDrag(Vector2 screenPosition, Camera camera);
        IDropTarget UpdateDrag(Vector2 screenPosition, Camera camera);
        IDropTarget EndDrag(Vector2 screenPosition, Camera camera);
        void CancelDrag();
    }
}
