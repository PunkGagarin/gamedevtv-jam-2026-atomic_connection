using System;
using _Project.Scripts.Gameplay.Units.AtomCores;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules
{
    public interface IBattleMoleculeService
    {
        BattleMolecule FirstMolecule { get; }
        BattleMolecule ActiveMolecule { get; }
        BattleMolecule FirstAdditionalMolecule { get; }
        event Action<BattleMolecule> MoleculeRegistered;
        event Action<BattleMolecule> MoleculeBonded;
        event Action<BattleMolecule> MoleculeCharged;
        event Action<BattleMolecule> ActiveMoleculeChanged;
        event Action<BattleMoleculeShotRequest> ShotRequested;
        void ConfigureCore(AtomCore core);
        void Register(BattleMolecule molecule);
        void Start();
        void Update();
        void Cleanup();
    }
}
