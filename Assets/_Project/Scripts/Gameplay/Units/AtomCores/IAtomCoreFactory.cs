using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.AtomCores
{
    public interface IAtomCoreFactory
    {
        AtomCore CurrentCore { get; }
        AtomCore Create(Vector3 at);
        void Cleanup();
    }
}
