using _Project.Scripts.Gameplay.Drag;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.FreeAtoms
{
    public class FreeAtom : MonoBehaviour, IDraggable
    {
        public Transform Transform => transform;

        public void OnDragStart()
        {
        }

        public void OnDragMove(Vector3 worldPosition)
        {
            transform.position = worldPosition;
        }

        public void OnDragEnd()
        {
        }

        public void OnDragCancel()
        {
        }
    }
}
