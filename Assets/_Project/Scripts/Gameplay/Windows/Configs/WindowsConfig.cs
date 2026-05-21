using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Windows.Configs
{
    [CreateAssetMenu(fileName = "windowConfig", menuName = "Game Resources/Window Config")]
    public class WindowsConfig : ScriptableObject
    {
        [field: SerializeField] public Color BackdropColor { get; private set; } = new(0f, 0f, 0f, 0.3f);
        [field: SerializeField] public List<WindowConfig> WindowConfigs { get; private set; } = new();
    }
}
