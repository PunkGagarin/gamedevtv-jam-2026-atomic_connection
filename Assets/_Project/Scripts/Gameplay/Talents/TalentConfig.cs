using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Talents
{
    [CreateAssetMenu(fileName = "TalentConfig", menuName = "Game Resources/Configs/Talents")]
    public class TalentConfig : ScriptableObject
    {
        [field: SerializeField] public bool ClearSavedProgressOnStartup { get; private set; }
        [field: Header("Tree Layout")]
        [field: SerializeField, Min(0.1f)] public float NodePositionScale { get; private set; } = 1.5f;
        [field: SerializeField] public List<TalentDefinition> Talents { get; private set; } = new();
    }
}
