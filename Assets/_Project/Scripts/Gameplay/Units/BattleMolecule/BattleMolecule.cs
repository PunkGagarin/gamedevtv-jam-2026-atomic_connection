using System.Collections.Generic;
using _Project.Scripts.Gameplay.Drag;
using UnityEngine;
using Zenject;
using AtomUnit = _Project.Scripts.Gameplay.Units.Atom.Atom;

namespace _Project.Scripts.Gameplay.Units.BattleMolecule
{
    public class BattleMolecule : MonoBehaviour, IDropTarget
    {
        [Inject] private BattleMoleculeConfig _config;

        private readonly List<GameObject> _depositedAtoms = new();
        private int _depositedCount;

        public bool CanAcceptDrop(IDraggable draggable)
        {
            if (_config == null)
                return false;

            return draggable is AtomUnit && _depositedCount < _config.AtomsRequired;
        }

        public void OnDropAccepted(IDraggable draggable)
        {
            if (draggable is not AtomUnit atom)
                return;

            GameObject atomGO = atom.gameObject;

            Collider2D col = atomGO.GetComponent<Collider2D>();
            if (col != null)
                col.enabled = false;

            atomGO.transform.SetParent(transform, true);
            atomGO.transform.localPosition = GetCirclePosition(_depositedCount, _config.AtomsRequired);

            _depositedAtoms.Add(atomGO);
            _depositedCount++;

            if (_depositedCount >= _config.AtomsRequired)
                Fire();
        }

        private void Fire()
        {
            Debug.Log("Boom");

            foreach (GameObject atom in _depositedAtoms)
            {
                if (atom != null)
                    Destroy(atom);
            }

            _depositedAtoms.Clear();
            _depositedCount = 0;
        }

        private Vector3 GetCirclePosition(int index, int total)
        {
            float angle = (index / (float)total) * Mathf.PI * 2f;
            return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * _config.AtomsPosCircleRadius;
        }

        public void OnDropRejected(IDraggable draggable)
        {
        }
    }
}
