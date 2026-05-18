using _Project.Scripts.Gameplay.Cameras.Provider;
using _Project.Scripts.Gameplay.Common.Physics;
using _Project.Scripts.Gameplay.Input.Service;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Drag
{
    public class DragService : IDragService
    {
        private static readonly Collider2D[] OverlapHits = new Collider2D[32];

        private IDraggable _currentDraggable;
        private Vector3 _dragOffset;

        [Inject] private IPhysicsService _physicsService;
        [Inject] private IInputService _inputService;
        [Inject] private ICameraProvider _cameraProvider;

        private bool IsDragging => _currentDraggable != null;

        public void Update()
        {
            Camera camera = _cameraProvider.MainCamera;
            if (camera == null)
                return;

            Vector2 screenPosition = _inputService.GetScreenMousePosition();

            if (!IsDragging)
            {
                if (_inputService.GetLeftMouseButtonDown())
                    TryStartDrag(screenPosition, camera);
            }
            else
            {
                UpdateDrag(screenPosition, camera);

                if (_inputService.GetLeftMouseButtonUpRaw())
                    EndDrag(screenPosition, camera);
            }
        }

        private bool TryStartDrag(Vector2 screenPosition, Camera camera)
        {
            if (_currentDraggable != null)
                return false;

            Vector3 worldPosition = camera.ScreenToWorldPoint(screenPosition);
            Collider2D hit = _physicsService.OverlapPoint(worldPosition, ~0);

            if (hit == null)
                return false;

            IDraggable draggable = GetDraggableAt(worldPosition);
            if (draggable == null || !draggable.CanStartDrag)
                return false;

            _currentDraggable = draggable;
            _dragOffset = draggable.Transform.position - worldPosition;
            draggable.OnDragStart();

            return true;
        }

        private IDropTarget UpdateDrag(Vector2 screenPosition, Camera camera)
        {
            if (_currentDraggable == null)
                return null;

            Vector3 worldPosition = camera.ScreenToWorldPoint(screenPosition);
            _currentDraggable.OnDragMove(worldPosition + _dragOffset);

            return GetDropTargetAt(worldPosition);
        }

        private IDropTarget EndDrag(Vector2 screenPosition, Camera camera)
        {
            if (_currentDraggable == null)
                return null;

            Vector3 worldPosition = camera.ScreenToWorldPoint(screenPosition);
            Vector3 draggablePosition = worldPosition + _dragOffset;
            IDropTarget target = GetDropTargetAt(worldPosition);

            if (target != null && target.CanAcceptDrop(_currentDraggable))
            {
                target.OnDropAccepted(_currentDraggable);
                _currentDraggable.OnDragEnd();
            }
            else if (_currentDraggable is IDragReleaseHandler releaseHandler
                     && releaseHandler.TryHandleDragRelease(draggablePosition))
            {
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

            IDropTarget target = null;
            int hitCount = _physicsService.OverlapPointNonAlloc(worldPosition, OverlapHits, ~0);

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hit = OverlapHits[i];
                if (hit == null || hit == col)
                    continue;

                target = hit.GetComponent<IDropTarget>();
                if (target != null)
                    break;
            }

            if (col != null)
                col.enabled = true;

            return target;
        }

        private IDraggable GetDraggableAt(Vector3 worldPosition)
        {
            int hitCount = _physicsService.OverlapPointNonAlloc(worldPosition, OverlapHits, ~0);

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hit = OverlapHits[i];
                if (hit == null)
                    continue;

                IDraggable draggable = hit.GetComponent<IDraggable>();
                if (draggable != null)
                    return draggable;
            }

            return null;
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
