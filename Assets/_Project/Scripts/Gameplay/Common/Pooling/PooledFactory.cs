using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Common.Pooling
{
    public abstract class PooledFactory<T> where T : MonoBehaviour
    {
        private readonly List<T> _active = new();
        private readonly Stack<T> _pooled = new();

        protected T GetFromPoolOrCreate(Vector3 at, Quaternion rotation, Transform parent)
        {
            T instance = _pooled.Count > 0
                ? _pooled.Pop()
                : CreateNew(at, rotation, parent);

            _active.Add(instance);
            return instance;
        }

        protected void ReturnToPool(T instance)
        {
            if (!_active.Remove(instance))
                throw new InvalidOperationException($"{GetType().Name} tried to return an untracked {typeof(T).Name} to the pool.");

            OnBeforeReturnToPool(instance);
            _pooled.Push(instance);
        }

        public void Cleanup()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
                DestroyInstance(_active[i]);

            _active.Clear();

            while (_pooled.Count > 0)
                DestroyInstance(_pooled.Pop());
        }

        protected abstract T CreateNew(Vector3 at, Quaternion rotation, Transform parent);
        protected abstract void OnBeforeReturnToPool(T instance);

        protected virtual void OnBeforeDestroy(T instance)
        {
        }

        private void DestroyInstance(T instance)
        {
            OnBeforeDestroy(instance);
            UnityEngine.Object.Destroy(instance.gameObject);
        }
    }
}
