namespace _Project.Scripts.Gameplay.Drag
{
    public interface IDragService
    {
        bool DragWasStartedThisPress { get; }
        bool IsDragActive { get; }

        bool IsReserved(IDraggable draggable);
        void Update();
        void CancelDrag();
    }
}
