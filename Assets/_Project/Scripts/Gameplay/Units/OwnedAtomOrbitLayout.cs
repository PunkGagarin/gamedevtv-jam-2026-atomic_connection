using System.Collections.Generic;
using _Project.Scripts.Gameplay.Units.FreeAtoms;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Units
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(OwnedAtoms))]
    public class OwnedAtomOrbitLayout : MonoBehaviour
    {
        private enum RadiusMode
        {
            Fixed,
            OwnerPlusAtom
        }

        private readonly List<FreeAtom> _atomsBuffer = new();

        [field: SerializeField] private OwnedAtoms OwnedAtoms { get; set; }

        private FreeAtomOwnerKind _ownerKind;
        private RadiusMode _radiusMode;
        private float _fixedRadius;
        private float _ownerRadius;

        private void Awake()
        {
            if (OwnedAtoms == null)
                OwnedAtoms = GetComponent<OwnedAtoms>();

            if (OwnedAtoms != null)
                OwnedAtoms.Changed += Arrange;
        }

        private void OnDestroy()
        {
            if (OwnedAtoms != null)
                OwnedAtoms.Changed -= Arrange;
        }

        public void ConfigureFixedRadius(FreeAtomOwnerKind ownerKind, float radius)
        {
            _ownerKind = ownerKind;
            _radiusMode = RadiusMode.Fixed;
            _fixedRadius = radius;
            Arrange();
        }

        public void ConfigureOwnerPlusAtomRadius(FreeAtomOwnerKind ownerKind, float ownerRadius)
        {
            _ownerKind = ownerKind;
            _radiusMode = RadiusMode.OwnerPlusAtom;
            _ownerRadius = ownerRadius;
            Arrange();
        }

        private void Arrange()
        {
            if (OwnedAtoms == null)
                return;

            OwnedAtoms.GetOwned(_ownerKind, _atomsBuffer);

            for (int i = _atomsBuffer.Count - 1; i >= 0; i--)
            {
                if (_atomsBuffer[i] == null || !_atomsBuffer[i].CanArrangeInOrbit)
                    _atomsBuffer.RemoveAt(i);
            }

            int count = _atomsBuffer.Count;
            if (count <= 0)
                return;

            float startAngle = GetStartAngle(_atomsBuffer[0]);
            float angleStep = Mathf.PI * 2f / count;

            for (int i = 0; i < count; i++)
                ArrangeAtom(_atomsBuffer[i], startAngle + angleStep * i);
        }

        private void ArrangeAtom(FreeAtom atom, float angle)
        {
            if (atom == null || atom.OrbitMotion == null)
                return;

            atom.OrbitMotion.Configure(transform, RadiusFor(atom), angle);
        }

        private float RadiusFor(FreeAtom atom)
        {
            return _radiusMode == RadiusMode.OwnerPlusAtom
                ? _ownerRadius + GetColliderRadius(atom.transform)
                : _fixedRadius;
        }

        private float GetStartAngle(FreeAtom atom)
        {
            if (atom == null)
                return 0;

            Vector3 offset = atom.transform.position - transform.position;
            return offset.sqrMagnitude > 0.001f
                ? Mathf.Atan2(offset.y, offset.x)
                : 0;
        }

        private static float GetColliderRadius(Transform target)
        {
            Collider2D col = target.GetComponent<Collider2D>();

            if (col == null)
                return 0;

            Vector3 extents = col.bounds.extents;
            return Mathf.Max(extents.x, extents.y);
        }
    }
}
