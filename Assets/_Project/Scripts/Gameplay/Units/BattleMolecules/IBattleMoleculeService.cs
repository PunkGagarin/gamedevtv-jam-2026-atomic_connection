using System;
using _Project.Scripts.Gameplay.Units.AtomCores;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules
{
    public interface IBattleMoleculeService
    {
        BattleMolecule FirstMolecule { get; }
        BattleMolecule ActiveMolecule { get; }
        event Action<BattleMolecule> MoleculeBonded;
        event Action<BattleMolecule> MoleculeCharged;
        event Action<BattleMoleculeShotRequest> ShotRequested;
        void ConfigureCore(AtomCore core);
        void Register(BattleMolecule molecule);
        void Start();
        void Update();
        void Cleanup();
    }
}
