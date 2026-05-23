using UnityEngine;

namespace _Project.Scripts.Gameplay.Common.Rendering
{
    public static class LineRendererUtility
    {
        private static Material _defaultLineMaterial;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            _defaultLineMaterial = null;
        }

        public static LineRenderer Ensure(GameObject owner, LineRenderer current = null)
        {
            if (current != null)
                return current;

            LineRenderer line = owner.GetComponent<LineRenderer>();
            return line != null ? line : owner.AddComponent<LineRenderer>();
        }

        public static void Configure(LineRenderer line, float width, int sortingOrder, Color color, bool isEnabled)
        {
            if (line == null)
                return;

            line.useWorldSpace = true;
            line.positionCount = Mathf.Max(2, line.positionCount);

            float lineWidth = Mathf.Max(0f, width);
            line.startWidth = lineWidth;
            line.endWidth = lineWidth;
            line.sortingOrder = sortingOrder;

            if (line.sharedMaterial == null)
                line.sharedMaterial = DefaultLineMaterial;

            SetColor(line, color);
            line.enabled = isEnabled;
        }

        public static void SetColor(LineRenderer line, Color color, float alpha = 1f)
        {
            if (line == null)
                return;

            color.a = alpha;
            line.startColor = color;
            line.endColor = color;
        }

        private static Material DefaultLineMaterial
        {
            get
            {
                if (_defaultLineMaterial != null)
                    return _defaultLineMaterial;

                Shader shader = Shader.Find("Sprites/Default");
                if (shader == null)
                    return null;

                _defaultLineMaterial = new Material(shader)
                {
                    hideFlags = HideFlags.HideAndDontSave
                };

                return _defaultLineMaterial;
            }
        }
    }
}
