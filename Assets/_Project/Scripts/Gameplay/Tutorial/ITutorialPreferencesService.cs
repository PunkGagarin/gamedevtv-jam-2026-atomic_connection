namespace _Project.Scripts.Gameplay.Tutorial
{
    public interface ITutorialPreferencesService
    {
        bool GameplayTutorialCompleted { get; }
        void MarkGameplayTutorialCompleted();
        void ClearGameplayTutorialCompleted();
    }
}
