using UnityEngine;

namespace _Project.Scripts.Gameplay.Tutorial
{
    public class TutorialPreferencesService : ITutorialPreferencesService
    {
        private const string GAMEPLAY_TUTORIAL_COMPLETED_KEY = "AtomicConnection.GameplayTutorialCompleted";

        public bool GameplayTutorialCompleted => PlayerPrefs.GetInt(GAMEPLAY_TUTORIAL_COMPLETED_KEY, 0) == 1;

        public void MarkGameplayTutorialCompleted()
        {
            PlayerPrefs.SetInt(GAMEPLAY_TUTORIAL_COMPLETED_KEY, 1);
            PlayerPrefs.Save();
        }

        public void ClearGameplayTutorialCompleted()
        {
            PlayerPrefs.DeleteKey(GAMEPLAY_TUTORIAL_COMPLETED_KEY);
            PlayerPrefs.Save();
        }
    }
}
