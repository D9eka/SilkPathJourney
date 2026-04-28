using System;
using System.Collections.Generic;

namespace Internal.Scripts.Meta
{
    public enum EndType
    {
        None = 0,
        Victory = 1,
        Defeat_Bankruptcy = 2,
        Defeat_Mutiny = 3,
        Defeat_CaravanLost = 4,
        Defeat_Famine = 5
    }

    [Serializable]
    public class BestDealRecord
    {
        public string ItemId;
        public string FromCityId;
        public string ToCityId;
        public int Profit;
    }

    [Serializable]
    public class RunStatsData
    {
        // General
        public int DaysTravelled;
        public List<string> CitiesVisitedList = new();
        public float KilometersTravelled;

        // Economy
        public int MoneyEarned;
        public int MoneySpent;
        public BestDealRecord BestDeal = new();
        public int ItemsBought;
        public int ItemsSold;

        // Caravan
        public int CompanionsHired;
        public int CompanionsLost;
        public int CompanionsFinal;
        public int CartsBought;
        public int RepairsDone;
        public int AnimalsLost;
        public int PeakLoadPercent;

        // Adventures
        public int EventsResolvedSuccess;
        public int EventsResolvedFail;
        public int CrisesSurvived;
        public int TotalHazards;
        public int QuestsCompleted;
        public int QuestsFailed;

        // Social
        public List<string> LanguagesLearnedList = new();
        public int LanguageLevelsGained;

        // End
        public EndType EndType;
        public string EndReasonKey;
        public int LegacyEarned;
        public int LegacyBonus;

        public void AddCity(string cityId)
        {
            if (!string.IsNullOrEmpty(cityId) && !CitiesVisitedList.Contains(cityId))
                CitiesVisitedList.Add(cityId);
        }

        public void AddLanguage(string languageId)
        {
            if (!string.IsNullOrEmpty(languageId) && !LanguagesLearnedList.Contains(languageId))
                LanguagesLearnedList.Add(languageId);
        }

        public bool HasVisitedCity(string cityId) => CitiesVisitedList.Contains(cityId);
    }
}
