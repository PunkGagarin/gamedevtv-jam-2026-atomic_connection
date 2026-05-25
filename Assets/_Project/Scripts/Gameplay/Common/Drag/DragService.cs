using System.Collections.Generic;
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
        private readonly List<Collider2D> _hiddenDragColliders = new();
        private readonly List<bool> _hiddenDragColliderStates = new();
        private Vector2 _pendingScreenPosition;
        private bool _dragWasStartedThisPress;

        [Inject] private IPhysicsService _physicsService;
        [Inject] private IInputService _inputService;
        [Inject] private ICameraProvider _cameraProvider;

        private bool HasCurrentDrag => _currentDraggable != null;
        private bool HasPendingDrag => _pendingCandidate.Draggable != null;
        public bool DragWasStartedThisPress => _dragWasStartedThisPress;
        public bool IsDragActive => HasCurrentDrag || HasPendingDrag;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            System.Array.Clear(OverlapHits, 0, OverlapHits.Length);
        }

        public bool IsReserved(IDraggable draggable)
        {
            return draggable != null &&
                   (_currentDraggable == draggable || _pendingCandidate.Draggable == draggable);
        }

        public bool IsDragging(IDraggable draggable)
        {
            return draggable != null && _currentDraggable == draggable;
        }

        public void Update()
        {
            Camera camera = _cameraProvider.MainCamera;
            if (camera == null)
                return;

            Vector2 screenPosition = _inputService.GetScreenMousePosition();

            if (!HasCurrentDrag)
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

            if (!HasCurrentDrag && !HasPendingDrag && !_inputService.GetLeftMouseButtonRaw() && !_inputService.GetLeftMouseButtonUpRaw())
                _dragWasStartedThisPress = false;
        }

        private bool TryPrepareDrag(Vector2 screenPosition, Camera camera)
        {
            if (_currentDraggable != null || HasPendingDrag)
                return false;

            Vector3 worldPosition = camera.ScreenToWorldPoint(screenPosition);
            _physicsService.SyncTransforms();
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
            if (!TryGetDragTransform(draggable, out Transform dragTransform) ||
                (_pendingCandidate.RequiresCanStartDrag && !draggable.CanStartDrag))
            {
                ClearPendingDrag();
                return false;
            }

            _currentDraggable = draggable;
            _dragWasStartedThisPress = true;
            ClearPendingDrag();
            draggable.OnDragStart();
            draggable.OnDragMove(GetDragWorldPosition(screenPosition, camera, dragTransform));

            return true;
        }

        private IDropTarget UpdateDrag(Vector2 screenPosition, Camera camera)
        {
            IDraggable draggable = _currentDraggable;
            if (!TryGetDragTransform(draggable, out Transform dragTransform))
            {
                _currentDraggable = null;
                return null;
            }

            Vector3 worldPosition = GetDragWorldPosition(screenPosition, camera, dragTransform);
            draggable.OnDragMove(worldPosition);

            return GetDropTargetAt(worldPosition, draggable, dragTransform);
        }

        private IDropTarget EndDrag(Vector2 screenPosition, Camera camera)
        {
            IDraggable draggable = _currentDraggable;
            if (!TryGetDragTransform(draggable, out Transform dragTransform))
            {
                _currentDraggable = null;
                return null;
            }

            Vector3 worldPosition = GetDragWorldPosition(screenPosition, camera, dragTransform);
            IDropTarget target = GetDropTargetAt(worldPosition, draggable, dragTransform);

            if (target != null && target.CanAcceptDrop(draggable))
            {
                target.OnDropAccepted(draggable);

                if (_currentDraggable == draggable)
                    draggable.OnDragEnd();
            }
            else if (draggable is IDragReleaseHandler releaseHandler
                     && releaseHandler.TryHandleDragRelease(worldPosition))
            {
                if (_currentDraggable == draggable)
                    draggable.OnDragEnd();
            }
            else
            {
                target?.OnDropRejected(draggable);

                if (_currentDraggable == draggable)
                    draggable.OnDragCancel();
            }

            IDropTarget result = target;

            if (_currentDraggable == draggable)
                _currentDraggable = null;

            return result;
        }

        private static Vector3 GetDragWorldPosition(Vector2 screenPosition, Camera camera, Transform dragTransform)
        {
            Vector3 worldPosition = camera.ScreenToWorldPoint(screenPosition);
            worldPosition.z = dragTransform.position.z;
            return worldPosition;
        }

        private IDropTarget GetDropTargetAt(Vector3 worldPosition, IDraggable draggable, Transform dragTransform)
        {
            Collider2D col = dragTransform.GetComponent<Collider2D>();
            ColliderSet colliderSet = dragTransform.GetComponent<ColliderSet>();
            bool colliderWasEnabled = col != null && col.enabled;

            if (colliderSet != null)
            {
                colliderSet.CaptureEnabledStates(_hiddenDragColliders, _hiddenDragColliderStates);
                colliderSet.SetEnabled(false);
            }
            else if (col != null)
                col.enabled = false;

            IDropTarget target = null;
            _physicsService.SyncTransforms();
            int hitCount = _physicsService.OverlapPointNonAlloc(worldPosition, OverlapHits, ~0);

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hit = OverlapHits[i];
                if (hit == null || hit == col)
                    continue;

                IDropTarget candidate = hit.GetComponent<IDropTarget>() ?? hit.GetComponentInParent<IDropTarget>();
                if (candidate == null)
                    continue;

                target ??= candidate;

                if (candidate.CanAcceptDrop(draggable))
                {
                    target = candidate;
                    break;
                }
            }

            if (colliderSet != null)
                colliderSet.RestoreEnabledStates(_hiddenDragColliders, _hiddenDragColliderStates);
            else if (col != null)
                col.enabled = colliderWasEnabled;

            return target;
        }

        private static bool TryGetDragTransform(IDraggable draggable, out Transform dragTransform)
        {
            dragTransform = null;

            if (draggable == null)
                return false;

            if (draggable is UnityEngine.Object unityObject && unityObject == null)
                return false;

            dragTransform = draggable.Transform;
            return dragTransform != null;
        }

        private DragStartCandidate GetDragStartCandidateAt(Vector3 worldPosition)
        {
            _physicsService.SyncTransforms();
            int hitCount = _physicsService.OverlapPointNonAlloc(worldPosition, OverlapHits, ~0);

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hit = OverlapHits[i];
                if (hit == null)
                    continue;

                IDraggable directDraggable = hit.GetComponent<IDraggable>();
                if (directDraggable != null && directDraggable.CanStartDrag)
                    return new DragStartCandidate(directDraggable, true);

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
