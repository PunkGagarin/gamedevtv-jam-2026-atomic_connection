using _Project.Scripts.Gameplay.Common.Time;
using Zenject;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules
{
    public class BattleMoleculeService : IBattleMoleculeService
    {
        [Inject] private IBattleMoleculeFactory _battleMoleculeFactory;
        [Inject] private ITimeService _time;

        public void Update()
        {
            foreach (BattleMolecule molecule in _battleMoleculeFactory.CreatedMolecules)
            {
                if (molecule != null)
                    molecule.Tick(_time.DeltaTime);
            }
        }

        public void Cleanup()
        {
        }
    }
}
