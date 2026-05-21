using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.AtomCores.Components
{
    public class AtomCoreShield : MonoBehaviour
    {
        [field: SerializeField] private SpriteRenderer Icon { get; set; }

        private float _duration;
        private float _timeRemaining;
        private float _secondsLostPerDamage;

        public bool IsActive => _timeRemaining > 0f;
        public float NormalizedTime => _duration > 0f ? Mathf.Clamp01(_timeRemaining / _duration) : 0f;

        private void Awake()
        {
            UpdateVisual();
        }

        public void Activate(float duration, float secondsLostPerDamage)
        {
            _duration = Mathf.Max(0.01f, duration);
            _timeRemaining = _duration;
            _secondsLostPerDamage = Mathf.Max(0f, secondsLostPerDamage);
            UpdateVisual();
        }

        public void Tick(float deltaTime)
        {
            if (!IsActive)
                return;

            _timeRemaining = Mathf.Max(0f, _timeRemaining - deltaTime);
            UpdateVisual();
        }

        public bool TryAbsorbDamage(int damage)
        {
            if (!IsActive)
                return false;

            float lostTime = Mathf.Max(0, damage) * _secondsLostPerDamage;
            _timeRemaining = Mathf.Max(0f, _timeRemaining - lostTime);
            UpdateVisual();

            return true;
        }

        private void UpdateVisual()
        {
            if (Icon != null)
                Icon.gameObject.SetActive(IsActive);
        }
    }
}
