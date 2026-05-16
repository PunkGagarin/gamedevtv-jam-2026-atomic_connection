using _Project.Scripts.Gameplay.Common.Physics;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Drag
{
    public class DragService : IDragService
    {
        private IDraggable _currentDraggable;
        private Vector3 _dragOffset;

        [Inject] private IPhysicsService _physicsService;

        public bool IsDragging => _currentDraggable != null;
        public IDraggable CurrentDraggable => _currentDraggable;

        public bool TryStartDrag(Vector2 screenPosition, Camera camera)
        {
            if (_currentDraggable != null)
                return false;

            Vector3 worldPosition = camera.ScreenToWorldPoint(screenPosition);
            Collider2D hit = _physicsService.OverlapPoint(worldPosition, ~0);

            if (hit == null)
                return false;

            IDraggable draggable = hit.GetComponent<IDraggable>();
            if (draggable == null)
                return false;

            _currentDraggable = draggable;
            _dragOffset = draggable.Transform.position - worldPosition;
            draggable.OnDragStart();

            return true;
        }

        public IDropTarget UpdateDrag(Vector2 screenPosition, Camera camera)
        {
            if (_currentDraggable == null)
                return null;

            Vector3 worldPosition = camera.ScreenToWorldPoint(screenPosition);
            _currentDraggable.OnDragMove(worldPosition + _dragOffset);

            Collider2D hit = _physicsService.OverlapPoint(worldPosition, ~0);
            return hit != null ? hit.GetComponent<IDropTarget>() : null;
        }

        public IDropTarget EndDrag(Vector2 screenPosition, Camera camera)
        {
            if (_currentDraggable == null)
                return null;

            Vector3 worldPosition = camera.ScreenToWorldPoint(screenPosition);
            Collider2D hit = _physicsService.OverlapPoint(worldPosition, ~0);
            IDropTarget target = hit != null ? hit.GetComponent<IDropTarget>() : null;

            if (target != null && target.CanAcceptDrop(_currentDraggable))
            {
                target.OnDropAccepted(_currentDraggable);
                _currentDraggable.OnDragEnd();
            }
            else
            {
                target?.OnDropRejected(_currentDraggable);
                _currentDraggable.OnDragCancel();
            }

            IDropTarget result = target;
            _currentDraggable = null;
            return result;
        }

        public void CancelDrag()
        {
            if (_currentDraggable == null)
                return;

            _currentDraggable.OnDragCancel();
            _currentDraggable = null;
        }
    }
}
