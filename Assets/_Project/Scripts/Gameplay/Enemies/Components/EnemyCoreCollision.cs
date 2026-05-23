using System.Collections.Generic;
using _Project.Scripts.Gameplay.Units.AtomCores;
using _Project.Scripts.Gameplay.Units.AtomCores.Components;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Enemies.Components
{
    [RequireComponent(typeof(Collider2D))]
    public class EnemyCoreCollision : MonoBehaviour
    {
        // TODO 1: вынести в сервис управления эффектами
        private const string CONTACT_EFFECT_PREFAB_PATH = "Test/EnemyCollisionEffect";

        [field: SerializeField] private Collider2D Collider { get; set; }

        private EnemyUnit _enemy;
        private AtomCore _core;
        private AtomCoreDamageHitArea _coreHitArea;

        // Contact buffer (GetContacts → ContactPoint2D, but properties are
        // read-only in Unity 6000, so we extract values into our own fields)
        private readonly List<ContactPoint2D> _contactBuffer = new();
        private Vector2 _lastContactPoint;
        private Vector2 _lastContactNormal;
        private float _lastContactSeparation;
        private bool _hasContact;

        protected AtomCore Core => _core;

        /// <summary>Contact point in world space from the most recent collision.</summary>
        public Vector2 LastContactPoint => _lastContactPoint;

        /// <summary>Surface normal at the contact point (points toward this collider).</summary>
        public Vector2 LastContactNormal => _lastContactNormal;

        /// <summary>
        /// Separation distance at the contact. Negative when overlapping (penetration),
        /// zero when touching, positive when separated (fallback only).
        /// </summary>
        public float LastContactSeparation => _lastContactSeparation;

        /// <summary>True if a collision was detected on the most recent Tick.</summary>
        public bool HasContact => _hasContact;

        private void Awake()
        {
            if (Collider == null)
                Collider = GetComponent<Collider2D>();

            _enemy = GetComponent<EnemyUnit>();
        }

        public void Configure(AtomCore core)
        {
            _core = core;
            _coreHitArea = core != null ? core.GetComponent<AtomCoreDamageHitArea>() : null;
        }

        public void Tick()
        {
            if (_enemy == null || !_enemy.IsAlive || _core == null || Collider == null || _coreHitArea == null)
                return;

            if (!TryGetCoreContact())
                return;

            ApplyCoreCollision();
            _enemy.DieFromCore();
        }

        public void Clear()
        {
            _core = null;
            _coreHitArea = null;
            _hasContact = false;
            _contactBuffer.Clear();
        }

        protected virtual void ApplyCoreCollision()
        {
            SpawnContactEffect();
            _core.TakeDamage(_enemy.CoreCollisionDamage);
        }

        private void SpawnContactEffect()
        {
            if (string.IsNullOrEmpty(CONTACT_EFFECT_PREFAB_PATH))
                return;

            GameObject prefab = Resources.Load<GameObject>(CONTACT_EFFECT_PREFAB_PATH);
            if (prefab == null)
                return;

            Quaternion rotation = _lastContactNormal != Vector2.zero
                ? Quaternion.LookRotation(Vector3.forward, _lastContactNormal)
                : Quaternion.identity;

            Instantiate(prefab, _lastContactPoint, rotation);
        }

        private bool TryGetCoreContact()
        {
            // Primary: physics contact points (richer data — point, normal, separation,
            //           multiple contacts per pair).
            // Available after Physics2D.Simulate() has run with synced transforms
            // (via EnemyService.TickCoreCollisions → PhysicsService.SyncTransforms).
            _contactBuffer.Clear();
            int count = Collider.GetContacts(_contactBuffer);

            for (int i = 0; i < count; i++)
            {
                ContactPoint2D c = _contactBuffer[i];
                if (c.otherCollider == _coreHitArea.Collider)
                {
                    _lastContactPoint = c.point;
                    _lastContactNormal = c.normal;
                    _lastContactSeparation = c.separation;
                    _hasContact = true;
                    return true;
                }
            }

            // Fallback: distance-based overlap when physics contacts aren't yet
            // available (e.g. early frame before FixedUpdate after movement).
            // Also catches any edge case where GetContacts misses an active collision.
            if (!_coreHitArea.GetOverlapInfo(Collider, out ColliderDistance2D distance))
            {
                _hasContact = false;
                return false;
            }

            _lastContactPoint = distance.pointB;
            _lastContactNormal = distance.normal;
            _lastContactSeparation = distance.distance;
            _hasContact = true;
            return true;
        }
    }
}
