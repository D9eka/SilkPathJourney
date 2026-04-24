using System;
using System.Collections.Generic;
using Internal.Scripts.Events;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.StackService;
using Zenject;

namespace Internal.Scripts.Travel.Triggers
{
    public sealed class DailyTriggerService : IInitializable, IDisposable
    {
        private readonly IReadOnlyList<IDailyTriggerAction> _actions;
        private readonly DayTracker _dayTracker;
        private readonly ScreenStackService _screens;

        public DailyTriggerService(
            List<IDailyTriggerAction> actions,
            DayTracker dayTracker,
            ScreenStackService screens)
        {
            _actions = actions;
            _dayTracker = dayTracker;
            _screens = screens;
        }

        public void Initialize() => _dayTracker.OnDayChanged += OnDay;

        public void Dispose() => _dayTracker.OnDayChanged -= OnDay;

        private void OnDay(int day)
        {
            if (_dayTracker.IsSkipping) return;
            if (_screens.IsOpen(ScreenId.Event)) return;

            foreach (IDailyTriggerAction action in _actions)
            {
                if (!action.CanTrigger()) continue;
                action.Trigger();
                return;
            }
        }
    }
}
