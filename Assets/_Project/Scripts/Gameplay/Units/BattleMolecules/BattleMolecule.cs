using System.Collections.Generic;
using _Project.Scripts.Gameplay.Drag;
using _Project.Scripts.Gameplay.Units.FreeAtoms;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules
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

            return draggable is FreeAtom && _depositedCount < _config.AtomsRequired;
        }

        public void OnDropAccepted(IDraggable draggable)
        {
            if (draggable is not FreeAtom freeAtom)
                return;

            GameObject freeAtomObject = freeAtom.gameObject;

            Collider2D col = freeAtomObject.GetComponent<Collider2D>();
            if (col != null)
                col.enabled = false;

            freeAtomObject.transform.SetParent(transform, true);
            freeAtomObject.transform.localPosition = GetCirclePosition(_depositedCount, _config.AtomsRequired);

            _depositedAtoms.Add(freeAtomObject);
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
