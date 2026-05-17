using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Project.Scripts.Infrastructure.SaveLoad
{
    [Serializable]
    public class ProgressData
    {
        public int Gold;
        public List<TalentSlot> TalentLevels = new();

        public int GetTalentLevel(int talentId)
        {
            TalentSlot slot = TalentLevels.FirstOrDefault(s => s.TalentId == talentId);
            return slot?.Level ?? 0;
        }

        public void SetTalentLevel(int talentId, int level)
        {
            for (int i = 0; i < TalentLevels.Count; i++)
            {
                if (TalentLevels[i].TalentId == talentId)
                {
                    TalentLevels[i].Level = level;
                    return;
                }
            }
            TalentLevels.Add(new TalentSlot { TalentId = talentId, Level = level });
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public static ProgressData FromJson(string json)
        {
            return JsonUtility.FromJson<ProgressData>(json);
        }
    }

    [Serializable]
    public class TalentSlot
    {
        public int TalentId;
        public int Level;
    }
}
