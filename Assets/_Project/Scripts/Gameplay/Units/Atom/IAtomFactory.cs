using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.Atom
{
    public interface IAtomFactory
    {
        Atom Create(Vector3 at);
        void Cleanup();
    }
}
