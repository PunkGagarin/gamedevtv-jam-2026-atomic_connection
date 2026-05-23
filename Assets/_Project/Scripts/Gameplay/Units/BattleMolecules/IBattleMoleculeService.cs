namespace _Project.Scripts.Gameplay.Units.BattleMolecules
{
    public interface IBattleMoleculeService
    {
        void Register(BattleMolecule molecule);
        void Start();
        void Update();
        void Cleanup();
    }
}
