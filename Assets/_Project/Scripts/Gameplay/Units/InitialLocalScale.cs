using UnityEngine;

namespace _Project.Scripts.Gameplay.Units
{
    [DisallowMultipleComponent]
    public class InitialLocalScale : MonoBehaviour
    {
        private Vector3 _initialScale;
        private bool _isCaptured;

        internal Vector3 Scale
        {
            get
            {
                Capture();
                return _initialScale;
            }
        }

        private void Awake()
        {
            Capture();
        }

        internal void ResetScale()
        {
            transform.localScale = Scale;
        }

        private void Capture()
        {
            if (_isCaptured)
                return;

            _initialScale = transform.localScale;
            _isCaptured = true;
        }
    }
}
