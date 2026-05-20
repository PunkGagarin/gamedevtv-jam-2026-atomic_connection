using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules
{
    [CreateAssetMenu(fileName = "BattleMoleculeConfig", menuName = "Game Resources/Configs/Battle Molecule")]
    public class BattleMoleculeConfig : ScriptableObject
    {
        [field: Header("Stinger Molecule")]
        [field: SerializeField] public Vector2 StingerMoleculeSpawnOffset { get; private set; } = new(2f, 0f);
        [field: SerializeField, Min(1)] public int StingerMoleculeAtomsRequired { get; private set; } = 3;
        [field: SerializeField, Min(1)] public int BaseShotDamage { get; private set; } = 1;
        [field: SerializeField] public float AtomsPosCircleRadius { get; private set; } = 0.6f;
        [field: SerializeField, Min(0f)] public float DepositedAtomsOrbitDegreesPerSecond { get; private set; } = 90f;
        [field: SerializeField, Min(0f)] public float CoreOrbitDegreesPerSecond { get; private set; } = 45f;

        [field: Header("Auto Load")]
        [field: SerializeField, Min(0.01f)] public float AutoLoadIntervalSeconds { get; private set; } = 2f;

        [field: Header("Membrane Molecule")]
        [field: SerializeField] public Vector2 MembraneMoleculeSpawnOffset { get; private set; } = new(0f, -2f);
        [field: SerializeField, Min(1)] public int MembraneMoleculeAtomsRequired { get; private set; } = 3;
        [field: SerializeField, Min(0.1f)] public float MembraneDurationSeconds { get; private set; } = 5f;
        [field: SerializeField, Min(0f)] public float MembraneSecondsLostPerDamage { get; private set; } = 1f;

        [field: Header("Swarm Molecule")]
        [field: SerializeField] public Vector2 SwarmMoleculeSpawnOffset { get; private set; } = new(-2f, 0f);
        [field: SerializeField, Min(1)] public int SwarmMoleculeAtomsRequired { get; private set; } = 5;
        [field: SerializeField, Min(1)] public int SwarmMoleculeShotCount { get; private set; } = 5;
        [field: SerializeField, Min(0f)] public float SwarmMoleculeShotSpreadDegrees { get; private set; } = 30f;
        [field: SerializeField, Min(0f)] public float SwarmMoleculeAttackRange { get; private set; } = 8f;
    }
}
