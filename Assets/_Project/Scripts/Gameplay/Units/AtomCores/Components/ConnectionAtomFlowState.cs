using _Project.Scripts.Gameplay.Units.BattleMolecules;
using _Project.Scripts.Gameplay.Units.FreeAtoms;

namespace _Project.Scripts.Gameplay.Units.AtomCores.Components
{
    internal sealed class ConnectionAtomFlowState
    {
        internal FreeAtom Atom;
        internal BattleMolecule Target;
        internal ConnectionAtomFlowPhase Phase;
        internal float Radius;
        internal bool HasConnectionProgress;
        internal float ConnectionProgress;

        internal void ClearConnectionProgress()
        {
            HasConnectionProgress = false;
            ConnectionProgress = 0f;
        }
    }
}
