using System;
using System.Collections.Generic;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.CityEntryConfirm
{
    public class CityEntryPreviewView : MonoBehaviour
    {
        [Header("Section Labels")]
        [SerializeField] private TextMeshProUGUI _cityTypeLabel;
        [SerializeField] private TextMeshProUGUI _buildingsLabel;

        [Header("City Info (dynamic)")]
        [SerializeField] private IconLabelView _cityTypeView;
        [SerializeField] private IconLabelView _haveQuestView;

        [Header("Icon+Label Lists")]
        [SerializeField] private IconLabelView _iconLabelPrefab;
        [SerializeField] private RectTransform _buildingsContent;
        [SerializeField] private RectTransform _modifiersContent;

        [Header("Entry Conditions")]
        [SerializeField] private IconLabelView _dutyCountView;
        [SerializeField] private TextMeshProUGUI _dutyDiscountText;
        [SerializeField] private TextMeshProUGUI _hiddenItemsText;
        [SerializeField] private TextMeshProUGUI _detectionPercentText;

        [Header("Buttons")]
        [SerializeField] private Button _enterCityButton;
        [SerializeField] private TextMeshProUGUI _enterCityButtonText;
        [SerializeField] private Button _leaveButton;
        [SerializeField] private TextMeshProUGUI _leaveButtonText;

        [Header("Localization")]
        [SerializeField] private LocalizedString _cityTypeLabelLocalized;
        [SerializeField] private LocalizedString _buildingsLabelLocalized;
        [SerializeField] private LocalizedString _dutyCountLocalized;
        [SerializeField] private LocalizedString _dutyDiscountLocalized;
        [SerializeField] private LocalizedString _hiddenItemsLocalized;
        [SerializeField] private LocalizedString _detectionPercentLocalized;
        [SerializeField] private LocalizedString _enterCityButtonLocalized;
        [SerializeField] private LocalizedString _leaveButtonLocalized;

        private readonly List<IconLabelView> _spawnedBuildings = new();
        private readonly List<IconLabelView> _spawnedModifiers = new();

        public event Action EnterClicked;
        public event Action LeaveClicked;

        private void Awake()
        {
            _enterCityButton.onClick.AddListener(() => EnterClicked?.Invoke());
            _leaveButton.onClick.AddListener(() => LeaveClicked?.Invoke());
        }

        public void Apply(CityEntryConfirmViewState state)
        {
            ApplyCityInfo(state);
            ApplyEntryConditions(state);
            ApplyButtons(state);
        }

        private void ApplyCityInfo(CityEntryConfirmViewState state)
        {
            _cityTypeLabel.text = LocalizationService.ResolveString(
                _cityTypeLabelLocalized, "Тип города:", "CityEntryConfirm.CityTypeLabel");
            _cityTypeView.Initialize(state.CityType.Icon, state.CityType.Label);

            _buildingsLabel.text = LocalizationService.ResolveString(
                _buildingsLabelLocalized, "Здания в городе:", "CityEntryConfirm.BuildingsLabel");
            RebuildIconLabels(_buildingsContent, _spawnedBuildings, state.Buildings);
            RebuildIconLabels(_modifiersContent, _spawnedModifiers, state.Modifiers);
            _haveQuestView.gameObject.SetActive(state.HasQuest);
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

        private void RebuildIconLabels(RectTransform container, List<IconLabelView> pool, IconLabelEntry[] entries)
        {
            foreach (var card in pool)
                Destroy(card.gameObject);
            pool.Clear();

            if (entries == null) return;

            foreach (var entry in entries)
            {
                IconLabelView card = Instantiate(_iconLabelPrefab, container);
                card.Initialize(entry.Icon, entry.Label);
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
