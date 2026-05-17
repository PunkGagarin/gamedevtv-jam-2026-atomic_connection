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

            return GetDropTargetAt(worldPosition);
        }

        public IDropTarget EndDrag(Vector2 screenPosition, Camera camera)
        {
            if (_currentDraggable == null)
                return null;

            Vector3 worldPosition = camera.ScreenToWorldPoint(screenPosition);
            IDropTarget target = GetDropTargetAt(worldPosition);

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

        private IDropTarget GetDropTargetAt(Vector3 worldPosition)
        {
            Collider2D col = _currentDraggable.Transform.GetComponent<Collider2D>();
            if (col != null)
                col.enabled = false;

            Collider2D hit = _physicsService.OverlapPoint(worldPosition, ~0);
            IDropTarget target = hit != null ? hit.GetComponent<IDropTarget>() : null;

            if (col != null)
                col.enabled = true;

            return target;
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
