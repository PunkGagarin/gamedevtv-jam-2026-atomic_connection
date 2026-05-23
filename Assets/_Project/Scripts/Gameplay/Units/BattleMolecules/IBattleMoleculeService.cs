using _Project.Scripts.Gameplay.Units.AtomCores;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules
{
    public interface IBattleMoleculeService
    {
        void ConfigureCore(AtomCore core);
        void Register(BattleMolecule molecule);
        void Start();
        void Update();
        void Cleanup();
    }
}
