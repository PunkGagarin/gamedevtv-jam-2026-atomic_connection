using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules
{
    [CreateAssetMenu(fileName = "BattleMoleculeConfig", menuName = "Game Resources/Configs/Battle Molecule")]
    public class BattleMoleculeConfig : ScriptableObject
    {
        [field: Header("Stinger Molecule")]
        [field: SerializeField] public Vector2 StingerMoleculeSpawnOffset { get; private set; } = new(2f, 0f);
        [field: SerializeField, Min(1)] public int StingerMoleculeBondAtomsRequired { get; private set; } = 1;
        [field: SerializeField, Min(1)] public int StingerMoleculeAtomsRequired { get; private set; } = 5;
        [field: SerializeField, Min(1)] public int StingerMoleculeShotDamage { get; private set; } = 3;
        [field: SerializeField, Min(1f)] public float StingerMoleculeCriticalDamageMultiplier { get; private set; } = 2f;

        [field: Header("Shared Orbit")]
        [field: SerializeField, Min(0f)] public float AtomsPosCircleRadius { get; private set; } = 0.6f;
        [field: SerializeField, Min(0f)] public float DepositedAtomsOrbitDegreesPerSecond { get; private set; } = 90f;
        [field: SerializeField, Min(0f)] public float CoreOrbitRadius { get; private set; } = 2f;
        [field: SerializeField, Min(0f)] public float CoreOrbitDegreesPerSecond { get; private set; } = 20f;

        [field: Header("Connection")]
        [field: SerializeField, Min(0.01f)] public float ConnectionAtomTravelSpeed { get; private set; } = 1f;
        [field: SerializeField, Min(0f)] public float ConnectionCoreRimDegreesPerSecond { get; private set; } = 60f;
        [field: SerializeField, Min(0f)] public float ConnectionAtomArrivalDistance { get; private set; } = 0.03f;
        [field: SerializeField, Min(0f)] public float ConnectionAtomMinimumFlowRadius { get; private set; } = 0.1f;
        [field: SerializeField, Min(0f)] public float ConnectionCoreRimSnapDistance { get; private set; } = 0.06f;
        [field: SerializeField, Min(0f)] public float ConnectionCoreRimArrivalAngleDegrees { get; private set; } = 0.5f;
        [field: SerializeField, Min(0f)] public float ConnectionLineWidth { get; private set; } = 0.08f;
        [field: SerializeField] public float ConnectionLineZOffset { get; private set; } = 0.05f;
        [field: SerializeField] public int ConnectionLineSortingOrder { get; private set; } = -1;
        [field: SerializeField] public Color ConnectionInactiveColor { get; private set; } = new(0.357f, 0.925f, 1f, 0.45f);
        [field: SerializeField] public Color ConnectionActiveColor { get; private set; } = new(0.357f, 0.925f, 1f, 0.95f);

        [field: Header("Membrane Molecule")]
        [field: SerializeField] public Vector2 MembraneMoleculeSpawnOffset { get; private set; } = new(0f, -2f);
        [field: SerializeField, Min(1)] public int MembraneMoleculeBondAtomsRequired { get; private set; } = 1;
        [field: SerializeField, Min(1)] public int MembraneMoleculeAtomsRequired { get; private set; } = 3;
        [field: SerializeField, Min(0.1f)] public float MembraneDurationSeconds { get; private set; } = 5f;
        [field: SerializeField, Min(1)] public int MembraneIntegrity { get; private set; } = 3;
        [field: SerializeField, Min(0f)] public float MembraneCooldownSeconds { get; private set; } = 6f;
        [field: SerializeField, Min(0f)] public float MembraneKnockbackRadius { get; private set; } = 2.5f;
        [field: SerializeField, Min(0f)] public float MembraneKnockbackDistance { get; private set; } = 0.8f;
        [field: SerializeField, Min(0.01f)] public float MembraneKnockbackDurationSeconds { get; private set; } = 0.18f;

        [field: Header("Swarm Molecule")]
        [field: SerializeField] public Vector2 SwarmMoleculeSpawnOffset { get; private set; } = new(-2f, 0f);
        [field: SerializeField, Min(1)] public int SwarmMoleculeBondAtomsRequired { get; private set; } = 1;
        [field: SerializeField, Min(1)] public int SwarmMoleculeAtomsRequired { get; private set; } = 7;
        [field: SerializeField, Min(1)] public int SwarmMoleculeShotDamage { get; private set; } = 1;
        [field: SerializeField, Min(1)] public int SwarmMoleculeShotCount { get; private set; } = 5;
        [field: SerializeField, Min(0f)] public float SwarmMoleculeShotSpreadDegrees { get; private set; } = 30f;
        [field: SerializeField, Min(0f)] public float SwarmMoleculeShotSpreadDegreesPerShotCountBonus { get; private set; } = 10f;
        [field: SerializeField, Min(0f)] public float SwarmMoleculeAttackRange { get; private set; } = 8f;
    }
}
