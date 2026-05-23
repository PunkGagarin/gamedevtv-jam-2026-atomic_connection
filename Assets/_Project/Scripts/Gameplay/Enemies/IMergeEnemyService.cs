namespace _Project.Scripts.Gameplay.Enemies
{
    public interface IMergeEnemyService
    {
        void Start();
        void RegisterEnemy(EnemyUnit enemy);
        void UnregisterEnemy(EnemyUnit enemy);
        void TickLinks(float deltaTime, float elapsedSeconds);
        void TickDeathWaves(float deltaTime);
        void TickMerge(float deltaTime);
        void Cleanup();
    }
}
