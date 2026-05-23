using _Project.Scripts.Gameplay.Common.Physics;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.FreeAtoms.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ColliderSet))]
    [RequireComponent(typeof(InitialLocalScale))]
    [RequireComponent(typeof(FreeAtomOwnership))]
    [RequireComponent(typeof(FreeAtomState))]
    public class FreeAtomLifecycle : MonoBehaviour
    {
        [field: SerializeField] private ColliderSet ColliderSet { get; set; }
        [field: SerializeField] private InitialLocalScale InitialLocalScale { get; set; }
        [field: SerializeField] private FreeAtomOwnership Ownership { get; set; }
        [field: SerializeField] private FreeAtomState State { get; set; }
        [field: SerializeField] private HoverBehaviour HoverBehaviour { get; set; }

        private void Awake()
        {
            if (ColliderSet == null)
                ColliderSet = GetComponent<ColliderSet>();

            if (InitialLocalScale == null)
                InitialLocalScale = GetComponent<InitialLocalScale>();

            if (Ownership == null)
                Ownership = GetComponent<FreeAtomOwnership>();

            if (State == null)
                State = GetComponent<FreeAtomState>();

            if (HoverBehaviour == null)
                HoverBehaviour = GetComponent<HoverBehaviour>();
        }

        public void PrepareForSpawn()
        {
            ResetReusableState();
            ColliderSet?.SetEnabled(true);
            gameObject.SetActive(true);
        }

        public void PrepareForPool()
        {
            ResetReusableState();
            ColliderSet?.SetEnabled(false);
            gameObject.SetActive(false);
        }

        private void ResetReusableState()
        {
            State?.ResetState();
            Ownership?.ClearOwner();
            InitialLocalScale?.ResetScale();
            SetHoverEnabled(false);
        }

        private void SetHoverEnabled(bool isEnabled)
        {
            if (HoverBehaviour != null)
                HoverBehaviour.enabled = isEnabled;
        }
    }
}
