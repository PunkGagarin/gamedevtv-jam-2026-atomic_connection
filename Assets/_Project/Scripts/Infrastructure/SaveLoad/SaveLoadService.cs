using UnityEngine;
using Zenject;

namespace _Project.Scripts.Infrastructure.SaveLoad
{
    public class SaveLoadService : ISaveLoadService
    {
        private const string PROGRESS_KEY = "AtomicConnection.Progress";

        private readonly IProgressProvider _progressProvider;

        [Inject]
        public SaveLoadService(IProgressProvider progressProvider)
        {
            _progressProvider = progressProvider;
            EnsureProgressLoaded();
        }

        public bool HasSavedProgress => PlayerPrefs.HasKey(PROGRESS_KEY);

        public void SaveProgress()
        {
            string json = _progressProvider.ProgressData.ToJson();
            PlayerPrefs.SetString(PROGRESS_KEY, json);
            PlayerPrefs.Save();
        }

        public void LoadProgress()
        {
            if (!HasSavedProgress)
            {
                CreateProgress();
                return;
            }

            string json = PlayerPrefs.GetString(PROGRESS_KEY);
            ProgressData data = ProgressData.FromJson(json);
            _progressProvider.SetProgressData(data);
        }

        public void CreateProgress()
        {
            _progressProvider.SetProgressData(new ProgressData());
        }

        public void DeleteAllSavedData()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            CreateProgress();
        }

        private void EnsureProgressLoaded()
        {
            if (_progressProvider.ProgressData != null)
                return;

            if (HasSavedProgress)
                LoadProgress();
            else
                CreateProgress();
        }
    }
}
