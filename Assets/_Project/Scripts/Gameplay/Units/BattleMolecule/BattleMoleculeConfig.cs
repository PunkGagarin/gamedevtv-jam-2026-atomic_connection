using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecule
{
    [CreateAssetMenu(fileName = "BattleMoleculeConfig", menuName = "Game Resources/Configs/Battle Molecule")]
    public class BattleMoleculeConfig : ScriptableObject
    {
        [field: SerializeField] public Vector2 SpawnOffset { get; private set; } = new(2f, 0f);
        [field: SerializeField, Min(1)] public int AtomsRequired { get; private set; } = 3;
        [field: SerializeField] public float AtomsPosCircleRadius { get; private set; } = 0.6f;
    }
}