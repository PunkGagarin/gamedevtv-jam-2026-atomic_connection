using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Talents.Editor
{
    [CustomEditor(typeof(TalentConfig))]
    public class TalentConfigEditor : UnityEditor.Editor
    {
        private const float DEFAULT_PREVIEW_HEIGHT = 720f;
        private const float MIN_PREVIEW_HEIGHT = 360f;
        private const float MAX_PREVIEW_HEIGHT = 1600f;
        private const float RESIZE_HANDLE_HEIGHT = 12f;
        private const float NODE_RADIUS = 12f;
        private const float LABEL_WIDTH = 116f;
        private const float LABEL_MIN_HEIGHT = 24f;
        private const float LABEL_MAX_HEIGHT = 52f;
        private const float LABEL_PADDING = 4f;
        private const float LABEL_GAP = 6f;
        private const float GRID_SIZE = 120f;
        private const string ID_FIELD = "<Id>k__BackingField";
        private const string PREREQUISITES_FIELD = "<Prerequisites>k__BackingField";
        private const string GRAPH_POSITION_FIELD = "<GraphPosition>k__BackingField";
        private const string PREVIEW_HEIGHT_PREF_KEY = "AtomicConnection.TalentConfigEditor.PreviewHeight";

        private SerializedProperty _talentsProp;
        private int _selectedIndex = -1;
        private bool _isDragging = false;
        private bool _didDragNode = false;
        private int _dragIndex = -1;
        private Vector2 _grabOffset;
        private Vector2 _graphCenter;
        private Vector2 _graphBoundsMin;
        private Vector2 _graphBoundsMax;
        private float _graphScale = 1f;
        private Rect _previewRect;
        private Rect _resizeHandleRect;
        private float _previewHeight = DEFAULT_PREVIEW_HEIGHT;

        private Vector2 _viewOffset;
        private float _panFactor = 1f;
        private bool _isPanning = false;
        private bool _isResizingPreview = false;

        private void OnEnable()
        {
            _talentsProp = serializedObject.FindProperty("<Talents>k__BackingField");
            _previewHeight = Mathf.Clamp(
                EditorPrefs.GetFloat(PREVIEW_HEIGHT_PREF_KEY, DEFAULT_PREVIEW_HEIGHT),
                MIN_PREVIEW_HEIGHT,
                MAX_PREVIEW_HEIGHT);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultFields();
            DrawTalentsList();
            DrawPreviewPanel();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawDefaultFields()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("<ClearSavedProgressOnStartup>k__BackingField"));
        }

        private void DrawTalentsList()
        {
            if (_talentsProp == null)
                return;

            EditorGUILayout.PropertyField(_talentsProp, new GUIContent("Talents"), true);
        }

        private void DrawPreviewPanel()
        {
            if (_talentsProp == null || _talentsProp.arraySize == 0)
                return;

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField(
                "Graph Preview (drag nodes, scroll to zoom, drag bg to pan, drag bottom edge to resize)",
                EditorStyles.boldLabel);

            if (GUILayout.Button("Snap All Nodes To Grid", GUILayout.Width(180f)))
                SnapAllNodesToGrid();

            Rect reserved = EditorGUILayout.BeginVertical();
            GUILayout.Space(_previewHeight + RESIZE_HANDLE_HEIGHT);
            EditorGUILayout.EndVertical();

            Rect indentedReserved = EditorGUI.IndentedRect(
                new Rect(reserved.x, reserved.y, reserved.width, _previewHeight + RESIZE_HANDLE_HEIGHT));
            _previewRect = new Rect(indentedReserved.x, indentedReserved.y, indentedReserved.width, _previewHeight);
            _resizeHandleRect = new Rect(
                _previewRect.x,
                _previewRect.yMax,
                _previewRect.width,
                RESIZE_HANDLE_HEIGHT);

            if (_previewRect.width <= 0 || _previewRect.height <= 0)
                return;

            EditorGUI.DrawRect(_previewRect, new Color(0.08f, 0.08f, 0.1f, 1f));

            Rect topBorder = new Rect(_previewRect.x, _previewRect.y, _previewRect.width, 1f);
            EditorGUI.DrawRect(topBorder, new Color(0.25f, 0.25f, 0.3f, 1f));

            if (!_isDragging && !_isPanning)
                CalculateGraphScale();

            DrawGrid();
            DrawConnections();
            DrawNodes();
            DrawResizeHandle();
            HandleEvents();

            HandleUtility.Repaint();
        }

        private void DrawResizeHandle()
        {
            EditorGUIUtility.AddCursorRect(_resizeHandleRect, MouseCursor.ResizeVertical);
            EditorGUI.DrawRect(_resizeHandleRect, new Color(0.13f, 0.13f, 0.16f, 1f));

            float centerY = _resizeHandleRect.center.y;
            Rect firstLine = new(_resizeHandleRect.center.x - 24f, centerY - 2f, 48f, 1f);
            Rect secondLine = new(_resizeHandleRect.center.x - 24f, centerY + 2f, 48f, 1f);
            Color lineColor = _isResizingPreview
                ? new Color(0.48f, 0.78f, 1f, 1f)
                : new Color(0.38f, 0.38f, 0.46f, 1f);
            EditorGUI.DrawRect(firstLine, lineColor);
            EditorGUI.DrawRect(secondLine, lineColor);
        }

        private void CalculateGraphScale()
        {
            _graphBoundsMin = new Vector2(float.MaxValue, float.MaxValue);
            _graphBoundsMax = new Vector2(float.MinValue, float.MinValue);
            bool hasTalent = false;

            for (int i = 0; i < _talentsProp.arraySize; i++)
            {
                if (!TryGetTalentProperty(i, GRAPH_POSITION_FIELD, out SerializedProperty posProp))
                    continue;

                Vector2 pos = posProp.vector2Value;
                hasTalent = true;

                _graphBoundsMin.x = Mathf.Min(_graphBoundsMin.x, pos.x);
                _graphBoundsMax.x = Mathf.Max(_graphBoundsMax.x, pos.x);
                _graphBoundsMin.y = Mathf.Min(_graphBoundsMin.y, pos.y);
                _graphBoundsMax.y = Mathf.Max(_graphBoundsMax.y, pos.y);
            }

            if (!hasTalent)
            {
                _graphBoundsMin = Vector2.zero;
                _graphBoundsMax = Vector2.one;
            }

            float width = Mathf.Max(_graphBoundsMax.x - _graphBoundsMin.x, 1f);
            float height = Mathf.Max(_graphBoundsMax.y - _graphBoundsMin.y, 1f);

            _graphCenter = new Vector2(
                (_graphBoundsMin.x + _graphBoundsMax.x) * 0.5f,
                (_graphBoundsMin.y + _graphBoundsMax.y) * 0.5f);

            float previewContentWidth = _previewRect.width - 20f;
            float previewContentHeight = _previewRect.height - 20f;
            _graphScale = Mathf.Min(previewContentWidth / width, previewContentHeight / height) * 0.85f;
        }

        private Vector2 GraphToScreen(Vector2 graphPos)
        {
            float effectiveScale = _graphScale * _panFactor;
            return new Vector2(
                _previewRect.center.x + (graphPos.x - _graphCenter.x) * effectiveScale + _viewOffset.x,
                _previewRect.center.y + (graphPos.y - _graphCenter.y) * effectiveScale + _viewOffset.y
            );
        }

        private Vector2 ScreenToGraph(Vector2 screenPos)
        {
            float effectiveScale = _graphScale * _panFactor;
            return new Vector2(
                _graphCenter.x + (screenPos.x - _previewRect.center.x - _viewOffset.x) / effectiveScale,
                _graphCenter.y + (screenPos.y - _previewRect.center.y - _viewOffset.y) / effectiveScale
            );
        }

        private void DrawConnections()
        {
            if (Event.current.type != EventType.Repaint)
                return;

            bool hasSelection = TryGetSelectedTalentId(out int selectedId);
            Dictionary<int, Vector2> talentPositions = new();
            for (int i = 0; i < _talentsProp.arraySize; i++)
            {
                if (!TryGetTalentProperty(i, ID_FIELD, out SerializedProperty idProp) ||
                    !TryGetTalentProperty(i, GRAPH_POSITION_FIELD, out SerializedProperty posProp))
                {
                    continue;
                }

                talentPositions[idProp.intValue] = GraphToScreen(posProp.vector2Value);
            }

            for (int i = 0; i < _talentsProp.arraySize; i++)
            {
                if (!TryGetTalentProperty(i, PREREQUISITES_FIELD, out SerializedProperty prereqsProp) ||
                    !TryGetTalentProperty(i, ID_FIELD, out SerializedProperty idProp))
                {
                    continue;
                }

                if (prereqsProp == null)
                    continue;

                if (!talentPositions.TryGetValue(idProp.intValue, out Vector2 childPos))
                    continue;

                for (int j = 0; j < prereqsProp.arraySize; j++)
                {
                    int parentId = prereqsProp.GetArrayElementAtIndex(j).intValue;

                    if (talentPositions.TryGetValue(parentId, out Vector2 parentPos))
                    {
                        bool isSelectedConnection = hasSelection &&
                                                    (parentId == selectedId || idProp.intValue == selectedId);
                        Vector3 start = new(parentPos.x, parentPos.y, 0f);
                        Vector3 end = new(childPos.x, childPos.y, 0f);
                        Handles.BeginGUI();
                        Handles.color = isSelectedConnection
                            ? new Color(0.95f, 0.78f, 0.3f, 0.95f)
                            : new Color(0.72f, 0.62f, 0.32f, 0.55f);
                        Handles.DrawAAPolyLine(isSelectedConnection ? 3f : 1.5f, start, end);
                        Handles.EndGUI();
                    }
                }
            }
        }

        private void DrawGrid()
        {
            if (Event.current.type != EventType.Repaint)
                return;

            Vector2 graphMin = ScreenToGraph(_previewRect.min);
            Vector2 graphMax = ScreenToGraph(_previewRect.max);

            float minX = Mathf.Floor(Mathf.Min(graphMin.x, graphMax.x) / GRID_SIZE) * GRID_SIZE;
            float maxX = Mathf.Ceil(Mathf.Max(graphMin.x, graphMax.x) / GRID_SIZE) * GRID_SIZE;
            float minY = Mathf.Floor(Mathf.Min(graphMin.y, graphMax.y) / GRID_SIZE) * GRID_SIZE;
            float maxY = Mathf.Ceil(Mathf.Max(graphMin.y, graphMax.y) / GRID_SIZE) * GRID_SIZE;

            Handles.BeginGUI();
            Handles.color = new Color(1f, 1f, 1f, 0.08f);

            for (float x = minX; x <= maxX; x += GRID_SIZE)
            {
                Vector2 start = GraphToScreen(new Vector2(x, minY));
                Vector2 end = GraphToScreen(new Vector2(x, maxY));
                Handles.DrawLine(start, end);
            }

            for (float y = minY; y <= maxY; y += GRID_SIZE)
            {
                Vector2 start = GraphToScreen(new Vector2(minX, y));
                Vector2 end = GraphToScreen(new Vector2(maxX, y));
                Handles.DrawLine(start, end);
            }

            Handles.EndGUI();
        }

        private void DrawNodes()
        {
            if (Event.current.type != EventType.Repaint)
                return;

            GUIStyle labelStyle = CreateNodeLabelStyle();

            for (int i = 0; i < _talentsProp.arraySize; i++)
            {
                if (!TryGetTalentProperty(i, ID_FIELD, out SerializedProperty idProp) ||
                    !TryGetTalentProperty(i, GRAPH_POSITION_FIELD, out SerializedProperty posProp))
                {
                    continue;
                }

                Vector2 screenPos = GraphToScreen(posProp.vector2Value);

                bool isSelected = i == _selectedIndex;
                DrawNodeDisc(screenPos, isSelected);
            }

            for (int i = 0; i < _talentsProp.arraySize; i++)
            {
                if (!TryGetTalentProperty(i, ID_FIELD, out SerializedProperty idProp) ||
                    !TryGetTalentProperty(i, GRAPH_POSITION_FIELD, out SerializedProperty posProp))
                {
                    continue;
                }

                Vector2 screenPos = GraphToScreen(posProp.vector2Value);
                bool isSelected = i == _selectedIndex;
                string label = DisplayNameForTalent(i, idProp);
                Rect labelRect = GetNodeLabelRect(screenPos, label, labelStyle);
                DrawNodeLabel(labelRect, label, labelStyle, isSelected);
            }
        }

        private void DrawNodeDisc(Vector2 screenPos, bool isSelected)
        {
            Handles.BeginGUI();
            if (isSelected)
            {
                Handles.color = new Color(0.35f, 0.72f, 1f, 0.28f);
                Handles.DrawSolidDisc(screenPos, Vector3.forward, NODE_RADIUS + 7f);
            }

            Handles.color = isSelected
                ? new Color(0.18f, 0.6f, 1f, 1f)
                : new Color(0.66f, 0.66f, 0.78f, 1f);
            Handles.DrawSolidDisc(screenPos, Vector3.forward, NODE_RADIUS);
            Handles.color = isSelected
                ? new Color(0.78f, 0.9f, 1f, 1f)
                : new Color(0.18f, 0.18f, 0.24f, 1f);
            Handles.DrawWireDisc(screenPos, Vector3.forward, NODE_RADIUS + 0.5f);
            Handles.EndGUI();
        }

        private GUIStyle CreateNodeLabelStyle()
        {
            GUIStyle style = new(EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                clipping = TextClipping.Clip,
                fontSize = 10,
                wordWrap = true
            };
            style.normal.textColor = new Color(0.92f, 0.93f, 0.98f, 1f);
            return style;
        }

        private string DisplayNameForTalent(int index, SerializedProperty idProp)
        {
            if (TryGetTalentAsset(index, out UnityEngine.Object talent) && !string.IsNullOrWhiteSpace(talent.name))
                return ObjectNames.NicifyVariableName(talent.name);

            return ObjectNames.NicifyVariableName(((TalentId)idProp.intValue).ToString());
        }

        private Rect GetNodeLabelRect(Vector2 screenPos, string label, GUIStyle labelStyle)
        {
            GUIContent content = new(label);
            float height = Mathf.Clamp(
                labelStyle.CalcHeight(content, LABEL_WIDTH - LABEL_PADDING * 2f) + LABEL_PADDING * 2f,
                LABEL_MIN_HEIGHT,
                LABEL_MAX_HEIGHT);
            float belowY = screenPos.y + NODE_RADIUS + LABEL_GAP;
            float aboveY = screenPos.y - NODE_RADIUS - LABEL_GAP - height;
            float y = belowY + height <= _previewRect.yMax - LABEL_PADDING ? belowY : aboveY;

            Rect rect = new(screenPos.x - LABEL_WIDTH * 0.5f, y, LABEL_WIDTH, height);
            rect.x = Mathf.Clamp(rect.x, _previewRect.x + LABEL_PADDING, _previewRect.xMax - rect.width - LABEL_PADDING);
            rect.y = Mathf.Clamp(rect.y, _previewRect.y + LABEL_PADDING, _previewRect.yMax - rect.height - LABEL_PADDING);
            return rect;
        }

        private void DrawNodeLabel(Rect rect, string label, GUIStyle labelStyle, bool isSelected)
        {
            Color backgroundColor = isSelected
                ? new Color(0.03f, 0.15f, 0.26f, 0.96f)
                : new Color(0.02f, 0.02f, 0.05f, 0.9f);
            Color borderColor = isSelected
                ? new Color(0.48f, 0.78f, 1f, 1f)
                : new Color(0.32f, 0.32f, 0.42f, 1f);

            EditorGUI.DrawRect(rect, backgroundColor);
            DrawRectBorder(rect, borderColor);

            Rect textRect = new(
                rect.x + LABEL_PADDING,
                rect.y + LABEL_PADDING * 0.5f,
                rect.width - LABEL_PADDING * 2f,
                rect.height - LABEL_PADDING);
            GUI.Label(textRect, label, labelStyle);
        }

        private void DrawRectBorder(Rect rect, Color color)
        {
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1f), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, 1f, rect.height), color);
            EditorGUI.DrawRect(new Rect(rect.xMax - 1f, rect.y, 1f, rect.height), color);
        }

        private void ClampViewOffset()
        {
            float maxPanX = _previewRect.width * 0.5f;
            float maxPanY = _previewRect.height * 0.5f;
            _viewOffset.x = Mathf.Clamp(_viewOffset.x, -maxPanX, maxPanX);
            _viewOffset.y = Mathf.Clamp(_viewOffset.y, -maxPanY, maxPanY);
        }

        private bool TryGetSelectedTalentId(out int selectedId)
        {
            selectedId = 0;

            if (_selectedIndex < 0)
                return false;

            if (!TryGetTalentProperty(_selectedIndex, ID_FIELD, out SerializedProperty idProp))
                return false;

            selectedId = idProp.intValue;
            return true;
        }

        private void HandleEvents()
        {
            Event e = Event.current;
            Vector2 mousePos = e.mousePosition;

            if (_isResizingPreview)
            {
                HandlePreviewResize(e);
                return;
            }

            if (_resizeHandleRect.Contains(mousePos))
            {
                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    _isResizingPreview = true;
                    e.Use();
                    Repaint();
                }

                return;
            }

            if (!_previewRect.Contains(mousePos))
            {
                if (_isDragging)
                {
                    _isDragging = false;
                    _didDragNode = false;
                    _dragIndex = -1;
                }
                if (_isPanning)
                {
                    _isPanning = false;
                }
                return;
            }

            switch (e.type)
            {
                case EventType.MouseDown when e.button == 0:
                {
                    _dragIndex = -1;

                    for (int i = _talentsProp.arraySize - 1; i >= 0; i--)
                    {
                        if (!TryGetTalentProperty(i, GRAPH_POSITION_FIELD, out SerializedProperty posProp))
                            continue;

                        Vector2 nodeScreenPos = GraphToScreen(posProp.vector2Value);

                        if (Vector2.Distance(mousePos, nodeScreenPos) <= NODE_RADIUS + 6f)
                        {
                            _selectedIndex = i;
                            _dragIndex = i;
                            _grabOffset = mousePos - nodeScreenPos;
                            _isDragging = true;
                            _didDragNode = false;
                            e.Use();
                            Repaint();
                            break;
                        }
                    }

                    if (_dragIndex == -1)
                    {
                        _isPanning = true;
                        e.Use();
                        Repaint();
                    }
                    break;
                }

                case EventType.MouseDrag when _isDragging && _dragIndex >= 0:
                {
                    if (!TryGetTalentObject(_dragIndex, out SerializedObject talentObject))
                        break;

                    SerializedProperty dragPosProp = talentObject.FindProperty(GRAPH_POSITION_FIELD);
                    Vector2 snappedPosition = SnapToGrid(ScreenToGraph(mousePos - _grabOffset));

                    if (dragPosProp.vector2Value == snappedPosition)
                    {
                        e.Use();
                        Repaint();
                        break;
                    }

                    if (!_didDragNode)
                    {
                        Undo.RecordObject(talentObject.targetObject, "Move talent node");
                        _didDragNode = true;
                    }

                    dragPosProp.vector2Value = snappedPosition;
                    talentObject.ApplyModifiedProperties();
                    e.Use();
                    Repaint();
                    break;
                }

                case EventType.MouseDrag when _isPanning:
                {
                    _viewOffset += e.delta;
                    ClampViewOffset();
                    e.Use();
                    Repaint();
                    break;
                }

                case EventType.MouseUp when _isDragging:
                {
                    int clickedIndex = _dragIndex;
                    bool shouldSelectAsset = !_didDragNode;
                    _isDragging = false;
                    _dragIndex = -1;
                    _didDragNode = false;

                    if (shouldSelectAsset)
                        SelectTalentAsset(clickedIndex);

                    e.Use();
                    Repaint();
                    break;
                }

                case EventType.MouseUp when _isPanning:
                    _isPanning = false;
                    e.Use();
                    Repaint();
                    break;

                case EventType.ScrollWheel:
                {
                    float zoomDelta = -e.delta.y * 0.002f;
                    _panFactor = Mathf.Clamp(_panFactor * (1f + zoomDelta), 0.2f, 8f);
                    e.Use();
                    Repaint();
                    break;
                }
            }
        }

        private void HandlePreviewResize(Event e)
        {
            switch (e.type)
            {
                case EventType.MouseDrag:
                    SetPreviewHeight(_previewHeight + e.delta.y);
                    e.Use();
                    Repaint();
                    break;

                case EventType.MouseUp:
                    _isResizingPreview = false;
                    EditorPrefs.SetFloat(PREVIEW_HEIGHT_PREF_KEY, _previewHeight);
                    e.Use();
                    Repaint();
                    break;
            }
        }

        private void SetPreviewHeight(float height)
        {
            float clampedHeight = Mathf.Clamp(height, MIN_PREVIEW_HEIGHT, MAX_PREVIEW_HEIGHT);

            if (Mathf.Approximately(_previewHeight, clampedHeight))
                return;

            _previewHeight = clampedHeight;
            EditorPrefs.SetFloat(PREVIEW_HEIGHT_PREF_KEY, _previewHeight);
        }

        private static Vector2 SnapToGrid(Vector2 position)
        {
            return new Vector2(
                Mathf.Round(position.x / GRID_SIZE) * GRID_SIZE,
                Mathf.Round(position.y / GRID_SIZE) * GRID_SIZE);
        }

        private void SnapAllNodesToGrid()
        {
            for (int i = 0; i < _talentsProp.arraySize; i++)
            {
                if (!TryGetTalentObject(i, out SerializedObject talentObject))
                    continue;

                Undo.RecordObject(talentObject.targetObject, "Snap talent nodes to grid");
                SerializedProperty posProp = talentObject.FindProperty(GRAPH_POSITION_FIELD);
                posProp.vector2Value = SnapToGrid(posProp.vector2Value);
                talentObject.ApplyModifiedProperties();
            }

            Repaint();
        }

        private void SelectTalentAsset(int index)
        {
            if (!TryGetTalentAsset(index, out UnityEngine.Object talent))
                return;

            Selection.activeObject = talent;
            EditorGUIUtility.PingObject(talent);
        }

        private bool TryGetTalentAsset(int index, out UnityEngine.Object talent)
        {
            talent = null;

            if (_talentsProp == null || index < 0 || index >= _talentsProp.arraySize)
                return false;

            talent = _talentsProp.GetArrayElementAtIndex(index).objectReferenceValue;
            return talent != null;
        }

        private bool TryGetTalentObject(int index, out SerializedObject talentObject)
        {
            talentObject = null;

            if (!TryGetTalentAsset(index, out UnityEngine.Object talent))
                return false;

            talentObject = new SerializedObject(talent);
            talentObject.Update();
            return true;
        }

        private bool TryGetTalentProperty(int index, string propertyName, out SerializedProperty property)
        {
            property = null;

            if (!TryGetTalentObject(index, out SerializedObject talentObject))
                return false;

            property = talentObject.FindProperty(propertyName);
            return property != null;
        }
    }
}
