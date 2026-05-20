namespace _Project.Scripts.Gameplay.Enemies.Components
{
    public class BossCoreCollision : EnemyCoreCollision
    {
        protected override void ApplyCoreCollision()
        {
            Core.TakeDamage(int.MaxValue);
        }
    }
}
