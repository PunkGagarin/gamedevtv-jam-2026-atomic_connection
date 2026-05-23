using UnityEngine;
using _Project.Scripts.Gameplay.Units.AtomCores;

namespace _Project.Scripts.Gameplay.Enemies
{
    public readonly struct EnemyProjectileShot
    {
        public EnemyProjectileShot(
            Component owner,
            string prefabResourcePath,
            Vector3 position,
            AtomCore target,
            Vector3 direction,
            float speed,
            int damage,
            float lifetime)
        {
            Owner = owner;
            PrefabResourcePath = prefabResourcePath;
            Position = position;
            Target = target;
            Direction = direction;
            Speed = speed;
            Damage = damage;
            Lifetime = lifetime;
        }

        public Component Owner { get; }
        public string PrefabResourcePath { get; }
        public Vector3 Position { get; }
        public AtomCore Target { get; }
        public Vector3 Direction { get; }
        public float Speed { get; }
        public int Damage { get; }
        public float Lifetime { get; }
    }
}
