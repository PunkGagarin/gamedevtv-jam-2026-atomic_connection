using _Project.Scripts.Gameplay.Common.Physics;
using _Project.Scripts.Gameplay.Common.Rendering;
using UnityEngine;

namespace _Project.Scripts.Gameplay.CurrencyDrops
{
    public class CurrencyPickupAreaIndicator : MonoBehaviour
    {
        private const int CORNER_COUNT = 5;

        private readonly Vector3[] _corners = new Vector3[CORNER_COUNT];
        private LineRenderer _line;

        private void Awake()
        {
            EnsureLine();
        }

        public void Configure(float lineWidth, int sortingOrder, Color color)
        {
            EnsureLine();

            LineRendererUtility.Configure(_line, lineWidth, sortingOrder, color, _line.enabled);
        }

        public void Show(Vector3 center, float halfSize)
        {
            SquareArea2D area = new(center, halfSize);
            if (area.IsEmpty)
            {
                Hide();
                return;
            }

            EnsureLine();
            area.WriteClosedCorners(_corners);
            _line.SetPositions(_corners);
            _line.enabled = true;
        }

        public void Hide()
        {
            if (_line != null)
                _line.enabled = false;
        }

        private void EnsureLine()
        {
            if (_line != null)
                return;

            _line = LineRendererUtility.Ensure(gameObject, _line);
            _line.positionCount = CORNER_COUNT;
            LineRendererUtility.Configure(_line, 0f, _line.sortingOrder, Color.white, false);
        }

    }
}
