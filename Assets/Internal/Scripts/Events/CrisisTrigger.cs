using System;
using System.Collections.Generic;
using Internal.Scripts.Config;
using Internal.Scripts.Economy;
using Internal.Scripts.Events.Data;
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
        private readonly GameBalanceConfig _balanceConfig;

        public CrisisTrigger(
            DayTracker dayTracker,
            PlayerResourceRepository resources,
            CrisisEventConfig crisisConfig,
            EventTrigger eventTrigger,
            GameBalanceConfig balanceConfig)
        {
            _dayTracker = dayTracker;
            _resources = resources;
            _crisisConfig = crisisConfig;
            _eventTrigger = eventTrigger;
            _balanceConfig = balanceConfig;
        }

        public void Initialize()
        {
            _dayTracker.OnDayChanged += HandleDayChanged;
        }

        public void Dispose()
        {
            _dayTracker.OnDayChanged -= HandleDayChanged;
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

            if (state.Food <= 0f
                && TryTriggerRandom(_crisisConfig.OnFoodDepleted)) return;

            if (state.Money <= 0
                && TryTriggerRandom(_crisisConfig.OnMoneyDepleted)) return;

            if ((state.Companions == null || state.Companions.Count == 0)
                && (state.Carts == null || state.Carts.Count == 0)
                && TryTriggerRandom(_crisisConfig.OnCaravanLost)) return;
        }

#if UNITY_EDITOR
        public void EditorForceCheck() => CheckCrisis();
#endif

        private bool TryTriggerRandom(List<EventData> events)
        {
            if (events == null || events.Count == 0) return false;
            EventData picked = events[Random.Range(0, events.Count)];
            return picked != null && _eventTrigger.TriggerEvent(picked);
        }
    }
}
