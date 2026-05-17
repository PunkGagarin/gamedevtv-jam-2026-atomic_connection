namespace _Project.Scripts.Gameplay.Drag
{
    public interface IDropTarget
    {
        bool CanAcceptDrop(IDraggable draggable);
        void OnDropAccepted(IDraggable draggable);
        void OnDropRejected(IDraggable draggable);
    }
}
