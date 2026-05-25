using System.Collections.Generic;
using _Project.Scripts.Gameplay.Units.FreeAtoms;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules
{
    public interface IBattleMoleculeConnectionAtomSourceProvider
    {
        void CollectSupplementalConnectionAtoms(BattleMolecule target, List<FreeAtom> results);
    }
}
