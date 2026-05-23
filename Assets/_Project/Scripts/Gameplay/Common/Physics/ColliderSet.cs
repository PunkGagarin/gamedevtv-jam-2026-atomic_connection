using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Common.Physics
{
    [DisallowMultipleComponent]
    public class ColliderSet : MonoBehaviour
    {
        private readonly List<Collider2D> _colliders = new();
        private bool _isDirty = true;

        private void Awake()
        {
            Refresh();
        }

        private void OnTransformChildrenChanged()
        {
            _isDirty = true;
        }

        public void SetEnabled(bool isEnabled)
        {
            RefreshIfDirty();

            foreach (Collider2D col in _colliders)
            {
                if (col != null)
                    col.enabled = isEnabled;
            }
        }

        public void CaptureEnabledStates(List<Collider2D> colliders, List<bool> enabledStates)
        {
            if (colliders == null || enabledStates == null)
                return;

            RefreshIfDirty();
            colliders.Clear();
            enabledStates.Clear();

            foreach (Collider2D col in _colliders)
            {
                if (col == null)
                    continue;

                colliders.Add(col);
                enabledStates.Add(col.enabled);
            }
        }

        public void RestoreEnabledStates(List<Collider2D> colliders, List<bool> enabledStates)
        {
            if (colliders == null || enabledStates == null)
                return;

            int count = Mathf.Min(colliders.Count, enabledStates.Count);

            for (int i = 0; i < count; i++)
            {
                Collider2D col = colliders[i];
                if (col != null)
                    col.enabled = enabledStates[i];
            }

            colliders.Clear();
            enabledStates.Clear();
        }

        private void RefreshIfDirty()
        {
            if (_isDirty)
                Refresh();
        }

        private void Refresh()
        {
            GetComponentsInChildren(true, _colliders);
            _isDirty = false;
        }
    }
}
