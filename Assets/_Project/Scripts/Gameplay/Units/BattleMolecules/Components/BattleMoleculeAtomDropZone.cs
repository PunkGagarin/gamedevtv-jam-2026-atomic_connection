using _Project.Scripts.Gameplay.Drag;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    [RequireComponent(typeof(Collider2D))]
    public class BattleMoleculeAtomDropZone : MonoBehaviour, IDropTarget
    {
        [field: SerializeField] private BattleMoleculeAtomReceiver Receiver { get; set; }

        private void Awake()
        {
            if (Receiver == null)
                Receiver = GetComponentInParent<BattleMoleculeAtomReceiver>();
        }

        public bool CanAcceptDrop(IDraggable draggable)
        {
            return Receiver != null && Receiver.CanAcceptDrop(draggable);
        }

        public void OnDropAccepted(IDraggable draggable)
        {
            Receiver?.AcceptDrop(draggable);
        }

        public void OnDropRejected(IDraggable draggable)
        {
        }
    }
}
