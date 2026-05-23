using _Project.Scripts.Gameplay.Units.BattleMolecules;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BattleMoleculeCoreOrbit))]
    [RequireComponent(typeof(BattleMoleculeConnectionVisual))]
    public class BattleMoleculeCoreLink : MonoBehaviour
    {
        [field: SerializeField] private BattleMoleculeCoreOrbit CoreOrbit { get; set; }
        [field: SerializeField] private BattleMoleculeConnectionVisual ConnectionVisual { get; set; }

        private void Awake()
        {
            if (CoreOrbit == null)
                CoreOrbit = GetComponent<BattleMoleculeCoreOrbit>();

            if (ConnectionVisual == null)
                ConnectionVisual = GetComponent<BattleMoleculeConnectionVisual>();
        }

        public void Configure(Transform coreTransform, BattleMoleculeConfig config)
        {
            if (config == null)
                return;

            CoreOrbit?.Configure(coreTransform, config.CoreOrbitDegreesPerSecond);
            ConnectionVisual?.Configure(coreTransform, config);
        }

        public void Tick(float deltaTime)
        {
            CoreOrbit?.Tick(deltaTime);
            ConnectionVisual?.Tick();
        }
    }
}
