using UnityEngine;

namespace _Project.Scripts.Gameplay.Units
{
    [CreateAssetMenu(fileName = "AtomCoreConfig", menuName = "Game Resources/Configs/Atom Core")]
    public class AtomCoreConfig : ScriptableObject
    {
        [field: Header("Core")]
        [field: SerializeField, Min(1)] public int CoreMaxHealth { get; private set; } = 2;

        [field: Header("Atom Production")]
        [field: SerializeField, Min(1)] public int ClicksToGenerateFreeAtom { get; private set; } = 6;
        [field: SerializeField, Min(0.01f)] public float AutoClickIntervalSeconds { get; private set; } = 0.35f;

        [field: Header("Free Atom Placement")]
        [field: SerializeField, Min(0f)] public float SpawnRadiusOffset { get; private set; } = 0.6f;

        [field: Header("Free Atom Orbit")]
        [field: SerializeField, Min(0f)] public float FreeAtomOrbitDegreesPerSecond { get; private set; } = 60f;
    }
}
