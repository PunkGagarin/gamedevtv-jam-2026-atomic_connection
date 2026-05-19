namespace _Project.Scripts.Gameplay.Drag
{
    public interface IDragService
    {
        bool DragWasStartedThisPress { get; }

        void Update();
        void CancelDrag();
    }
}
