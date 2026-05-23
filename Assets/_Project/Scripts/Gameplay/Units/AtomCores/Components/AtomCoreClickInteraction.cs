using _Project.Scripts.Audio.Domain;
using _Project.Scripts.Gameplay.Units;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace _Project.Scripts.Gameplay.Units.AtomCores.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AtomProductionProgress))]
    [RequireComponent(typeof(PointHitArea))]
    public class AtomCoreClickInteraction : MonoBehaviour
    {
        [field: SerializeField]
        private AtomProductionProgress ProductionProgress { get; set; }

        [field: SerializeField]
        private PointHitArea HitArea { get; set; }

        [SerializeField]
        private UnityEvent OnClickEvent;

        [Inject]
        private AudioService _audioService;

        [SerializeField]
        private Sounds _clickSound;

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
            OnClickEvent?.Invoke();
            _audioService.PlaySound(_clickSound.ToString());
            return ProductionProgress.RegisterClick();
        }
    }
}
