using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules
{
    [CreateAssetMenu(fileName = "BattleMoleculeConfig", menuName = "Game Resources/Configs/Battle Molecule")]
    public class BattleMoleculeConfig : ScriptableObject
    {
        [field: Header("Standard Molecule")]
        [field: SerializeField] public Vector2 SpawnOffset { get; private set; } = new(2f, 0f);
        [field: SerializeField, Min(1)] public int AtomsRequired { get; private set; } = 3;
        [field: SerializeField, Min(1)] public int BaseShotDamage { get; private set; } = 1;
        [field: SerializeField] public float AtomsPosCircleRadius { get; private set; } = 0.6f;
        [field: SerializeField, Min(0f)] public float DepositedAtomsOrbitDegreesPerSecond { get; private set; } = 90f;
        [field: SerializeField, Min(0f)] public float CoreOrbitDegreesPerSecond { get; private set; } = 45f;

        [field: Header("Mass Molecule")]
        [field: SerializeField] public Vector2 MassMoleculeSpawnOffset { get; private set; } = new(-2f, 0f);
        [field: SerializeField, Min(1)] public int MassMoleculeAtomsRequired { get; private set; } = 5;
        [field: SerializeField, Min(1)] public int MassMoleculeShotCount { get; private set; } = 5;
        [field: SerializeField, Min(0f)] public float MassMoleculeShotSpreadDegrees { get; private set; } = 30f;
    }
}
