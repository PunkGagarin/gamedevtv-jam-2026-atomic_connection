using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.FreeAtoms
{
    public interface IFreeAtomFactory
    {
        FreeAtom Create(Vector3 at, Transform parent);
        void Cleanup();
    }
}
