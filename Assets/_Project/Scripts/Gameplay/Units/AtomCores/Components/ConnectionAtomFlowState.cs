using _Project.Scripts.Gameplay.Units.BattleMolecules;
using _Project.Scripts.Gameplay.Units.FreeAtoms;

namespace _Project.Scripts.Gameplay.Units.AtomCores.Components
{
    internal sealed class ConnectionAtomFlowState
    {
        internal FreeAtom Atom;
        internal BattleMolecule Source;
        internal BattleMolecule Target;
        internal ConnectionAtomFlowPhase Phase;
        internal float Radius;
        internal bool HasConnectionProgress;
        internal float ConnectionProgress;
        internal bool IsLeavingSource => Source != null
                                         && (Phase == ConnectionAtomFlowPhase.MoveToSourceConnection
                                             || Phase == ConnectionAtomFlowPhase.SourceConnectionToCore);

        internal void ClearConnectionProgress()
        {
            HasConnectionProgress = false;
            ConnectionProgress = 0f;
        }
    }
}
