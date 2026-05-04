using System;
using System.Collections.Generic;
using Internal.Scripts.Config;
using Internal.Scripts.Economy;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Outcomes;
using Internal.Scripts.Inventory;
using Internal.Scripts.Items;
using Internal.Scripts.Meta;
using Zenject;
using Random = UnityEngine.Random;

namespace Internal.Scripts.Events
{
    public class CrisisTrigger : IInitializable, IDisposable
    {
        private readonly DayTracker _dayTracker;
        private readonly PlayerResourceRepository _resources;
        private readonly CrisisEventConfig _crisisConfig;
        private readonly EventTrigger _eventTrigger;
        private readonly EventCloseSignal _closeSignal;
        private readonly GameBalanceConfig _balanceConfig;
        private readonly InventoryRepository _inventoryRepository;
        private readonly RunStatsService _runStats;
        private readonly RunEndOutcomeApplier _runEndApplier;

        private EventData _activeCrisisEvent;

        public CrisisTrigger(
            DayTracker dayTracker,
            PlayerResourceRepository resources,
            CrisisEventConfig crisisConfig,
            EventTrigger eventTrigger,
            EventCloseSignal closeSignal,
            GameBalanceConfig balanceConfig,
            InventoryRepository inventoryRepository,
            RunStatsService runStats,
            RunEndOutcomeApplier runEndApplier)
        {
            _dayTracker = dayTracker;
            _resources = resources;
            _crisisConfig = crisisConfig;
            _eventTrigger = eventTrigger;
            _closeSignal = closeSignal;
            _balanceConfig = balanceConfig;
            _inventoryRepository = inventoryRepository;
            _runStats = runStats;
            _runEndApplier = runEndApplier;
        }

        public void Initialize()
        {
            _dayTracker.OnDayChanged += HandleDayChanged;
            _closeSignal.Closed += HandleEventClosed;
        }

        public void Dispose()
        {
            _dayTracker.OnDayChanged -= HandleDayChanged;
            _closeSignal.Closed -= HandleEventClosed;
            _activeCrisisEvent = null;
        }

        private void HandleEventClosed(EventData ev)
        {
            if (_activeCrisisEvent == null || ev != _activeCrisisEvent) return;
            _activeCrisisEvent = null;
            if (!_runEndApplier.IsEnded)
                _runStats.RecordCrisisSurvived();
        }

        private void HandleDayChanged(int currentDay)
        {
            if (_dayTracker.IsSkipping) return;

            CheckCrisis();
        }

        private void CheckCrisis()
        {
            var state = _resources.Current;

            if (state.AccumulatedDanger >= _balanceConfig.MaxDanger
                && TryTriggerRandom(_crisisConfig.OnDangerMaxed)) return;

            if (state.Morale <= 0f
                && TryTriggerRandom(_crisisConfig.OnMoraleDepleted)) return;

            if (state.PlayerCart != null && state.PlayerCart.Durability <= 0f
                && TryTriggerRandom(_crisisConfig.OnCartBroken)) return;

            if (!_inventoryRepository.HasPlayerItem(SuppliesItemId.Value)
                && TryTriggerRandom(_crisisConfig.OnFoodDepleted)) return;

            if (state.Money <= 0
                && TryTriggerRandom(_crisisConfig.OnMoneyDepleted)) return;

            bool noCart = state.PlayerCart == null
                          && (state.Carts == null || state.Carts.Count == 0);
            bool noCompanions = state.Companions == null || state.Companions.Count == 0;
            if (noCart && noCompanions
                && TryTriggerRandom(_crisisConfig.OnCaravanLost)) return;
        }

#if UNITY_EDITOR
        public void EditorForceCheck() => CheckCrisis();
#endif

        private bool TryTriggerRandom(List<EventData> events)
        {
            if (events == null || events.Count == 0) return false;
            EventData picked = events[Random.Range(0, events.Count)];
            if (picked == null) return false;
            if (!_eventTrigger.TriggerEvent(picked)) return false;
            _activeCrisisEvent = picked;
            return true;
        }
    }
}
