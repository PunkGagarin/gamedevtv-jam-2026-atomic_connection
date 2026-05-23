namespace _Project.Scripts.Gameplay.Enemies
{
    public interface IEnemyKillRewardService
    {
        void RegisterEnemy(EnemyUnit enemy);
        void UnregisterEnemy(EnemyUnit enemy);
        void Cleanup();
    }
}
