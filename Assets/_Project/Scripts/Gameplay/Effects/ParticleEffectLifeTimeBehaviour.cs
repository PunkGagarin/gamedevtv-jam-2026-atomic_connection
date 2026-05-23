using UnityEngine;

namespace _Project.Scripts.Gameplay.Effects
{
    public class ParticleEffectLifeTimeBehaviour : MonoBehaviour
    {
        [field: SerializeField] private ParticleSystem Particle { get; set; }

        private void Awake()
        {
            if (Particle == null)
                Particle = GetComponent<ParticleSystem>();
        }

        private void Start()
        {
            if (Particle == null)
            {
                Debug.LogWarning($"ParticleSystem not found on {name}, destroying immediately.", this);
                Destroy(gameObject);
                return;
            }

            float duration = GetEffectDuration();
            Destroy(gameObject, duration);
        }

        private float GetEffectDuration()
        {
            ParticleSystem.MainModule main = Particle.main;

            if (main.loop)
                return main.duration;

            // Non-looping: total time from first emit to last particle death
            float maxLifetime = main.startLifetime.constantMax > 0
                ? main.startLifetime.constantMax
                : main.startLifetime.constant;

            return main.duration + maxLifetime;
        }
    }
}
