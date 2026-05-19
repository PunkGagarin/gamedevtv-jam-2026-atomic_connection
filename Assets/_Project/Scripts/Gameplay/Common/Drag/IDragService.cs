namespace _Project.Scripts.Gameplay.Drag
{
    public interface IDragService
    {
        bool DragWasStartedThisPress { get; }
        bool IsDragActive { get; }

        void Update();
        void CancelDrag();
    }
}
