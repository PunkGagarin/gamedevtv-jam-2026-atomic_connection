using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    [DisallowMultipleComponent]
    public abstract class BattleMoleculeConnectionLineVisual : MonoBehaviour
    {
        internal abstract bool IsVisible { get; }
        internal abstract void Configure(float width, int sortingOrder, Color color);
        internal abstract void SetEndpoints(Vector3 from, Vector3 to);
        internal abstract void SetColor(Color color);
        internal abstract void SetVisible(bool isVisible);
    }
}
