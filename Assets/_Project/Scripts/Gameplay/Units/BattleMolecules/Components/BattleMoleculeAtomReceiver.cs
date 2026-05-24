using _Project.Scripts.Audio.Domain;
using _Project.Scripts.Gameplay.Drag;
using _Project.Scripts.Gameplay.Units.FreeAtoms;
using _Project.Scripts.Gameplay.Units.FreeAtoms.Components;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    [RequireComponent(typeof(BattleMoleculeBond))]
    [RequireComponent(typeof(BattleMoleculeConnectionAtomReceiver))]
    public class BattleMoleculeAtomReceiver : MonoBehaviour
    {
        [field: SerializeField] private BattleMoleculeBond Bond { get; set; }
        [field: SerializeField] private BattleMoleculeConnectionAtomReceiver ConnectionReceiver { get; set; }
        [field: SerializeField] private Sounds DropAcceptedSound { get; set; } = Sounds.singlePop;

        [Inject] private AudioService _audioService;

        private void Awake()
        {
            if (Bond == null)
                Bond = GetComponent<BattleMoleculeBond>();

            if (ConnectionReceiver == null)
                ConnectionReceiver = GetComponent<BattleMoleculeConnectionAtomReceiver>();
        }

        internal bool CanAcceptDrop(IDraggable draggable)
        {
            if (!TryGetFreeAtom(draggable, out _))
                return false;

            return Bond.CanReceiveAtom || ConnectionReceiver.CanReceiveAtom;
        }

        internal void AcceptDrop(IDraggable draggable)
        {
            if (!TryGetFreeAtom(draggable, out FreeAtom freeAtom))
                return;

            if (Bond.CanReceiveAtom)
            {
                if (Bond.TryAcceptAtom(freeAtom))
                    PlayDropAcceptedSound();

                return;
            }

            if (ConnectionReceiver.TryReceive(freeAtom, false))
                PlayDropAcceptedSound();
        }

        private void PlayDropAcceptedSound()
        {
            _audioService?.PlaySfxWithRandomPitch(DropAcceptedSound);
        }

        private static bool TryGetFreeAtom(IDraggable draggable, out FreeAtom freeAtom)
        {
            freeAtom = draggable switch
            {
                FreeAtomDrag drag => drag.Atom,
                _ => null
            };

            return freeAtom != null;
        }
    }
}
