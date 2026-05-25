using System.Collections.Generic;
using _Project.Scripts.Gameplay.Units;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules.Components
{
    [DisallowMultipleComponent]
    public class BattleMoleculeChargeRequirementVisual : MonoBehaviour
    {
        [field: SerializeField] private List<SpriteRenderer> ChargeSlots { get; set; } = new();
        [field: SerializeField, Min(0f)] private float SlotSpacing { get; set; } = 0.22f;
        [field: SerializeField] private Color EmptyColor { get; set; } = Color.gray;
        [field: SerializeField] private Color FilledColor { get; set; } = Color.white;

        private OwnedAtoms _ownedAtoms;
        private BattleMoleculeCharge _charge;
        private int _requiredAtoms;

        private void OnEnable()
        {
            Subscribe();
            Refresh();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void Configure(OwnedAtoms ownedAtoms, BattleMoleculeCharge charge, int requiredAtoms)
        {
            Unsubscribe();
            _ownedAtoms = ownedAtoms;
            _charge = charge;
            _requiredAtoms = Mathf.Clamp(requiredAtoms, 0, ChargeSlots.Count);
            Subscribe();
            Refresh();
        }

        private void Refresh()
        {
            int filledSlots = _charge != null && _charge.IsCharged
                ? _requiredAtoms
                : Mathf.Clamp(_ownedAtoms != null ? _ownedAtoms.Count : 0, 0, _requiredAtoms);

            for (int i = 0; i < ChargeSlots.Count; i++)
            {
                SpriteRenderer slot = ChargeSlots[i];

                if (slot == null)
                    continue;

                bool isRequired = i < _requiredAtoms;
                slot.gameObject.SetActive(isRequired);
                slot.transform.localPosition = SlotPosition(i);
                slot.color = i < filledSlots ? FilledColor : EmptyColor;
            }
        }

        private Vector3 SlotPosition(int index)
        {
            float startX = -SlotSpacing * (_requiredAtoms - 1) * 0.5f;
            return new Vector3(startX + SlotSpacing * index, 0f, 0f);
        }

        private void Subscribe()
        {
            if (!isActiveAndEnabled)
                return;

            if (_ownedAtoms != null)
                _ownedAtoms.Changed += Refresh;

            if (_charge != null)
            {
                _charge.Charged += Refresh;
                _charge.Spent += Refresh;
            }
        }

        private void Unsubscribe()
        {
            if (_ownedAtoms != null)
                _ownedAtoms.Changed -= Refresh;

            if (_charge != null)
            {
                _charge.Charged -= Refresh;
                _charge.Spent -= Refresh;
            }
        }
    }
}
