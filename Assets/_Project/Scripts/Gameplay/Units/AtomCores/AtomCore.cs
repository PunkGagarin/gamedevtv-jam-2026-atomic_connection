using System;
using _Project.Scripts.Gameplay.Units.AtomCores.Components;
using _Project.Scripts.Gameplay.Units.FreeAtoms;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.AtomCores
{
    [RequireComponent(typeof(OwnedAtoms))]
    [RequireComponent(typeof(OwnedAtomReceiver))]
    [RequireComponent(typeof(OwnedAtomOrbitLayout))]
    [RequireComponent(typeof(ObjectRadius))]
    [RequireComponent(typeof(PointHitArea))]
    [RequireComponent(typeof(AtomOrbit))]
    [RequireComponent(typeof(AtomProductionProgress))]
    [RequireComponent(typeof(AtomCoreHealth))]
    [RequireComponent(typeof(AtomCoreClickInteraction))]
    public class AtomCore : MonoBehaviour
    {
        [field: SerializeField] private OwnedAtomReceiver AtomReceiver { get; set; }
        [field: SerializeField] private OwnedAtomOrbitLayout AtomOrbitLayout { get; set; }
        [field: SerializeField] private AtomOrbit AtomOrbit { get; set; }
        [field: SerializeField] private AtomCoreHealth Health { get; set; }
        [field: SerializeField] private AtomCoreClickInteraction ClickInteraction { get; set; }

        public bool IsAlive => Health.IsAlive;

        public event Action Died
        {
            add => Health.Died += value;
            remove => Health.Died -= value;
        }

        private void Awake()
        {
            AtomReceiver = GetComponent<OwnedAtomReceiver>();
            AtomOrbitLayout = GetComponent<OwnedAtomOrbitLayout>();
            AtomOrbit = GetComponent<AtomOrbit>();
            Health = GetComponent<AtomCoreHealth>();
            ClickInteraction = GetComponent<AtomCoreClickInteraction>();
        }

        public void Configure(AtomCoreConfig config, int clicksRequired, int maxHealth)
        {
            Health.Configure(maxHealth);
            AtomReceiver.Configure(FreeAtomOwnerKind.Core);
            AtomOrbitLayout.ConfigureFixedRadius(FreeAtomOwnerKind.Core, config.FreeAtomOrbitRadius);
            AtomOrbit.Configure(config.FreeAtomOrbitDegreesPerSecond);
            ClickInteraction.Configure(clicksRequired);
        }

        public void TakeGeneratedAtom(FreeAtom freeAtom)
        {
            AtomReceiver.TryTake(freeAtom);
        }

        public void Tick(float deltaTime)
        {
            Health.Tick(deltaTime);
            AtomOrbit.Tick(deltaTime);
        }

        public bool ContainsPoint(Vector2 worldPosition)
        {
            return ClickInteraction.Contains(worldPosition);
        }

        public bool RegisterAtomClick()
        {
            return ClickInteraction.RegisterClick();
        }

        public void TakeDamage(int amount)
        {
            Health.TakeDamage(amount);
        }

        public void Kill()
        {
            Health.Kill();
        }

        public void CleanupAtoms()
        {
            AtomReceiver.ReleaseAll();
        }
    }
}
