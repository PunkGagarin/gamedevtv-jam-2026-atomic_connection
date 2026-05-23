using _Project.Scripts.Gameplay.Units;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.AtomCores.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AtomProductionProgress))]
    [RequireComponent(typeof(PointHitArea))]
    public class AtomCoreClickInteraction : MonoBehaviour
    {
        [field: SerializeField] private AtomProductionProgress ProductionProgress { get; set; }
        [field: SerializeField] private PointHitArea HitArea { get; set; }

        private void Awake()
        {
            if (ProductionProgress == null)
                ProductionProgress = GetComponent<AtomProductionProgress>();

            if (HitArea == null)
                HitArea = GetComponent<PointHitArea>();
        }

        public void Configure(int clicksRequired)
        {
            ProductionProgress.Configure(clicksRequired);
        }

        public bool Contains(Vector2 worldPosition)
        {
            return HitArea.Contains(worldPosition);
        }

        public bool RegisterClick()
        {
            return ProductionProgress.RegisterClick();
        }
    }
}
