namespace _Project.Scripts.Gameplay.Units.BattleMolecules
{
    public interface IBattleMoleculeRuntimeBehavior
    {
        void Configure(BattleMoleculeRuntimeContext context);
        void Tick(float deltaTime);
    }
}
