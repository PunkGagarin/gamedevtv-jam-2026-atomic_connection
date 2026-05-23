using UnityEngine;

using _Project.Scripts.Gameplay.Common.Rendering;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(LineRenderer))]
    public class BattleMoleculeConnectionLineRendererVisual : BattleMoleculeConnectionLineVisual
    {
        [field: SerializeField] private LineRenderer Line { get; set; }

        internal override bool IsVisible => Line != null && Line.enabled;

        private void Awake()
        {
            Line = LineRendererUtility.Ensure(gameObject, Line);
            SetVisible(false);
        }

        internal override void Configure(float width, int sortingOrder, Color color)
        {
            if (Line == null)
                return;

            Line.positionCount = 2;
            LineRendererUtility.Configure(Line, width, sortingOrder, color, Line.enabled);
        }

        internal override void SetEndpoints(Vector3 from, Vector3 to)
        {
            if (Line == null)
                return;

            Line.SetPosition(0, from);
            Line.SetPosition(1, to);
        }

        internal override void SetColor(Color color)
        {
            LineRendererUtility.SetColor(Line, color);
        }

        internal override void SetVisible(bool isVisible)
        {
            if (Line != null)
                Line.enabled = isVisible;
        }
    }
}
