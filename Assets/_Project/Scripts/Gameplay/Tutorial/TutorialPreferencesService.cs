using UnityEngine;

namespace _Project.Scripts.Gameplay.Tutorial
{
    public class TutorialPreferencesService : ITutorialPreferencesService
    {
        private const string GAMEPLAY_TUTORIAL_COMPLETED_KEY = "AtomicConnection.GameplayTutorialCompleted";
        private const string ACTIVE_MOLECULE_TUTORIAL_COMPLETED_KEY = "AtomicConnection.ActiveMoleculeTutorialCompleted";

        public bool GameplayTutorialCompleted => PlayerPrefs.GetInt(GAMEPLAY_TUTORIAL_COMPLETED_KEY, 0) == 1;
        public bool ActiveMoleculeTutorialCompleted => PlayerPrefs.GetInt(ACTIVE_MOLECULE_TUTORIAL_COMPLETED_KEY, 0) == 1;

        public void MarkGameplayTutorialCompleted()
        {
            PlayerPrefs.SetInt(GAMEPLAY_TUTORIAL_COMPLETED_KEY, 1);
            PlayerPrefs.Save();
        }

        public void MarkActiveMoleculeTutorialCompleted()
        {
            PlayerPrefs.SetInt(ACTIVE_MOLECULE_TUTORIAL_COMPLETED_KEY, 1);
            PlayerPrefs.Save();
        }

        public void ClearGameplayTutorialCompleted()
        {
            PlayerPrefs.DeleteKey(GAMEPLAY_TUTORIAL_COMPLETED_KEY);
            PlayerPrefs.DeleteKey(ACTIVE_MOLECULE_TUTORIAL_COMPLETED_KEY);
            PlayerPrefs.Save();
        }
    }
}
