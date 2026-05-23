using System;
using _Project.Scripts.Gameplay.Units.BattleMolecules;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BattleMolecule))]
    [RequireComponent(typeof(BattleMoleculeBond))]
    public class BattleMoleculeBondEventRelay : MonoBehaviour
    {
        [field: SerializeField] private BattleMolecule Molecule { get; set; }
        [field: SerializeField] private BattleMoleculeBond Bond { get; set; }

        public event Action<BattleMolecule> Bonded;

        private void Awake()
        {
            if (Molecule == null)
                Molecule = GetComponent<BattleMolecule>();

            if (Bond == null)
                Bond = GetComponent<BattleMoleculeBond>();
        }

        private void OnEnable()
        {
            if (Bond != null)
                Bond.Bonded += OnBonded;
        }

        private void OnDisable()
        {
            if (Bond != null)
                Bond.Bonded -= OnBonded;
        }

        private void OnBonded()
        {
            Bonded?.Invoke(Molecule);
        }
    }
}
