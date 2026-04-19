using System;
using System.Collections.Generic;
using Internal.Scripts.Camp;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.Screens.Camp
{
    public class CampScreen : PopupScreen
    {
        [SerializeField] private CampActionButtonView[] _actionButtons;
        [SerializeField] private ResourceIndicator[] _resourceViews;
        [SerializeField] private TextMeshProUGUI _dayText;
        [SerializeField] private LocalizedString _dayTextLocalizedString;

        private CampScreenViewModel _viewModel;
        private IDisposable _stateSubscription;
        private LocalizationService.LocalizedTextHandle _dayHandle;

        protected override void OnEnable()
        {
            base.OnEnable();
            SubscribeViewModel();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            UnsubscribeViewModel();
        }

        public override void BindViewModel(IScreenViewModel viewModel)
        {
            _viewModel = viewModel as CampScreenViewModel;
            SubscribeViewModel();
        }

        private void SubscribeViewModel()
        {
            if (_viewModel == null || _stateSubscription != null)
                return;

            SetupIcons();
            _stateSubscription = _viewModel.State.Subscribe(ApplyState);
            _viewModel.DayChanged += ApplyDay;
            ApplyDay(_viewModel.CurrentDay);
        }

        private void UnsubscribeViewModel()
        {
            _stateSubscription?.Dispose();
            _stateSubscription = null;

            if (_viewModel != null)
                _viewModel.DayChanged -= ApplyDay;

            _dayHandle?.Dispose();
            _dayHandle = null;
        }

        private void ApplyDay(int day)
        {
            if (_dayText == null) return;
            _dayHandle?.Dispose();
            if (_dayTextLocalizedString != null && Localization != null)
                _dayHandle = Localization.BindText(_dayText, _dayTextLocalizedString,
                    "Camp.Day", $"Day {day}", null, day);
            else
                _dayText.text = $"Day {day}";
        }

        private void ApplyState(CampViewState state)
        {
            if (state == null) return;

            ApplyResources(state.ResourceValues);

            if (_actionButtons == null || state.Actions == null)
                return;

            int count = Math.Min(_actionButtons.Length, state.Actions.Length);
            for (int i = 0; i < count; i++)
            {
                if (_actionButtons[i] == null) continue;
                var actionState = state.Actions[i];
                _actionButtons[i].Initialize(actionState, CreateActionHandler(actionState.ActionType));
            }
        }

        private void ApplyResources(IReadOnlyDictionary<ResourceType, float> values)
        {
            if (_resourceViews == null || values == null) return;
            foreach (var indicator in _resourceViews)
            {
                if (indicator == null) continue;
                if (!values.TryGetValue(indicator.ResourceType, out float value)) continue;
                indicator.ApplyImmediate(value, 0f);
            }
        }

        private void SetupIcons()
        {
            var icons = _viewModel?.ResourceIcons;
            if (icons == null || _resourceViews == null) return;
            foreach (var indicator in _resourceViews)
            {
                if (indicator == null) continue;
                indicator.SetIcon(icons.Get(indicator.ResourceType)?.Icon);
                indicator.HideChangeImmediate();
            }
        }

        private Action CreateActionHandler(CampActionType type)
        {
            return () => _viewModel?.ExecuteAction(type);
        }
    }
}
