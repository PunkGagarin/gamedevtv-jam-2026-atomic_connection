using UnityEngine;

namespace _Project.Scripts.Gameplay.Units
{
    [CreateAssetMenu(fileName = "UnitClickConfig", menuName = "Game Resources/Configs/Unit Click")]
    public class UnitClickConfig : ScriptableObject
    {
        [field: SerializeField, Min(1)] public int ClicksToGenerateFreeAtom { get; private set; } = 4;
        [field: SerializeField, Min(0f)] public float SpawnRadiusOffset { get; private set; } = 0.6f;
        [field: SerializeField, Min(0f)] public float FreeAtomOrbitDegreesPerSecond { get; private set; } = 60f;
    }
}
