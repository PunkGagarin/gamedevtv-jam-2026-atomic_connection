namespace _Project.Scripts.Infrastructure.SaveLoad
{
    public interface ISaveLoadService
    {
        bool HasSavedProgress { get; }
        void SaveProgress();
        void LoadProgress();
        void CreateProgress();
        void DeleteAllSavedData();
    }
}
