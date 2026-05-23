using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.AtomCores
{
    public interface IAtomCoreCreator
    {
        AtomCore Create(Vector3 at);
    }
}
