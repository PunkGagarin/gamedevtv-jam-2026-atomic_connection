using System;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.AtomCores
{
    public interface IAtomCoreService
    {
        Transform CurrentCoreTransform { get; }
        event Action CoreDied;
        void Start();
        void Update();
        void Cleanup();
    }
}
