using UnityEngine;
using UnityEngine.UI;
using _Project.Scripts.Localization;

namespace _Project.Scripts.Gameplay.Tutorial
{
    public class GameplayTutorialView : MonoBehaviour
    {
        [field: SerializeField] private CanvasGroup CanvasGroup { get; set; }
        [field: SerializeField] private RectTransform OverlayRoot { get; set; }
        [field: SerializeField] private RectTransform Cursor { get; set; }
        [field: SerializeField] private Image CursorImage { get; set; }
        [field: SerializeField] private Sprite CursorReleasedSprite { get; set; }
        [field: SerializeField] private Sprite CursorPressedSprite { get; set; }
        [field: SerializeField] private RectTransform CursorIcon { get; set; }
        [field: SerializeField] private Image CursorIconImage { get; set; }
        [field: SerializeField] private RectTransform TargetIcon { get; set; }
        [field: SerializeField] private Image TargetIconImage { get; set; }
        [field: SerializeField] private RectTransform DragPath { get; set; }
        [field: SerializeField] private Image DragPathImage { get; set; }
        [field: SerializeField] private RectTransform TopHint { get; set; }
        [field: SerializeField] private ToLocalize TopHintLocalize { get; set; }

        private TutorialHintMode _hintMode;
        private float _hintTime;
        private Vector2 _cursorIconOffset;
        private Canvas _canvas;
        private GameplayTutorialConfig _config;

        private void Awake()
        {
            Hide();
        }

        public void ShowCoreHint(
            Vector2 screenPosition,
            GameplayTutorialConfig config,
            GameplayTutorialStepVisual visual)
        {
            _config = config;
            if (_hintMode != TutorialHintMode.Pulse)
                _hintTime = 0f;

            _hintMode = TutorialHintMode.Pulse;
            ApplyVisual(visual, false);
            SetVisible(true);
            SetDragPathVisible(false);
            SetCursorPressed(false);
            SetCursorScreenPosition(screenPosition);
            SetTargetIconVisible(false);
            SetTopHintVisible(false);
        }

        public void ShowDragHint(
            Vector2 fromScreenPosition,
            Vector2 toScreenPosition,
            GameplayTutorialConfig config,
            GameplayTutorialStepVisual visual,
            Vector2? targetIconScreenPosition = null)
        {
            _config = config;
            if (_hintMode != TutorialHintMode.Drag)
                _hintTime = 0f;

            _hintMode = TutorialHintMode.Drag;
            ApplyVisual(visual, true);
            Cursor.localScale = Vector3.one * _config.CursorBaseScale;
            SetVisible(true);
            SetDragPathVisible(true);
            SetDragPath(fromScreenPosition, toScreenPosition);
            SetTargetIcon(targetIconScreenPosition, visual);
            SetTopHintVisible(false);
            UpdateDragCursor(fromScreenPosition, toScreenPosition);
        }

        public void ShowActiveMoleculeHint(
            Vector2 screenPosition,
            GameplayTutorialConfig config,
            GameplayTutorialStepVisual visual,
            string textKey)
        {
            _config = config;
            if (_hintMode != TutorialHintMode.Pulse)
                _hintTime = 0f;

            _hintMode = TutorialHintMode.Pulse;
            ApplyVisual(visual, false);
            SetVisible(true);
            SetDragPathVisible(false);
            SetCursorPressed(false);
            SetCursorScreenPosition(screenPosition);
            SetTargetIconVisible(false);
            SetTopHint(textKey);
        }

        public void Tick(float deltaTime)
        {
            if (_hintMode == TutorialHintMode.None || _config == null)
                return;

            _hintTime += Mathf.Max(0f, deltaTime);

            if (_hintMode == TutorialHintMode.Pulse)
            {
                float phase = Mathf.PingPong(_hintTime / _config.PulseDurationSeconds, 1f);
                float scale = Mathf.Lerp(_config.CursorBaseScale, _config.CursorPulseScale, phase);
                Cursor.localScale = Vector3.one * scale;
            }
        }

        public void Hide()
        {
            _hintMode = TutorialHintMode.None;
            SetVisible(false);
            SetDragPathVisible(false);
            SetTargetIconVisible(false);
            SetTopHintVisible(false);
        }

        private void UpdateDragCursor(Vector2 fromScreenPosition, Vector2 toScreenPosition)
        {
            float duration = Mathf.Max(0.01f, _config.DragLoopDurationSeconds);
            float phase = (_hintTime % duration) / duration;
            float travelPhase = Mathf.SmoothStep(0f, 1f, phase);
            Vector2 position = Vector2.Lerp(fromScreenPosition, toScreenPosition, travelPhase);

            float pressedPhase = _config.DragPressedFrameSeconds / duration;
            float releasePhase = 1f - _config.DragReleaseFrameSeconds / duration;
            SetCursorPressed(phase > pressedPhase && phase < releasePhase);
            SetCursorScreenPosition(position);
        }

        private void SetCursorScreenPosition(Vector2 screenPosition)
        {
            Vector2 anchoredPosition = ClampToOverlay(ToAnchoredPosition(screenPosition));
            Cursor.anchoredPosition = anchoredPosition;

            if (CursorIcon != null && CursorIcon.gameObject.activeSelf)
                CursorIcon.anchoredPosition = anchoredPosition + _cursorIconOffset;
        }

        private void SetDragPath(Vector2 fromScreenPosition, Vector2 toScreenPosition)
        {
            Vector2 from = ClampToOverlay(ToAnchoredPosition(fromScreenPosition));
            Vector2 to = ClampToOverlay(ToAnchoredPosition(toScreenPosition));
            Vector2 direction = to - from;

            if (DragPathImage != null && _config.DragPathSprite != null)
                DragPathImage.sprite = _config.DragPathSprite;

            DragPath.anchoredPosition = from + direction * 0.5f;
            DragPath.sizeDelta = _config.DragPathSize;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + _config.DragPathAngleOffsetDegrees;
            DragPath.localRotation = Quaternion.Euler(0f, 0f, angle);
        }

        private Vector2 ToAnchoredPosition(Vector2 screenPosition)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                OverlayRoot,
                screenPosition,
                UiCamera,
                out Vector2 localPosition);
            return localPosition;
        }

        private Camera UiCamera
        {
            get
            {
                if (_canvas == null)
                    _canvas = GetComponentInParent<Canvas>();

                if (_canvas == null || _canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                    return null;

                return _canvas.worldCamera;
            }
        }

        private Vector2 ClampToOverlay(Vector2 anchoredPosition)
        {
            Rect rect = OverlayRoot.rect;
            float padding = _config != null ? _config.ScreenPadding : 0f;
            return new Vector2(
                Mathf.Clamp(anchoredPosition.x, rect.xMin + padding, rect.xMax - padding),
                Mathf.Clamp(anchoredPosition.y, rect.yMin + padding, rect.yMax - padding));
        }

        private void SetVisible(bool isVisible)
        {
            gameObject.SetActive(isVisible);
            CanvasGroup.alpha = isVisible ? 1f : 0f;
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;
        }

        private void SetDragPathVisible(bool isVisible)
        {
            DragPath.gameObject.SetActive(isVisible);
            if (DragPathImage != null)
                DragPathImage.enabled = isVisible;
        }

        private void SetCursorPressed(bool isPressed)
        {
            Sprite sprite = isPressed ? CursorPressedSprite : CursorReleasedSprite;
            if (CursorImage != null && sprite != null)
                CursorImage.sprite = sprite;
        }

        private void ApplyVisual(GameplayTutorialStepVisual visual, bool isDrag)
        {
            if (visual == null)
                return;

            CursorReleasedSprite = visual.ReleasedCursorSprite != null ? visual.ReleasedCursorSprite : visual.CursorSprite;
            CursorPressedSprite = visual.PressedCursorSprite != null ? visual.PressedCursorSprite : CursorReleasedSprite;

            if (CursorImage != null && CursorReleasedSprite != null)
                CursorImage.sprite = CursorReleasedSprite;

            SetCursorIcon(visual, isDrag);
        }

        private void SetCursorIcon(GameplayTutorialStepVisual visual, bool isVisible)
        {
            bool showIcon = isVisible && visual != null && visual.CursorIconSprite != null && CursorIcon != null && CursorIconImage != null;
            if (CursorIcon != null)
                CursorIcon.gameObject.SetActive(showIcon);

            if (!showIcon)
                return;

            CursorIconImage.sprite = visual.CursorIconSprite;
            CursorIcon.sizeDelta = Vector2.one * visual.CursorIconSize;
            _cursorIconOffset = visual.CursorIconOffset;
        }

        private void SetTargetIcon(Vector2? screenPosition, GameplayTutorialStepVisual visual)
        {
            bool showIcon = screenPosition.HasValue &&
                            visual != null &&
                            visual.TargetIconSprite != null &&
                            TargetIcon != null &&
                            TargetIconImage != null;
            SetTargetIconVisible(showIcon);

            if (!showIcon)
                return;

            TargetIconImage.sprite = visual.TargetIconSprite;
            TargetIcon.sizeDelta = Vector2.one * visual.TargetIconSize;
            TargetIcon.anchoredPosition = ClampToOverlay(ToAnchoredPosition(screenPosition.Value)) + visual.TargetIconOffset;
        }

        private void SetTargetIconVisible(bool isVisible)
        {
            if (TargetIcon != null)
                TargetIcon.gameObject.SetActive(isVisible);
        }

        private void SetTopHint(string textKey)
        {
            SetTopHintVisible(true);

            if (TopHintLocalize != null)
                TopHintLocalize.SetKey(textKey);
        }

        private void SetTopHintVisible(bool isVisible)
        {
            if (TopHint != null)
                TopHint.gameObject.SetActive(isVisible);
        }

        private enum TutorialHintMode
        {
            None = 0,
            Pulse = 1,
            Drag = 2
        }
    }
}
