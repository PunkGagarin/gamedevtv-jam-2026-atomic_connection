using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Enemies
{
    internal sealed class EnemyMergeGroup
    {
        private readonly List<EnemyUnit> _members = new();
        private readonly List<ActiveEnemyMergeLink> _links = new();
        private readonly List<EnemyUnit> _deathWaveMembers = new();

        public int Count => _members.Count;
        public int MaxHealth { get; private set; }
        public int CurrentHealth { get; private set; }
        public bool IsDeathWaveActive { get; private set; }

        private bool _deathWaveKilledByPlayer;
        private int _deathWaveIndex;
        private float _configuredDeathWaveStepSeconds;
        private float _deathWaveStepSeconds;
        private float _timeToNextDeath;
        private float _deathWaveKillRewardMultiplier = 1f;

        public int CoreCollisionDamage
        {
            get
            {
                int damage = 0;

                foreach (EnemyUnit member in _members)
                {
                    if (member != null && member.IsAlive)
                        damage += member.MergeCoreCollisionDamageContribution;
                }

                return damage;
            }
        }

        public bool Contains(EnemyUnit enemy)
        {
            return _members.Contains(enemy);
        }

        public void ConfigureDeathWave(float stepSeconds)
        {
            _configuredDeathWaveStepSeconds = stepSeconds;
        }

        public void AddLink(ActiveEnemyMergeLink link)
        {
            if (link != null && !_links.Contains(link))
                _links.Add(link);
        }

        public void AddUngrouped(EnemyUnit enemy)
        {
            if (enemy == null || Contains(enemy))
                return;

            MaxHealth += enemy.MergeMaxHealthContribution;
            CurrentHealth += enemy.MergeCurrentHealthContribution;
            _members.Add(enemy);
            enemy.AssignMergeGroup(this);
            SyncHealth();
        }

        public void MergeWith(EnemyMergeGroup other)
        {
            if (other == null || other == this)
                return;

            MaxHealth += other.MaxHealth;
            CurrentHealth += other.CurrentHealth;

            foreach (EnemyUnit member in other._members)
            {
                if (member == null || Contains(member))
                    continue;

                _members.Add(member);
                member.AssignMergeGroup(this);
            }

            foreach (ActiveEnemyMergeLink link in other._links)
            {
                if (link != null && !_links.Contains(link))
                    _links.Add(link);
            }

            other._members.Clear();
            other._links.Clear();
            other.MaxHealth = 0;
            other.CurrentHealth = 0;
            SyncHealth();
        }

        public void TakeDamage(int amount, EnemyUnit damagedMember, float killRewardMultiplier, bool isCritical)
        {
            if (amount <= 0 || IsDeathWaveActive)
                return;

            CurrentHealth -= amount;
            damagedMember?.ApplyMergeDamage(amount, isCritical);

            if (CurrentHealth <= 0)
            {
                StartDeathWave(damagedMember, true, killRewardMultiplier);
                return;
            }

            SyncHealth();
        }

        public void DieFromCore(EnemyUnit origin)
        {
            StartDeathWave(origin, false, 1f);
        }

        public void DieFromMemberKill(EnemyUnit origin)
        {
            StartDeathWave(origin, true, 1f);
        }

        public void TickDeathWave(float deltaTime)
        {
            if (!IsDeathWaveActive)
                return;

            _timeToNextDeath -= deltaTime;

            while (_deathWaveIndex < _deathWaveMembers.Count && _timeToNextDeath <= 0f)
            {
                KillDeathWaveMember(_deathWaveMembers[_deathWaveIndex]);
                _deathWaveIndex++;
                _timeToNextDeath += _deathWaveStepSeconds;

                if (_deathWaveStepSeconds <= 0f)
                    _timeToNextDeath = 0f;
            }

            if (_deathWaveIndex < _deathWaveMembers.Count)
                return;

            ClearMembers();
        }

        public void ClearMembers()
        {
            DestroyAllLinks();

            foreach (EnemyUnit member in _members)
                member?.ClearMergeGroupReference(this);

            _members.Clear();
            MaxHealth = 0;
            CurrentHealth = 0;
            IsDeathWaveActive = false;
            _deathWaveMembers.Clear();
        }

        public void ReleaseDeathWaveMember(EnemyUnit member)
        {
            if (!IsDeathWaveActive || member == null)
                return;

            _members.Remove(member);
            member.ClearMergeGroupReference(this);
        }

        private void StartDeathWave(EnemyUnit origin, bool killedByPlayer, float killRewardMultiplier)
        {
            if (IsDeathWaveActive)
                return;

            IsDeathWaveActive = true;
            _deathWaveKilledByPlayer = killedByPlayer;
            _deathWaveKillRewardMultiplier = Mathf.Max(0f, killedByPlayer ? killRewardMultiplier : 1f);
            _deathWaveIndex = 0;
            _deathWaveStepSeconds = _configuredDeathWaveStepSeconds;
            _timeToNextDeath = 0f;
            BuildDeathWave(origin);
        }

        private void BuildDeathWave(EnemyUnit origin)
        {
            _deathWaveMembers.Clear();

            if (origin != null && Contains(origin))
                AddWaveBranch(origin);

            foreach (EnemyUnit member in _members)
                AddWaveBranch(member);
        }

        private void AddWaveBranch(EnemyUnit origin)
        {
            if (origin == null || _deathWaveMembers.Contains(origin))
                return;

            Queue<EnemyUnit> queue = new();
            queue.Enqueue(origin);
            _deathWaveMembers.Add(origin);

            while (queue.Count > 0)
            {
                EnemyUnit current = queue.Dequeue();

                foreach (ActiveEnemyMergeLink link in _links)
                {
                    EnemyUnit neighbor = link.NeighborOf(current);

                    if (neighbor == null || !Contains(neighbor) || _deathWaveMembers.Contains(neighbor))
                        continue;

                    _deathWaveMembers.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        private void KillDeathWaveMember(EnemyUnit member)
        {
            if (member == null)
                return;

            DestroyLinksFor(member);

            if (_deathWaveKilledByPlayer)
                member.DieFromMergeGroupDamage(_deathWaveKillRewardMultiplier);
            else
                member.DieFromMergeGroupCore();
        }

        private void DestroyLinksFor(EnemyUnit member)
        {
            for (int i = _links.Count - 1; i >= 0; i--)
            {
                ActiveEnemyMergeLink link = _links[i];

                if (link == null || !link.Contains(member))
                    continue;

                link.DestroyView();
                _links.RemoveAt(i);
            }
        }

        private void DestroyAllLinks()
        {
            foreach (ActiveEnemyMergeLink link in _links)
                link?.DestroyView();

            _links.Clear();
        }

        private void SyncHealth()
        {
            foreach (EnemyUnit member in _members)
                member?.SyncMergeHealth(MaxHealth, CurrentHealth);
        }
    }
}
