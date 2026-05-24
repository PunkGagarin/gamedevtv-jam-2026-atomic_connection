using System;
using _Project.Scripts.Gameplay.Units.FreeAtoms;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.AtomCores
{
    public interface IAtomCoreService
    {
        Transform CurrentCoreTransform { get; }
        event Action CoreDied;
        event Action<FreeAtom> AtomGenerated;
        void Start();
        void Update();
        void Cleanup();
    }
}
