using System;
using System.Collections.Generic;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.TargetSelection.Search;
using Internal.Scripts.UI.Tooltip;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.CityEntryConfirm
{
    public class CityEntryPreviewView : MonoBehaviour
    {
        [Header("City Info")]
        [SerializeField] private CityInfoView _cityInfo;

        [Header("Modifiers")]
        [SerializeField] private TextMeshProUGUI _modifiersLabel;
        [SerializeField] private LocalizedString _modifiersLabelLocalized;
        [SerializeField] private RectTransform _modifiersContent;
        [SerializeField] private IconLabel _iconLabelPrefab;

        [Header("Entry Conditions")]
        [SerializeField] private IconLabel _dutyCountView;
        [SerializeField] private TextMeshProUGUI _dutyDiscountText;
        [SerializeField] private TextMeshProUGUI _hiddenItemsText;
        [SerializeField] private TextMeshProUGUI _detectionPercentText;

        [Header("Buttons")]
        [SerializeField] private Button _enterCityButton;
        [SerializeField] private TextMeshProUGUI _enterCityButtonText;
        [SerializeField] private Button _leaveButton;
        [SerializeField] private TextMeshProUGUI _leaveButtonText;

        [Header("Localization")]
        [SerializeField] private LocalizedString _dutyCountLocalized;
        [SerializeField] private LocalizedString _dutyDiscountLocalized;
        [SerializeField] private LocalizedString _hiddenItemsLocalized;
        [SerializeField] private LocalizedString _detectionPercentLocalized;
        [SerializeField] private LocalizedString _enterCityButtonLocalized;
        [SerializeField] private LocalizedString _leaveButtonLocalized;

        private readonly List<IconLabel> _spawnedModifiers = new();

        private TooltipService _tooltipService;

        public event Action EnterClicked;
        public event Action LeaveClicked;

        private void Awake()
        {
            _enterCityButton.onClick.AddListener(() => EnterClicked?.Invoke());
            _leaveButton.onClick.AddListener(() => LeaveClicked?.Invoke());
        }

        public void Initialize(TooltipService tooltipService)
        {
            _tooltipService = tooltipService;
            if (_cityInfo != null)
                _cityInfo.SetTooltipService(tooltipService);
        }

        public void Apply(CityEntryConfirmViewState state)
        {
            ApplyCityInfo(state);
            ApplyModifiers(state);
            ApplyEntryConditions(state);
            ApplyButtons(state);
        }

        private void ApplyCityInfo(CityEntryConfirmViewState state)
        {
            if (_cityInfo != null)
                _cityInfo.Apply(state.CityType.Icon, state.CityType.Label, state.CityType.TooltipProvider,
                    state.Buildings, state.QuestIndicatorIcon, state.QuestIndicatorText);
        }

        private void ApplyModifiers(CityEntryConfirmViewState state)
        {
            bool hasModifiers = state.Modifiers != null && state.Modifiers.Length > 0;
            if (_modifiersLabel != null)
            {
                _modifiersLabel.gameObject.SetActive(hasModifiers);
                if (hasModifiers && _modifiersLabelLocalized != null)
                    _modifiersLabel.text = LocalizationService.ResolveString(
                        _modifiersLabelLocalized, "Модификаторы:", "CityEntryConfirm.ModifiersLabel");
            }
            RebuildIconLabels(_modifiersContent, _spawnedModifiers, state.Modifiers);
        }

        private void ApplyEntryConditions(CityEntryConfirmViewState state)
        {
            _dutyCountView.Initialize(state.MoneyIcon, LocalizationService.ResolveString(
                _dutyCountLocalized, $"Для входа вам нужно заплатить пошлину: {state.TariffAmount}",
                "CityEntryConfirm.Duty", state.TariffAmount));

            ApplyGuildDiscount(state);
            ApplySmugglingRisk(state);
        }

        private void ApplyGuildDiscount(CityEntryConfirmViewState state)
        {
            _dutyDiscountText.gameObject.SetActive(state.IsGuildMember);
            if (!state.IsGuildMember) return;

            int pct = Mathf.RoundToInt(state.GuildDiscountPct * 100);
            _dutyDiscountText.text = LocalizationService.ResolveString(
                _dutyDiscountLocalized, $"Скидка гильдии: {pct}%",
                "CityEntryConfirm.GuildDiscount", pct);
        }

        private void ApplySmugglingRisk(CityEntryConfirmViewState state)
        {
            _hiddenItemsText.gameObject.SetActive(state.HasSmugglingRisk);
            _detectionPercentText.gameObject.SetActive(state.HasSmugglingRisk);
            if (!state.HasSmugglingRisk) return;

            _hiddenItemsText.text = LocalizationService.ResolveString(
                _hiddenItemsLocalized, $"В скрытом отсеке: {state.HiddenItemCount} предм.",
                "CityEntryConfirm.HiddenItems", state.HiddenItemCount);

            int pct = Mathf.RoundToInt(state.DetectionChance * 100);
            _detectionPercentText.text = LocalizationService.ResolveString(
                _detectionPercentLocalized, $"Шанс обнаружения: {pct}%",
                "CityEntryConfirm.DetectionChance", pct);
        }

        private void ApplyButtons(CityEntryConfirmViewState state)
        {
            _enterCityButtonText.text = LocalizationService.ResolveString(
                _enterCityButtonLocalized, $"Войти за {state.TariffAmount}",
                "CityEntryConfirm.EnterButton", state.TariffAmount);
            _enterCityButton.interactable = state.CanAfford;

            _leaveButtonText.text = LocalizationService.ResolveString(
                _leaveButtonLocalized, "Уйти", "CityEntryConfirm.LeaveButton");
        }

        private void RebuildIconLabels(RectTransform container, List<IconLabel> pool, IconLabelEntry[] entries)
        {
            foreach (var card in pool)
                Destroy(card.gameObject);
            pool.Clear();

            if (entries == null) return;

            foreach (var entry in entries)
            {
                IconLabel card = Instantiate(_iconLabelPrefab, container);
                card.Initialize(entry.Icon, entry.Label);

                if (entry.TooltipProvider != null && _tooltipService != null)
                {
                    var provider = entry.TooltipProvider;
                    var hover = HoverReporter.GetOrAdd(card.gameObject);
                    hover.Entered += () => _tooltipService.ShowTooltipDelayed(provider);
                    hover.Exited += () => _tooltipService.HideTooltip();
                }

                pool.Add(card);
            }
        }

        private void OnDestroy()
        {
            _enterCityButton.onClick.RemoveAllListeners();
            _leaveButton.onClick.RemoveAllListeners();
        }
    }
}
