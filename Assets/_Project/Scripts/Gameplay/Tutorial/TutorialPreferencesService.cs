using UnityEngine;
using Zenject;
using _Project.Scripts.Infrastructure.SaveLoad;

namespace _Project.Scripts.Gameplay.Tutorial
{
    public class TutorialPreferencesService : ITutorialPreferencesService
    {
        private const string GAMEPLAY_TUTORIAL_COMPLETED_KEY = "AtomicConnection.GameplayTutorialCompleted";
        private const string ACTIVE_MOLECULE_TUTORIAL_COMPLETED_KEY = "AtomicConnection.ActiveMoleculeTutorialCompleted";

        [Inject] private IProgressProvider _progressProvider;
        [Inject] private ISaveLoadService _saveLoadService;

        public bool GameplayTutorialCompleted
        {
            get
            {
                ProgressData progressData = _progressProvider.ProgressData;
                if (progressData.GameplayTutorialCompleted)
                    return true;

                if (PlayerPrefs.GetInt(GAMEPLAY_TUTORIAL_COMPLETED_KEY, 0) != 1)
                    return false;

                progressData.GameplayTutorialCompleted = true;
                _saveLoadService.SaveProgress();
                return true;
            }
        }

        public bool ActiveMoleculeTutorialCompleted
        {
            get
            {
                ProgressData progressData = _progressProvider.ProgressData;
                if (progressData.ActiveMoleculeTutorialCompleted)
                    return true;

                if (PlayerPrefs.GetInt(ACTIVE_MOLECULE_TUTORIAL_COMPLETED_KEY, 0) != 1)
                    return false;

                progressData.ActiveMoleculeTutorialCompleted = true;
                _saveLoadService.SaveProgress();
                return true;
            }
        }

        public void MarkGameplayTutorialCompleted()
        {
            _progressProvider.ProgressData.GameplayTutorialCompleted = true;
            _saveLoadService.SaveProgress();
            PlayerPrefs.SetInt(GAMEPLAY_TUTORIAL_COMPLETED_KEY, 1);
            PlayerPrefs.Save();
        }

        public void MarkActiveMoleculeTutorialCompleted()
        {
            _progressProvider.ProgressData.ActiveMoleculeTutorialCompleted = true;
            _saveLoadService.SaveProgress();
            PlayerPrefs.SetInt(ACTIVE_MOLECULE_TUTORIAL_COMPLETED_KEY, 1);
            PlayerPrefs.Save();
        }

        public void ClearGameplayTutorialCompleted()
        {
            _progressProvider.ProgressData.GameplayTutorialCompleted = false;
            _progressProvider.ProgressData.ActiveMoleculeTutorialCompleted = false;
            _saveLoadService.SaveProgress();
            PlayerPrefs.DeleteKey(GAMEPLAY_TUTORIAL_COMPLETED_KEY);
            PlayerPrefs.DeleteKey(ACTIVE_MOLECULE_TUTORIAL_COMPLETED_KEY);
            PlayerPrefs.Save();
        }
    }
}
