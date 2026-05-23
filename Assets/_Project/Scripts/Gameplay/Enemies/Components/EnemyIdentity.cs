using System;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Enemies.Components
{
    public class EnemyIdentity : MonoBehaviour
    {
        private EnemyDefinition _definition;
        private int _coreCollisionDamage = 1;

        public EnemyId Id => _definition?.Id ?? EnemyId.Standard;
        public int CoreCollisionDamage => _coreCollisionDamage;
        public int NucleotideReward => _definition?.NucleotideReward ?? 0;

        public void Configure(EnemyDefinition definition, int coreCollisionDamage)
        {
            _definition = definition ?? throw new ArgumentNullException(nameof(definition));
            _coreCollisionDamage = Mathf.Max(1, coreCollisionDamage);
        }
    }
}
