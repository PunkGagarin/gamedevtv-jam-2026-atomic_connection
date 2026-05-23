using System;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.AtomCores.Components
{
    public class AtomCoreShield : MonoBehaviour
    {
        [field: SerializeField] private SpriteRenderer Icon { get; set; }

        private float _duration;
        private float _timeRemaining;
        private int _integrity;

        public bool IsActive => _timeRemaining > 0f && _integrity > 0;
        public float NormalizedTime => _duration > 0f ? Mathf.Clamp01(_timeRemaining / _duration) : 0f;

        public event Action Broken;
        public event Action Expired;

        private void Awake()
        {
            UpdateVisual();
        }

        public void Activate(float duration, int integrity)
        {
            _duration = Mathf.Max(0.01f, duration);
            _timeRemaining = _duration;
            _integrity = Mathf.Max(1, integrity);
            UpdateVisual();
        }

        public void Tick(float deltaTime)
        {
            if (!IsActive)
                return;

            _timeRemaining = Mathf.Max(0f, _timeRemaining - deltaTime);
            UpdateVisual();

            if (!IsActive)
                Expired?.Invoke();
        }

        public bool TryAbsorbDamage(int damage)
        {
            if (!IsActive)
                return false;

            _integrity = Mathf.Max(0, _integrity - Mathf.Max(0, damage));
            UpdateVisual();

            if (!IsActive)
                Broken?.Invoke();

            return true;
        }

        private void UpdateVisual()
        {
            if (Icon != null)
                Icon.gameObject.SetActive(IsActive);
        }
    }
}
