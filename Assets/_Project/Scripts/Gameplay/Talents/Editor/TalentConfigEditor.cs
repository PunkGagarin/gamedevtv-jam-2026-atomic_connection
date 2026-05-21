using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Talents.Editor
{
    [CustomEditor(typeof(TalentConfig))]
    public class TalentConfigEditor : UnityEditor.Editor
    {
        private const float PREVIEW_HEIGHT = 280f;
        private const float NODE_RADIUS = 10f;
        private const float GRID_SIZE = 120f;
        private const string ID_FIELD = "<Id>k__BackingField";
        private const string TITLE_FIELD = "<Title>k__BackingField";
        private const string PREREQUISITES_FIELD = "<Prerequisites>k__BackingField";
        private const string GRAPH_POSITION_FIELD = "<GraphPosition>k__BackingField";

        private SerializedProperty _talentsProp;
        private int _selectedIndex = -1;
        private bool _isDragging = false;
        private int _dragIndex = -1;
        private Vector2 _grabOffset;
        private Vector2 _graphCenter;
        private Vector2 _graphBoundsMin;
        private Vector2 _graphBoundsMax;
        private float _graphScale = 1f;
        private Rect _previewRect;

        private Vector2 _viewOffset;
        private float _panFactor = 1f;
        private bool _isPanning = false;

        private void OnEnable()
        {
            _talentsProp = serializedObject.FindProperty("<Talents>k__BackingField");
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
            EditorGUILayout.LabelField("Graph Preview (drag nodes, snap to grid, scroll to zoom, drag bg to pan)", EditorStyles.boldLabel);

            if (GUILayout.Button("Snap All Nodes To Grid", GUILayout.Width(180f)))
                SnapAllNodesToGrid();

            Rect reserved = EditorGUILayout.BeginVertical();
            GUILayout.Space(PREVIEW_HEIGHT);
            EditorGUILayout.EndVertical();

            _previewRect = new Rect(reserved.x, reserved.y, reserved.width, PREVIEW_HEIGHT);
            _previewRect = EditorGUI.IndentedRect(_previewRect);

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
            HandleEvents();

            HandleUtility.Repaint();
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
                        Vector3 start = new(parentPos.x, parentPos.y, 0f);
                        Vector3 end = new(childPos.x, childPos.y, 0f);
                        Handles.BeginGUI();
                        Handles.color = new Color(0.55f, 0.45f, 0.2f, 0.5f);
                        Handles.DrawLine(start, end);
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

            for (int i = 0; i < _talentsProp.arraySize; i++)
            {
                if (!TryGetTalentProperty(i, ID_FIELD, out SerializedProperty idProp) ||
                    !TryGetTalentProperty(i, GRAPH_POSITION_FIELD, out SerializedProperty posProp) ||
                    !TryGetTalentProperty(i, TITLE_FIELD, out SerializedProperty titleProp))
                {
                    continue;
                }

                Vector2 screenPos = GraphToScreen(posProp.vector2Value);

                bool isSelected = i == _selectedIndex;
                Color fillColor = isSelected
                    ? new Color(0.2f, 0.6f, 1f, 0.9f)
                    : new Color(0.5f, 0.5f, 0.6f, 0.8f);

                Handles.BeginGUI();
                Handles.color = fillColor;
                Handles.DrawSolidDisc(screenPos, Vector3.forward, NODE_RADIUS);

                if (isSelected)
                {
                    Handles.color = new Color(0.4f, 0.8f, 1f, 0.4f);
                    Handles.DrawSolidDisc(screenPos, Vector3.forward, NODE_RADIUS + 3f);
                }
                Handles.EndGUI();

                string label = titleProp.stringValue;
                if (string.IsNullOrEmpty(label))
                    label = ((TalentId)idProp.intValue).ToString();

                GUIStyle labelStyle = new(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 9
                };

                float labelY = screenPos.y - NODE_RADIUS - 10f;
                Rect labelRect = new(screenPos.x - 60f, labelY - 7f, 120f, 14f);
                GUI.Label(labelRect, label, labelStyle);
            }
        }

        private void ClampViewOffset()
        {
            float maxPanX = _previewRect.width * 0.5f;
            float maxPanY = _previewRect.height * 0.5f;
            _viewOffset.x = Mathf.Clamp(_viewOffset.x, -maxPanX, maxPanX);
            _viewOffset.y = Mathf.Clamp(_viewOffset.y, -maxPanY, maxPanY);
        }

        private void HandleEvents()
        {
            Event e = Event.current;
            Vector2 mousePos = e.mousePosition;

            if (!_previewRect.Contains(mousePos))
            {
                if (_isDragging)
                {
                    _isDragging = false;
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
                            if (TryGetTalentObject(i, out SerializedObject talentObject))
                                Undo.RecordObject(talentObject.targetObject, "Move talent node");
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
                    dragPosProp.vector2Value = SnapToGrid(ScreenToGraph(mousePos - _grabOffset));
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
                    _isDragging = false;
                    _dragIndex = -1;
                    e.Use();
                    Repaint();
                    break;

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

        private bool TryGetTalentObject(int index, out SerializedObject talentObject)
        {
            talentObject = null;

            if (_talentsProp == null || index < 0 || index >= _talentsProp.arraySize)
                return false;

            UnityEngine.Object talent = _talentsProp.GetArrayElementAtIndex(index).objectReferenceValue;

            if (talent == null)
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
