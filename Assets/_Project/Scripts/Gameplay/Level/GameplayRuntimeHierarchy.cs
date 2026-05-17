using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Gameplay.Level
{
    public class GameplayRuntimeHierarchy : IGameplayRuntimeHierarchy
    {
        private const string ROOT_NAME = "GameplayRuntime";

        private readonly Dictionary<string, Transform> _containers = new();

        private Transform _root;

        public Transform GetOrCreateContainer(string containerName)
        {
            EnsureRoot();

            if (_containers.TryGetValue(containerName, out Transform container) && container != null)
                return container;

            GameObject containerObject = new(containerName);
            containerObject.transform.SetParent(_root, false);
            _containers[containerName] = containerObject.transform;

            return containerObject.transform;
        }

        public void Cleanup()
        {
            if (_root != null)
                Object.Destroy(_root.gameObject);

            _root = null;
            _containers.Clear();
        }

        private void EnsureRoot()
        {
            if (_root != null)
                return;

            GameObject rootObject = new(ROOT_NAME);
            SceneManager.MoveGameObjectToScene(rootObject, SceneManager.GetActiveScene());
            _root = rootObject.transform;
        }
    }
}
