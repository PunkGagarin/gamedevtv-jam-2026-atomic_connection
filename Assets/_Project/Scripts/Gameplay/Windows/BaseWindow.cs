using UnityEngine;

namespace _Project.Scripts.Gameplay.Windows
{
    public class BaseWindow : MonoBehaviour
    {
        private GameObject _modalRoot;

        public WindowId Id { get; protected set; }
        internal GameObject ModalRoot => _modalRoot == null ? gameObject : _modalRoot;

        internal void SetModalRoot(GameObject modalRoot) =>
            _modalRoot = modalRoot;

        public virtual void OnBackdropClicked()
        {
        }

        private void Awake() =>
            OnAwake();

        private void Start()
        {
            Initialize();
            SubscribeUpdates();
        }

        private void OnDestroy() =>
            Cleanup();

        protected virtual void OnAwake()
        {
        }

        protected virtual void Initialize()
        {
        }

        protected virtual void SubscribeUpdates()
        {
        }

        protected virtual void UnsubscribeUpdates()
        {
        }

        protected virtual void Cleanup() =>
            UnsubscribeUpdates();
    }
}
