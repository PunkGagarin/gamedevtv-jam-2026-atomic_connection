using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules
{
    public interface IBattleMoleculeFactory
    {
        IReadOnlyList<BattleMolecule> CreatedMolecules { get; }
        BattleMolecule Create(Vector3 at, BattleMoleculeConfig config);
        void Cleanup();
    }
}
