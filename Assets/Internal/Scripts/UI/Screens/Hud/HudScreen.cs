using System;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Events;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Core.View;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.World.State;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.Hud
{
    public class HudScreen : ScreenViewBase
    {
        [SerializeField] private MinorEventView _minorEventView;
        [SerializeField] private GameObject _cityTextContainer;
        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI _startActionButtonText;
        [SerializeField] private TextMeshProUGUI _actionButtonText;
        [SerializeField] private TextMeshProUGUI _endActionButtonText;
        [SerializeField] private TextMeshProUGUI _dayText;
        [SerializeField] private TextMeshProUGUI _cityText;
        [Header("ResourceIndicators")]
        [SerializeField] private SliderResourceIndicator _playerCaretDurabilitySlider;
        [SerializeField] private SliderResourceIndicator _otherCaretsDurabilitySlider;
        [SerializeField] private ResourceIndicator _foodIndicator;
        [SerializeField] private SliderResourceIndicator _dangerSlider;
        [Header("MovementActionButtons")]
        [SerializeField] private Button _startActionButton;
        [SerializeField] private Button _actionButton;
        [SerializeField] private Button _endActionButton;
        [Header("Borders")]
        [SerializeField] private GameObject _startActionBorder;
        [SerializeField] private GameObject _actionBorder;
        [SerializeField] private GameObject _endActionBorder;
        [Header("TimeSpeedButtons")]
        [SerializeField] private Button _pauseTimeButton;
        [SerializeField] private Button _normalTimeButton;
        [SerializeField] private Button _fastTimeButton;
        [SerializeField] private Button _veryFastTimeButton;
        [Header("TimeSpeedBorders")]
        [SerializeField] private GameObject _pauseTimeBorder;
        [SerializeField] private GameObject _normalTimeBorder;
        [SerializeField] private GameObject _fastTimeBorder;
        [SerializeField] private GameObject _veryFastTimeBorder;
        [Header("ScreenActionButtons")]
        [SerializeField] private Button _openPauseButton;
        [SerializeField] private Button _openDiaryButton;
        [SerializeField] private Button _openInventoryButton;
        [SerializeField] private Button _openQuestsButton;
        [SerializeField] private Button _openTraderButton;
        [SerializeField] private Button _openCompanionsButton;
        [Header("LocalizedStrings")]
        [SerializeField] private LocalizedString _dayTextLocalizedString;
        [SerializeField] private LocalizedString _enterCityLocalizedString;
        [SerializeField] private LocalizedString _campLocalizedString;
        [SerializeField] private LocalizedString _moveLocalizedString;
        [SerializeField] private LocalizedString _fastMoveLocalizedString;
        [SerializeField] private LocalizedString _leaveCityLocalizedString;

        private HudScreenViewModel _viewModel;
        private IDisposable _stateSubscription;
        private IDisposable _resourceSubscription;
        private IDisposable _timeSpeedSubscription;
        private LocalizationService.LocalizedTextGroup _buttonHandles;
        private LocalizationService.LocalizedTextHandle _cityHandle;

        private void OnEnable()
        {
            SubscribeViewModel();
        }

        private void OnDisable()
        {
            UnsubscribeViewModel();
        }

        public override void BindViewModel(IScreenViewModel viewModel)
        {
            _viewModel = viewModel as HudScreenViewModel;
            RegisterToastView();
            if (_minorEventView != null)
                _minorEventView.SetOutcomeFormatter(_viewModel.OutcomeFormatter);
            _viewModel.VisibilityChanged += SetVisible;
            SubscribeViewModel();
        }

        private void RegisterToastView()
        {
            if (_viewModel == null) return;
            IEventToastView toastView = GetComponentInChildren<IEventToastView>(true);
            _viewModel.RegisterToastView(toastView);
        }

        private void SubscribeViewModel()
        {
            if (_viewModel == null || _stateSubscription != null || Localization == null)
                return;

            _stateSubscription = _viewModel.State.Subscribe(ApplyState);
            _resourceSubscription = _viewModel.Resources.Subscribe(ApplyResources);
            _viewModel.InteractableChanged += SetInteractable;
            _viewModel.DayChanged += ApplyDay;

            ApplyDay(_viewModel.CurrentDay);
            SetupIcons();

            _startActionButton.onClick.AddListener(OnStartAction);
            _actionButton.onClick.AddListener(OnAction);
            _endActionButton.onClick.AddListener(OnEndAction);
            _openInventoryButton.onClick.AddListener(OnOpenInventory);
            _openPauseButton.onClick.AddListener(OnOpenPause);
            _openTraderButton.onClick.AddListener(OnOpenTrader);

            _timeSpeedSubscription = _viewModel.TimeSpeedState.Subscribe(ApplyTimeSpeedBorder);
            if (_pauseTimeButton != null) _pauseTimeButton.onClick.AddListener(OnPauseTime);
            if (_normalTimeButton != null) _normalTimeButton.onClick.AddListener(OnNormalTime);
            if (_fastTimeButton != null) _fastTimeButton.onClick.AddListener(OnFastTime);
            if (_veryFastTimeButton != null) _veryFastTimeButton.onClick.AddListener(OnVeryFastTime);
        }

        private void UnsubscribeViewModel()
        {
            if (_viewModel == null)
                return;

            _stateSubscription?.Dispose();
            _stateSubscription = null;
            _resourceSubscription?.Dispose();
            _resourceSubscription = null;
            _timeSpeedSubscription?.Dispose();
            _timeSpeedSubscription = null;

            _viewModel.InteractableChanged -= SetInteractable;
            _viewModel.DayChanged -= ApplyDay;

            _buttonHandles?.Dispose();
            _buttonHandles = null;
            _cityHandle?.Dispose();
            _cityHandle = null;

            _startActionButton.onClick.RemoveListener(OnStartAction);
            _actionButton.onClick.RemoveListener(OnAction);
            _endActionButton.onClick.RemoveListener(OnEndAction);
            _openInventoryButton.onClick.RemoveListener(OnOpenInventory);
            _openPauseButton.onClick.RemoveListener(OnOpenPause);
            _openTraderButton.onClick.RemoveListener(OnOpenTrader);

            if (_pauseTimeButton != null) _pauseTimeButton.onClick.RemoveListener(OnPauseTime);
            if (_normalTimeButton != null) _normalTimeButton.onClick.RemoveListener(OnNormalTime);
            if (_fastTimeButton != null) _fastTimeButton.onClick.RemoveListener(OnFastTime);
            if (_veryFastTimeButton != null) _veryFastTimeButton.onClick.RemoveListener(OnVeryFastTime);
        }

        private void ApplyState(HudViewState state)
        {
            switch (state.Mode)
            {
                case HudMode.Travel:
                    ApplyTravelMode(state.ActiveActionIndex);
                    break;
                case HudMode.City:
                    ApplyCityMode(state.City);
                    break;
            }
        }

        private void ApplyTravelMode(int activeActionIndex)
        {
            _startActionButton.gameObject.SetActive(true);
            _actionButton.gameObject.SetActive(true);
            _endActionButton.gameObject.SetActive(true);
            _cityTextContainer.SetActive(false);

            _buttonHandles?.Dispose();
            _buttonHandles = Localization.CreateTextGroup();
            _buttonHandles.Bind(_startActionButtonText, _campLocalizedString, "Hud.Camp");
            _buttonHandles.Bind(_actionButtonText, _moveLocalizedString, "Hud.Move");
            _buttonHandles.Bind(_endActionButtonText, _fastMoveLocalizedString, "Hud.Rush");

            SetActionBorder(activeActionIndex);
        }

        private void ApplyCityMode(CityData city)
        {
            bool inCity = city != null;

            _startActionButton.gameObject.SetActive(true);
            _actionButton.gameObject.SetActive(false);
            _endActionButton.gameObject.SetActive(!inCity);
            _cityTextContainer.SetActive(inCity);

            _buttonHandles?.Dispose();
            _buttonHandles = Localization.CreateTextGroup();

            if (inCity)
            {
                _buttonHandles.Bind(_startActionButtonText, _leaveCityLocalizedString, "Hud.LeaveCity");
                _cityHandle?.Dispose();
                _cityHandle = Localization.BindText(_cityText, city.Name, "Hud.CityName");
            }
            else
            {
                _buttonHandles.Bind(_startActionButtonText, _enterCityLocalizedString, "Hud.EnterCity");
                _buttonHandles.Bind(_endActionButtonText, _moveLocalizedString, "Hud.Move");
            }

            ClearSpeedBorders();
        }

        private void SetActionBorder(int activeIndex)
        {
            _startActionBorder.SetActive(activeIndex == 0);
            _actionBorder.SetActive(activeIndex == 1);
            _endActionBorder.SetActive(activeIndex == 2);
        }

        private void ClearSpeedBorders()
        {
            _startActionBorder.SetActive(false);
            _actionBorder.SetActive(false);
            _endActionBorder.SetActive(false);
        }

        private void ApplyResources(HudResourceViewState res)
        {
            _foodIndicator.SetValue($"{res.Food.Value:0}");
            if (res.Food.Animate)
                _foodIndicator.ShowTemporaryChange(res.Food.Change, res.Food.IncreaseIsPositive);

            ApplySlider(_playerCaretDurabilitySlider, res.PlayerCart);

            _otherCaretsDurabilitySlider.gameObject.SetActive(res.OtherCarts.Visible);
            if (res.OtherCarts.Visible)
                _otherCaretsDurabilitySlider.SetValue(res.OtherCarts.Value, res.OtherCarts.MaxValue);

            ApplySlider(_dangerSlider, res.Danger);
        }

        private void ApplySlider(SliderResourceIndicator slider, ResourceIndicatorState s)
        {
            if (s.Animate)
            {
                slider.AnimateValue(s.Value, s.MaxValue);
                slider.ShowTemporaryChange(s.Change, s.IncreaseIsPositive);
            }
            else
            {
                slider.SetValue(s.Value, s.MaxValue);
            }
        }

        private void ApplyDay(int day)
        {
            if (_dayText == null) return;
            _dayText.text = LocalizationService.ResolveString(
                _dayTextLocalizedString, $"Day {day}", "Hud.Day", day);
        }

        private void SetupIcons()
        {
            ResourceIconCatalog icons = _viewModel.ResourceIcons;
            if (icons == null) return;

            _foodIndicator.SetIcon(icons.Get(ResourceType.Food)?.Icon);
            _playerCaretDurabilitySlider.SetIcon(icons.Get(ResourceType.PlayerCartDurability)?.Icon);
            _otherCaretsDurabilitySlider.SetIcon(icons.Get(ResourceType.OtherCartsDurability)?.Icon);
            _dangerSlider.SetIcon(icons.Get(ResourceType.Danger)?.Icon);
        }

        private void SetInteractable(bool state)
        {
            _openInventoryButton.interactable = state;
            _openTraderButton.interactable = state;
            _startActionButton.interactable = state;
            _actionButton.interactable = state;
            _endActionButton.interactable = state;
            if (_pauseTimeButton != null) _pauseTimeButton.interactable = state;
            if (_normalTimeButton != null) _normalTimeButton.interactable = state;
            if (_fastTimeButton != null) _fastTimeButton.interactable = state;
            if (_veryFastTimeButton != null) _veryFastTimeButton.interactable = state;
        }

        private void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        private void OnDestroy()
        {
            if (_viewModel != null)
                _viewModel.VisibilityChanged -= SetVisible;
        }

        private void ApplyTimeSpeedBorder(TimeSpeed speed)
        {
            if (_pauseTimeBorder != null) _pauseTimeBorder.SetActive(speed == TimeSpeed.Paused);
            if (_normalTimeBorder != null) _normalTimeBorder.SetActive(speed == TimeSpeed.Normal);
            if (_fastTimeBorder != null) _fastTimeBorder.SetActive(speed == TimeSpeed.Fast);
            if (_veryFastTimeBorder != null) _veryFastTimeBorder.SetActive(speed == TimeSpeed.VeryFast);
        }

        private void OnStartAction() => _viewModel?.OnStartAction();
        private void OnAction() => _viewModel?.OnAction();
        private void OnEndAction() => _viewModel?.OnEndAction();
        private void OnOpenInventory() => _viewModel?.OpenInventory();
        private void OnOpenPause() => _viewModel?.OpenPause();
        private void OnOpenTrader() => _viewModel?.OpenTrader();
        private void OnPauseTime() => _viewModel?.OnTimeSpeedSelected(TimeSpeed.Paused);
        private void OnNormalTime() => _viewModel?.OnTimeSpeedSelected(TimeSpeed.Normal);
        private void OnFastTime() => _viewModel?.OnTimeSpeedSelected(TimeSpeed.Fast);
        private void OnVeryFastTime() => _viewModel?.OnTimeSpeedSelected(TimeSpeed.VeryFast);
    }
}
