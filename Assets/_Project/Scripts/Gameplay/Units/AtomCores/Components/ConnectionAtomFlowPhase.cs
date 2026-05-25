namespace _Project.Scripts.Gameplay.Units.AtomCores.Components
{
    internal enum ConnectionAtomFlowPhase
    {
        None,
        MoveToSourceConnection,
        SourceConnectionToCore,
        MoveToRim,
        OrbitToConnection,
        Connection,
        ReturnToCore
    }
}
