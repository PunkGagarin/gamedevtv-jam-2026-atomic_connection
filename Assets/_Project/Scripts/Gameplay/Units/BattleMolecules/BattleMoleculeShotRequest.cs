using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules
{
    public readonly struct BattleMoleculeShotRequest
    {
        public BattleMoleculeShotRequest(Vector3 origin, Vector3 direction, BattleMoleculeShotKind kind)
        {
            Origin = origin;
            Direction = direction;
            Kind = kind;
        }

        public Vector3 Origin { get; }
        public Vector3 Direction { get; }
        public BattleMoleculeShotKind Kind { get; }
    }

    public enum BattleMoleculeShotKind
    {
        Regular = 0,
        Mass = 1,
    }
}
