using System;

namespace _Project.Scripts.Gameplay.Units.AtomCores
{
    public interface IAtomCoreService
    {
        event Action CoreDied;
        void Start(AtomCore core);
        void Update();
        void Cleanup();
    }
}
