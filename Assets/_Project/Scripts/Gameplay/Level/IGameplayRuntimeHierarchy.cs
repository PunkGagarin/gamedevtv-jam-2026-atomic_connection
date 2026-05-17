using UnityEngine;

namespace _Project.Scripts.Gameplay.Level
{
    public interface IGameplayRuntimeHierarchy
    {
        Transform GetOrCreateContainer(string containerName);
        void Cleanup();
    }
}
