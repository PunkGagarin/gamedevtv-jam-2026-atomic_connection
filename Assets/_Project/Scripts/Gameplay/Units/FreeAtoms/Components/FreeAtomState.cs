using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.FreeAtoms.Components
{
    [DisallowMultipleComponent]
    public class FreeAtomState : MonoBehaviour
    {
        private bool _isDragging;
        private bool _isInConnectionFlow;

        public bool CanStartDrag => _isInConnectionFlow;
        public bool CanOrbit => !_isDragging && !_isInConnectionFlow;
        public bool CanArrangeInOrbit => !_isInConnectionFlow;
        public bool IsInConnectionFlow => _isInConnectionFlow;

        public void ResetState()
        {
            _isDragging = false;
            _isInConnectionFlow = false;
        }

        public void BeginDrag()
        {
            _isDragging = true;
            _isInConnectionFlow = false;
        }

        public void EndDrag()
        {
            _isDragging = false;
        }

        public void BeginConnectionFlow()
        {
            _isInConnectionFlow = true;
        }

        public void EndConnectionFlow()
        {
            _isInConnectionFlow = false;
        }
    }
}
