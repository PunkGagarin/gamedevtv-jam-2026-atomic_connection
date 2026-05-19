using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules
{
    public interface IBattleMoleculeFactory
    {
        IReadOnlyList<BattleMolecule> CreatedMolecules { get; }
        event Action<BattleMolecule> MoleculeCreated;
        BattleMolecule Create(Vector3 at, BattleMoleculeConfig config);
        BattleMolecule CreateShield(Vector3 at, BattleMoleculeConfig config);
        BattleMolecule CreateMass(Vector3 at, BattleMoleculeConfig config);
        void Cleanup();
    }
}
