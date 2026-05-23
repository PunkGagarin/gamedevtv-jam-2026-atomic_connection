using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules
{
    public interface IBattleMoleculeFactory
    {
        BattleMolecule CreateStinger(Vector3 at, BattleMoleculeConfig config);
        BattleMolecule CreateMembrane(Vector3 at, BattleMoleculeConfig config);
        BattleMolecule CreateSwarm(Vector3 at, BattleMoleculeConfig config);
    }
}
