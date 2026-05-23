using UnityEngine;

namespace _Project.Scripts.Gameplay.Common.Progress
{
    public sealed class CompletionThreshold
    {
        private int _required;
        private int _current;

        public int Required => _required;
        public int Current => _current;
        public bool HasRequirement => _required > 0;
        public bool IsComplete => _required <= 0 || _current >= _required;
        public float Normalized => _required <= 0 ? 1f : Mathf.Clamp01((float)_current / _required);

        public void Configure(int required)
        {
            _required = Mathf.Max(0, required);
            Reset();
        }

        public void Reset()
        {
            _current = 0;
        }

        public bool Advance(int amount = 1)
        {
            if (amount <= 0 || IsComplete)
                return false;

            _current = Mathf.Min(_required, _current + amount);
            return IsComplete;
        }

        public bool SetCurrent(int current)
        {
            if (IsComplete)
                return false;

            _current = Mathf.Clamp(current, 0, _required);
            return IsComplete;
        }

        public int RemainingFrom(int current)
        {
            if (_required <= 0)
                return 0;

            return Mathf.Max(0, _required - current);
        }
    }
}
