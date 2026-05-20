using UnityEngine;

namespace _Project.Scripts.Gameplay.Units.BattleMolecules
{
    public readonly struct BattleMoleculeShotRequest
    {
        public BattleMoleculeShotRequest(Vector3 origin, Vector3 direction, BattleMoleculeShotKind kind, int shotSequenceId = 0)
        {
            Origin = origin;
            Direction = direction;
            Kind = kind;
            ShotSequenceId = shotSequenceId;
        }

        public Vector3 Origin { get; }
        public Vector3 Direction { get; }
        public BattleMoleculeShotKind Kind { get; }
        public int ShotSequenceId { get; }
    }

    public enum BattleMoleculeShotKind
    {
        Regular = 0,
        Mass = 1,
    }
}
