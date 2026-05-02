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
        [Header("QuestTracker")]
        [SerializeField] private QuestTrackerView _questTracker;
        [Header("CameraControls")]
        [SerializeField] private Button _lockCameraButton;
        [SerializeField] private TextMeshProUGUI _lockCameraButtonText;
        [Header("LocalizedStrings")]
        [SerializeField] private LocalizedString _dayTextLocalizedString;
        [SerializeField] private LocalizedString _enterCityLocalizedString;
        [SerializeField] private LocalizedString _campLocalizedString;
        [SerializeField] private LocalizedString _moveLocalizedString;
        [SerializeField] private LocalizedString _fastMoveLocalizedString;
        [SerializeField] private LocalizedString _leaveCityLocalizedString;
        [SerializeField] private LocalizedString _lockCameraLocalizedString;

        private HudScreenViewModel _viewModel;
        private IDisposable _stateSubscription;
        private IDisposable _resourceSubscription;
        private IDisposable _timeSpeedSubscription;
        private IDisposable _trackerSubscription;
        private LocalizationService.LocalizedTextGroup _buttonHandles;
        private LocalizationService.LocalizedTextHandle _cityHandle;
        private LocalizationService.LocalizedTextHandle _lockCameraHandle;
        private LocalizationService.LocalizedTextHandle _dayHandle;

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
            _openQuestsButton.onClick.AddListener(OnOpenQuests);
            _openCompanionsButton.onClick.AddListener(OnOpenCaravan);
            _openDiaryButton.onClick.AddListener(OnOpenDiary);
            _lockCameraButton.onClick.AddListener(OnLockCamera);
            _lockCameraHandle = Localization.BindText(_lockCameraButtonText, _lockCameraLocalizedString, "Hud.LockCamera");

            _timeSpeedSubscription = _viewModel.TimeSpeedState.Subscribe(ApplyTimeSpeedBorder);
            _trackerSubscription = _viewModel.TrackerState.Subscribe(ApplyTrackerState);
            if (_questTracker != null && _questTracker.OpenQuestsButton != null)
                _questTracker.OpenQuestsButton.onClick.AddListener(OnOpenQuests);
            _pauseTimeButton.onClick.AddListener(OnPauseTime);
            _normalTimeButton.onClick.AddListener(OnNormalTime);
            _fastTimeButton.onClick.AddListener(OnFastTime);
            _veryFastTimeButton.onClick.AddListener(OnVeryFastTime);
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
            _trackerSubscription?.Dispose();
            _trackerSubscription = null;

            _viewModel.InteractableChanged -= SetInteractable;
            _viewModel.DayChanged -= ApplyDay;

            _buttonHandles?.Dispose();
            _buttonHandles = null;
            _cityHandle?.Dispose();
            _cityHandle = null;
            _lockCameraHandle?.Dispose();
            _lockCameraHandle = null;

            _startActionButton.onClick.RemoveListener(OnStartAction);
            _actionButton.onClick.RemoveListener(OnAction);
            _endActionButton.onClick.RemoveListener(OnEndAction);
            _openInventoryButton.onClick.RemoveListener(OnOpenInventory);
            _openPauseButton.onClick.RemoveListener(OnOpenPause);
            _openTraderButton.onClick.RemoveListener(OnOpenTrader);
            _openQuestsButton.onClick.RemoveListener(OnOpenQuests);
            _openCompanionsButton.onClick.RemoveListener(OnOpenCaravan);
            _openDiaryButton.onClick.RemoveListener(OnOpenDiary);
            _lockCameraButton.onClick.RemoveListener(OnLockCamera);

            if (_questTracker != null && _questTracker.OpenQuestsButton != null)
                _questTracker.OpenQuestsButton.onClick.RemoveListener(OnOpenQuests);
            _pauseTimeButton.onClick.RemoveListener(OnPauseTime);
            _normalTimeButton.onClick.RemoveListener(OnNormalTime);
            _fastTimeButton.onClick.RemoveListener(OnFastTime);
            _veryFastTimeButton.onClick.RemoveListener(OnVeryFastTime);
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

            if (_lockCameraButton != null)
                _lockCameraButton.gameObject.SetActive(state.ShowLockCameraButton);
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
            ApplyIndicator(_foodIndicator, res.Food);
            ApplyIndicator(_playerCaretDurabilitySlider, res.PlayerCart);
            ApplyIndicator(_otherCaretsDurabilitySlider, res.OtherCarts);
            ApplyIndicator(_dangerSlider, res.Danger);
        }

        private void ApplyIndicator(ResourceIndicator indicator, ResourceIndicatorState s)
        {
            indicator.gameObject.SetActive(s.Visible);
            if (!s.Visible) return;

            if (s.Animate)
                indicator.ApplyAnimated(s.Value, s.MaxValue, s.Change, s.IncreaseIsPositive, ScaledDuration(3f));
            else
                indicator.ApplyImmediate(s.Value, s.MaxValue);
        }

        private float ScaledDuration(float baseDuration)
            => baseDuration / Mathf.Max(_viewModel.TimeScale, 1f);

        private void ApplyDay(int day)
        {
            if (_dayText == null) return;
            _dayHandle?.Dispose();
            if (_dayTextLocalizedString != null && Localization != null)
                _dayHandle = Localization.BindText(_dayText, _dayTextLocalizedString,
                    "Hud.Day", $"Day {day}", null, day);
            else
                _dayText.text = $"Day {day}";
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

        protected override void OnDestroy()
        {
            if (_viewModel != null)
                _viewModel.VisibilityChanged -= SetVisible;

            base.OnDestroy();
        }

        private void ApplyTimeSpeedBorder(TimeSpeed speed)
        {
            if (_pauseTimeBorder != null) _pauseTimeBorder.SetActive(speed == TimeSpeed.Paused);
            if (_normalTimeBorder != null) _normalTimeBorder.SetActive(speed == TimeSpeed.Normal);
            if (_fastTimeBorder != null) _fastTimeBorder.SetActive(speed == TimeSpeed.Fast);
            if (_veryFastTimeBorder != null) _veryFastTimeBorder.SetActive(speed == TimeSpeed.VeryFast);
        }

        private void ApplyTrackerState(QuestTrackerState state)
        {
            if (_questTracker == null) return;

            if (state.Quest == null &&
                state.ChangeType != TrackerChangeType.QuestCompleted &&
                state.ChangeType != TrackerChangeType.QuestFailed)
            {
                _questTracker.Hide();
                return;
            }

            _questTracker.ApplyState(state);
        }

        private void OnStartAction() => _viewModel?.OnStartAction();
        private void OnAction() => _viewModel?.OnAction();
        private void OnEndAction() => _viewModel?.OnEndAction();
        private void OnOpenInventory() => _viewModel?.OpenInventory();
        private void OnOpenPause() => _viewModel?.OpenPause();
        private void OnOpenTrader() => _viewModel?.OpenTrader();
        private void OnOpenQuests() => _viewModel?.OpenQuests();
        private void OnOpenCaravan() => _viewModel?.OpenCaravan();
        private void OnOpenDiary() => _viewModel?.OpenDiary();
        private void OnLockCamera() => _viewModel?.LockCameraToPlayer();
        private void OnPauseTime() => _viewModel?.OnTimeSpeedSelected(TimeSpeed.Paused);
        private void OnNormalTime() => _viewModel?.OnTimeSpeedSelected(TimeSpeed.Normal);
        private void OnFastTime() => _viewModel?.OnTimeSpeedSelected(TimeSpeed.Fast);
        private void OnVeryFastTime() => _viewModel?.OnTimeSpeedSelected(TimeSpeed.VeryFast);
    }
}
