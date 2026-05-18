namespace _Project.Scripts.Infrastructure.SaveLoad
{
    public interface IProgressProvider
    {
        ProgressData ProgressData { get; }
        void SetProgressData(ProgressData data);
    }
}
