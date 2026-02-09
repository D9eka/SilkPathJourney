using System;
using Internal.Scripts.Config;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Events;
using Internal.Scripts.Hud;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screen.View;
using Internal.Scripts.UI.Screen.ViewModel;
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
        [Header("ScreenActionButtons")]
        [SerializeField] private Button _openDiaryButton;
        [SerializeField] private Button _openInventoryButton;
        [SerializeField] private Button _openQuestsButton;
        [SerializeField] private Button _openPerksButton;
        [SerializeField] private Button _openCompanionsButton;
        [Header("LocalizedStrings")]
        [SerializeField] private LocalizedString _dayTextLocalizedString;
        [SerializeField] private LocalizedString _enterCityLocalizedString;
        [SerializeField] private LocalizedString _campLocalizedString;
        [SerializeField] private LocalizedString _moveLocalizedString;
        [SerializeField] private LocalizedString _fastMoveLocalizedString;

        private HudScreenViewModel _viewModel;
        private IDisposable _stateSubscription;
        private IDisposable _resourceSubscription;

        private float _prevFood = -1f;
        private float _prevDanger = -1f;
        private float _prevPlayerCartDur = -1f;

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
            if (_viewModel == null || _stateSubscription != null)
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
        }

        private void UnsubscribeViewModel()
        {
            if (_viewModel == null)
                return;

            _stateSubscription?.Dispose();
            _stateSubscription = null;
            _resourceSubscription?.Dispose();
            _resourceSubscription = null;

            _viewModel.InteractableChanged -= SetInteractable;
            _viewModel.DayChanged -= ApplyDay;

            _startActionButton.onClick.RemoveListener(OnStartAction);
            _actionButton.onClick.RemoveListener(OnAction);
            _endActionButton.onClick.RemoveListener(OnEndAction);
            _openInventoryButton.onClick.RemoveListener(OnOpenInventory);
        }

        private void ApplyState(HudViewState state)
        {
            switch (state.Mode)
            {
                case HudMode.Travel:
                    ApplyTravelMode(state.ActiveSpeedIndex);
                    break;
                case HudMode.CityStrategic:
                    ApplyCityStrategicMode();
                    break;
                case HudMode.CityDetailed:
                    ApplyCityDetailedMode(state.City);
                    break;
            }
        }

        private void ApplyTravelMode(int activeSpeedIndex)
        {
            _startActionButton.gameObject.SetActive(true);
            _actionButton.gameObject.SetActive(true);
            _endActionButton.gameObject.SetActive(true);
            _cityTextContainer.SetActive(false);

            SetButtonText(_startActionButtonText, _campLocalizedString, "Camp");
            SetButtonText(_actionButtonText, _moveLocalizedString, "Move");
            SetButtonText(_endActionButtonText, _fastMoveLocalizedString, "Rush");

            SetSpeedBorder(activeSpeedIndex);
        }

        private void ApplyCityStrategicMode()
        {
            _startActionButton.gameObject.SetActive(true);
            _actionButton.gameObject.SetActive(false);
            _endActionButton.gameObject.SetActive(true);
            _cityTextContainer.SetActive(false);

            SetButtonText(_startActionButtonText, _enterCityLocalizedString, "EnterCity");
            SetButtonText(_endActionButtonText, _moveLocalizedString, "Move");

            ClearSpeedBorders();
        }

        private void ApplyCityDetailedMode(CityData city)
        {
            _startActionButton.gameObject.SetActive(true);
            _actionButton.gameObject.SetActive(false);
            _endActionButton.gameObject.SetActive(false);
            _cityTextContainer.SetActive(true);

            SetButtonText(_startActionButtonText, _moveLocalizedString, "Move");

            if (city != null)
                _cityText.text = LocalizationHelper.ResolveString(city.Name, city.Id, "CityName");
            else
                _cityText.text = "";

            ClearSpeedBorders();
        }

        private void SetSpeedBorder(int activeIndex)
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

        private void SetButtonText(TextMeshProUGUI textField, LocalizedString localizedString, string context)
        {
            if (textField != null)
                textField.text = LocalizationHelper.ResolveString(localizedString,
                    localizedString.TableEntryReference.Key, context);
        }

        private void ApplyResources(PlayerResourceState res)
        {
            GameBalanceConfig config = _viewModel.BalanceConfig;

            float food = res.Food;
            _foodIndicator.SetValue($"{food:0}");
            if (_prevFood >= 0f && !Mathf.Approximately(food, _prevFood))
                _foodIndicator.ShowTemporaryChange(Mathf.RoundToInt(food - _prevFood), 
                    GetIncreaseIsPositive(ResourceType.Food));
            _prevFood = food;

            float dur = res.PlayerCart.Durability;
            float maxDur = res.PlayerCart.MaxDurability;
            if (_prevPlayerCartDur >= 0f && !Mathf.Approximately(dur, _prevPlayerCartDur))
            {
                _playerCaretDurabilitySlider.AnimateValue(dur, maxDur);
                _playerCaretDurabilitySlider.ShowTemporaryChange(
                    Mathf.RoundToInt(dur - _prevPlayerCartDur), 
                    GetIncreaseIsPositive(ResourceType.PlayerCartDurability));
            }
            else
            {
                _playerCaretDurabilitySlider.SetValue(dur, maxDur);
            }
            _prevPlayerCartDur = dur;

            if (res.Carts.Count == 0)
            {
                _otherCaretsDurabilitySlider.gameObject.SetActive(false);
            }
            else
            {
                float totalDur = 0f;
                float totalMax = 0f;
                foreach (CartState c in res.Carts)
                {
                    totalDur += c.Durability;
                    totalMax += c.MaxDurability;
                }
                _otherCaretsDurabilitySlider.gameObject.SetActive(true);
                _otherCaretsDurabilitySlider.SetValue(totalDur / res.Carts.Count, totalMax / res.Carts.Count);
            }

            float danger = res.AccumulatedDanger;
            if (_prevDanger >= 0f && !Mathf.Approximately(danger, _prevDanger))
            {
                _dangerSlider.AnimateValue(danger, config.MaxDanger);
                _dangerSlider.ShowTemporaryChange(Mathf.RoundToInt(danger - _prevDanger), GetIncreaseIsPositive(ResourceType.Danger));
            }
            else
            {
                _dangerSlider.SetValue(danger, config.MaxDanger);
            }
            _prevDanger = danger;
        }

        private void ApplyDay(int day)
        {
            string label = LocalizationHelper.ResolveString(
                _dayTextLocalizedString, _dayTextLocalizedString.TableEntryReference.Key, "Day");
            _dayText.text = $"{label} {day}";
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

        private bool GetIncreaseIsPositive(ResourceType type) =>
            _viewModel?.ResourceIcons?.Get(type)?.IncreaseIsPositive ?? true;

        private void SetInteractable(bool state)
        {
            _openInventoryButton.interactable = state;
            _startActionButton.interactable = state;
            _actionButton.interactable = state;
            _endActionButton.interactable = state;
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

        private void OnStartAction() => _viewModel?.OnStartAction();
        private void OnAction() => _viewModel?.OnAction();
        private void OnEndAction() => _viewModel?.OnEndAction();
        private void OnOpenInventory() => _viewModel?.OpenInventory();
    }
}
