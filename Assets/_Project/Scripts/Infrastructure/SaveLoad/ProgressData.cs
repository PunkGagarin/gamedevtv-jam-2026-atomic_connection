using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Gameplay.Currencies;
using UnityEngine;

namespace _Project.Scripts.Infrastructure.SaveLoad
{
    [Serializable]
    public class ProgressData
    {
        public int ProgressVersion;
        public int Gold;
        public int CompletedLevelCount;
        public int SelectedLevel = 1;
        public List<CurrencySlot> Currencies = new();
        public List<TalentSlot> TalentLevels = new();

        public int GetCurrencyAmount(CurrencyId currencyId)
        {
            EnsureLegacyGoldMigrated();

            CurrencySlot slot = Currencies.FirstOrDefault(s => s.CurrencyId == (int)currencyId);
            return slot?.Amount ?? 0;
        }

        public void SetCurrencyAmount(CurrencyId currencyId, int amount)
        {
            EnsureLegacyGoldMigrated();

            for (int i = 0; i < Currencies.Count; i++)
            {
                if (Currencies[i].CurrencyId == (int)currencyId)
                {
                    Currencies[i].Amount = amount;
                    return;
                }
            }

            Currencies.Add(new CurrencySlot { CurrencyId = (int)currencyId, Amount = amount });
        }

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
            EnsureLegacyGoldMigrated();
            return JsonUtility.ToJson(this);
        }

        public static ProgressData FromJson(string json)
        {
            return JsonUtility.FromJson<ProgressData>(json);
        }

        private void EnsureLegacyGoldMigrated()
        {
            Currencies ??= new List<CurrencySlot>();

            if (Gold <= 0 || Currencies.Any(s => s.CurrencyId == (int)CurrencyId.Nucleotides))
                return;

            Currencies.Add(new CurrencySlot { CurrencyId = (int)CurrencyId.Nucleotides, Amount = Gold });
            Gold = 0;
        }
    }

    [Serializable]
    public class CurrencySlot
    {
        public int CurrencyId;
        public int Amount;
    }

    [Serializable]
    public class TalentSlot
    {
        public int TalentId;
        public int Level;
    }
}
