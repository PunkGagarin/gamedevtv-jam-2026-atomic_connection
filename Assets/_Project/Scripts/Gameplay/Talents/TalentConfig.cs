using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Talents
{
    [CreateAssetMenu(fileName = "TalentConfig", menuName = "Game Resources/Configs/Talents")]
    public class TalentConfig : ScriptableObject
    {
        [field: SerializeField] public bool ClearSavedProgressOnStartup { get; private set; }
        [field: SerializeField, Min(0)] public int TestStartingGold { get; private set; } = 500;
        [field: SerializeField] public List<TalentDefinition> Talents { get; private set; } = new();
    }
}
