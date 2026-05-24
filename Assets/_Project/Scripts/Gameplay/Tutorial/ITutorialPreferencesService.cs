namespace _Project.Scripts.Gameplay.Tutorial
{
    public interface ITutorialPreferencesService
    {
        bool GameplayTutorialCompleted { get; }
        bool ActiveMoleculeTutorialCompleted { get; }
        void MarkGameplayTutorialCompleted();
        void MarkActiveMoleculeTutorialCompleted();
        void ClearGameplayTutorialCompleted();
    }
}
