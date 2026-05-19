using _Project.Scripts.Gameplay.Cameras.Provider;
using _Project.Scripts.Gameplay.Common.Physics;
using _Project.Scripts.Gameplay.Input.Service;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Drag
{
    public class DragService : IDragService
    {
        private const float MIN_DRAG_DISTANCE_PIXELS = 6f;

        private static readonly Collider2D[] OverlapHits = new Collider2D[32];

        private DragStartCandidate _pendingCandidate;
        private IDraggable _currentDraggable;
        private Vector2 _pendingScreenPosition;
        private bool _dragWasStartedThisPress;

        [Inject] private IPhysicsService _physicsService;
        [Inject] private IInputService _inputService;
        [Inject] private ICameraProvider _cameraProvider;

        private bool IsDragging => _currentDraggable != null;
        private bool HasPendingDrag => _pendingCandidate.Draggable != null;
        public bool DragWasStartedThisPress => _dragWasStartedThisPress;
        public bool IsDragActive => IsDragging || HasPendingDrag;

        public void Update()
        {
            Camera camera = _cameraProvider.MainCamera;
            if (camera == null)
                return;

            Vector2 screenPosition = _inputService.GetScreenMousePosition();

            if (!IsDragging)
            {
                if (_inputService.GetLeftMouseButtonDown())
                {
                    _dragWasStartedThisPress = false;
                    TryPrepareDrag(screenPosition, camera);
                }
                else if (HasPendingDrag)
                    UpdatePendingDrag(screenPosition, camera);
            }
            else
            {
                UpdateDrag(screenPosition, camera);

                if (_inputService.GetLeftMouseButtonUpRaw())
                    EndDrag(screenPosition, camera);
            }

            if (!IsDragging && !HasPendingDrag && !_inputService.GetLeftMouseButtonRaw() && !_inputService.GetLeftMouseButtonUpRaw())
                _dragWasStartedThisPress = false;
        }

        private bool TryPrepareDrag(Vector2 screenPosition, Camera camera)
        {
            if (_currentDraggable != null || HasPendingDrag)
                return false;

            Vector3 worldPosition = camera.ScreenToWorldPoint(screenPosition);
            Collider2D hit = _physicsService.OverlapPoint(worldPosition, ~0);

            if (hit == null)
                return false;

            DragStartCandidate candidate = GetDragStartCandidateAt(worldPosition);
            if (candidate.Draggable == null)
                return false;

            _pendingCandidate = candidate;
            _pendingScreenPosition = screenPosition;

            return true;
        }

        private void UpdatePendingDrag(Vector2 screenPosition, Camera camera)
        {
            if (_inputService.GetLeftMouseButtonUpRaw())
            {
                ClearPendingDrag();
                return;
            }

            if ((screenPosition - _pendingScreenPosition).sqrMagnitude < MIN_DRAG_DISTANCE_PIXELS * MIN_DRAG_DISTANCE_PIXELS)
                return;

            StartPendingDrag(screenPosition, camera);
        }

        private bool StartPendingDrag(Vector2 screenPosition, Camera camera)
        {
            IDraggable draggable = _pendingCandidate.Draggable;
            if (draggable == null || (_pendingCandidate.RequiresCanStartDrag && !draggable.CanStartDrag))
            {
                ClearPendingDrag();
                return false;
            }

            _currentDraggable = draggable;
            _dragWasStartedThisPress = true;
            ClearPendingDrag();
            draggable.OnDragStart();
            draggable.OnDragMove(GetDragWorldPosition(screenPosition, camera, draggable));

            return true;
        }

        private IDropTarget UpdateDrag(Vector2 screenPosition, Camera camera)
        {
            if (_currentDraggable == null)
                return null;

            Vector3 worldPosition = GetDragWorldPosition(screenPosition, camera, _currentDraggable);
            _currentDraggable.OnDragMove(worldPosition);

            return GetDropTargetAt(worldPosition);
        }

        private IDropTarget EndDrag(Vector2 screenPosition, Camera camera)
        {
            if (_currentDraggable == null)
                return null;

            Vector3 worldPosition = GetDragWorldPosition(screenPosition, camera, _currentDraggable);
            IDropTarget target = GetDropTargetAt(worldPosition);

            if (target != null && target.CanAcceptDrop(_currentDraggable))
            {
                target.OnDropAccepted(_currentDraggable);
                _currentDraggable.OnDragEnd();
            }
            else if (_currentDraggable is IDragReleaseHandler releaseHandler
                     && releaseHandler.TryHandleDragRelease(worldPosition))
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

        private static Vector3 GetDragWorldPosition(Vector2 screenPosition, Camera camera, IDraggable draggable)
        {
            Vector3 worldPosition = camera.ScreenToWorldPoint(screenPosition);
            worldPosition.z = draggable.Transform.position.z;
            return worldPosition;
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

        private DragStartCandidate GetDragStartCandidateAt(Vector3 worldPosition)
        {
            int hitCount = _physicsService.OverlapPointNonAlloc(worldPosition, OverlapHits, ~0);

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hit = OverlapHits[i];
                if (hit == null)
                    continue;

                IDragSource dragSource = hit.GetComponentInParent<IDragSource>();
                IDraggable draggable = dragSource?.GetDraggable();
                if (draggable != null)
                    return new DragStartCandidate(draggable, false);
            }

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hit = OverlapHits[i];
                if (hit == null)
                    continue;

                IDraggable draggable = hit.GetComponent<IDraggable>();
                if (draggable != null)
                    return new DragStartCandidate(draggable, true);
            }

            return default;
        }

        public void CancelDrag()
        {
            if (_currentDraggable == null)
            {
                ClearPendingDrag();
                return;
            }

            _currentDraggable.OnDragCancel();
            _currentDraggable = null;
            ClearPendingDrag();
        }

        private void ClearPendingDrag()
        {
            _pendingCandidate = default;
            _pendingScreenPosition = default;
        }

        private readonly struct DragStartCandidate
        {
            public DragStartCandidate(IDraggable draggable, bool requiresCanStartDrag)
            {
                Draggable = draggable;
                RequiresCanStartDrag = requiresCanStartDrag;
            }

            public IDraggable Draggable { get; }
            public bool RequiresCanStartDrag { get; }
        }
    }
}
