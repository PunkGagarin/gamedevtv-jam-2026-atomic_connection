namespace _Project.Scripts.Gameplay.Units.BattleMolecules
{
    public interface IBattleMoleculeAutoLoadRule
    {
        bool CanAutoLoad(BattleMoleculeRuntimeContext context);
    }
}
