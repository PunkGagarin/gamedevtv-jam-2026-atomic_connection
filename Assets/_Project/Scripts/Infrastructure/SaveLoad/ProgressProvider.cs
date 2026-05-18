namespace _Project.Scripts.Infrastructure.SaveLoad
{
    public class ProgressProvider : IProgressProvider
    {
        public ProgressData ProgressData { get; private set; }

        public void SetProgressData(ProgressData data)
        {
            ProgressData = data;
        }
    }
}
