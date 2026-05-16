using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecule
{
    public interface IBattleMoleculeFactory
    {
        BattleMolecule Create(Vector3 at);
        void Cleanup();
    }
}
