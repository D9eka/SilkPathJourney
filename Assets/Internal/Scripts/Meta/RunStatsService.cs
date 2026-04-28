using System;
using System.Collections.Generic;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Events;
using Internal.Scripts.Inventory;
using Internal.Scripts.Player;
using Internal.Scripts.Player.Languages;
using Internal.Scripts.Quests;
using Internal.Scripts.Save;
using Internal.Scripts.Trading;
using R3;
using Zenject;

namespace Internal.Scripts.Meta
{
    public sealed class RunStatsService : IInitializable, IDisposable
    {
        private readonly SaveRepository _saveRepository;
        private readonly DayTracker _dayTracker;
        private readonly ICityEntryService _cityEntryService;
        private readonly QuestRepository _questRepository;
        private readonly TradeModel _tradeModel;
        private readonly CompanionService _companionService;
        private readonly PlayerLanguageRepository _languageRepository;
        private readonly PlayerResourceRepository _resourceRepository;
        private readonly InventoryModel _inventoryModel;

        private IDisposable _languageSubscription;
        private IDisposable _inventorySubscription;
        private bool _languageBaselineSet;
        private readonly HashSet<string> _baselineLanguageIds = new();

        public RunStatsData Stats => _saveRepository.Data.RunStats ??= new RunStatsData();

        public RunStatsService(
            SaveRepository saveRepository,
            DayTracker dayTracker,
            ICityEntryService cityEntryService,
            QuestRepository questRepository,
            TradeModel tradeModel,
            CompanionService companionService,
            PlayerLanguageRepository languageRepository,
            PlayerResourceRepository resourceRepository,
            InventoryModel inventoryModel)
        {
            _saveRepository = saveRepository;
            _dayTracker = dayTracker;
            _cityEntryService = cityEntryService;
            _questRepository = questRepository;
            _tradeModel = tradeModel;
            _companionService = companionService;
            _languageRepository = languageRepository;
            _resourceRepository = resourceRepository;
            _inventoryModel = inventoryModel;
        }

        public void Initialize()
        {
            _dayTracker.OnDayChanged += HandleDayChanged;
            _cityEntryService.OnCityEntered += HandleCityEntered;
            _questRepository.QuestCompleted += HandleQuestCompleted;
            _questRepository.QuestFailed += HandleQuestFailed;
            _tradeModel.TradeExecuted += HandleTradeExecuted;
            _companionService.CompanionHired += HandleCompanionHired;
            _companionService.CompanionRemoved += HandleCompanionRemoved;
            _languageSubscription = _languageRepository.StateStream.Subscribe(HandleLanguageStateChanged);
            _inventorySubscription = _inventoryModel.State.Subscribe(HandleInventoryChanged);
        }

        public void Dispose()
        {
            _dayTracker.OnDayChanged -= HandleDayChanged;
            _cityEntryService.OnCityEntered -= HandleCityEntered;
            _questRepository.QuestCompleted -= HandleQuestCompleted;
            _questRepository.QuestFailed -= HandleQuestFailed;
            _tradeModel.TradeExecuted -= HandleTradeExecuted;
            _companionService.CompanionHired -= HandleCompanionHired;
            _companionService.CompanionRemoved -= HandleCompanionRemoved;
            _languageSubscription?.Dispose();
            _languageSubscription = null;
            _inventorySubscription?.Dispose();
            _inventorySubscription = null;
        }

        private void HandleDayChanged(int day)
        {
            Stats.DaysTravelled = day - 1;
        }

        private void HandleCityEntered(CityData city)
        {
            if (!string.IsNullOrEmpty(city?.Id))
                Stats.AddCity(city.Id);
        }

        private void HandleTradeExecuted(
            Dictionary<string, int> bought,
            Dictionary<string, int> sold,
            int buyTotal,
            int sellTotal)
        {
            if (buyTotal > 0)
            {
                Stats.MoneySpent += buyTotal;
                foreach (KeyValuePair<string, int> kvp in bought)
                    Stats.ItemsBought += kvp.Value;
            }

            if (sellTotal > 0)
            {
                Stats.MoneyEarned += sellTotal;
                foreach (KeyValuePair<string, int> kvp in sold)
                    Stats.ItemsSold += kvp.Value;
            }

            int profit = sellTotal - buyTotal;
            if (profit > Stats.BestDeal.Profit && sold.Count > 0)
            {
                foreach (KeyValuePair<string, int> kvp in sold)
                {
                    Stats.BestDeal.ItemId = kvp.Key;
                    Stats.BestDeal.Profit = profit;
                    break;
                }
            }
        }

        private void HandleQuestCompleted(string _) => Stats.QuestsCompleted++;

        private void HandleQuestFailed(string _) => Stats.QuestsFailed++;

        private void HandleCompanionHired() => Stats.CompanionsHired++;

        private void HandleCompanionRemoved() => Stats.CompanionsLost++;

        private void HandleInventoryChanged(InventoryViewState state)
        {
            if (state.MaxWeight <= 0f) return;
            int loadPercent = UnityEngine.Mathf.Clamp(
                UnityEngine.Mathf.RoundToInt(state.CurrentWeight / state.MaxWeight * 100f), 0, 100);
            if (loadPercent > Stats.PeakLoadPercent)
                Stats.PeakLoadPercent = loadPercent;
        }

        private void HandleLanguageStateChanged(PlayerLanguageState state)
        {
            if (state?.Languages == null) return;

            if (!_languageBaselineSet)
            {
                foreach (LanguageEntry entry in state.Languages)
                    if (entry.Proficiency > LanguageProficiency.None)
                        _baselineLanguageIds.Add(entry.Type.ToString());
                _languageBaselineSet = true;
                return;
            }

            foreach (LanguageEntry entry in state.Languages)
            {
                if (entry.Proficiency <= LanguageProficiency.None) continue;
                string id = entry.Type.ToString();
                if (_baselineLanguageIds.Contains(id)) continue;
                if (Stats.LanguagesLearnedList.Contains(id)) continue;
                Stats.AddLanguage(id);
                Stats.LanguageLevelsGained++;
            }
        }

        public void RecordRepairDone() => Stats.RepairsDone++;

        public void RecordCartBought() => Stats.CartsBought++;

        public void RecordAnimalLost() => Stats.AnimalsLost++;

        public void RecordCrisisSurvived() => Stats.CrisesSurvived++;

        public void RecordEventResolved(bool success)
        {
            if (success)
                Stats.EventsResolvedSuccess++;
            else
                Stats.EventsResolvedFail++;
        }

        public void RecordHazard() => Stats.TotalHazards++;

        public void Snapshot()
        {
            Stats.CompanionsFinal = _resourceRepository.Current.Companions.Count;
        }
    }
}
