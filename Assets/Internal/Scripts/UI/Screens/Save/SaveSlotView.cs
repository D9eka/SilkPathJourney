using System;
using Internal.Scripts.Save;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.Save
{
    public class SaveSlotView : MonoBehaviour, IPointerClickHandler
    {
        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI _timeText;
        [SerializeField] private TextMeshProUGUI _dayText;
        [SerializeField] private TextMeshProUGUI _cityText;

        [Header("Resources")]
        [SerializeField] private ResourceIndicator _playerCartDurabilityIndicator;
        [SerializeField] private ResourceIndicator _otherCartsDurabilityIndicator;
        [SerializeField] private ResourceIndicator _foodIndicator;
        [SerializeField] private ResourceIndicator _dangerIndicator;
        [SerializeField] private ResourceIndicator _partnersIndicator;
        [SerializeField] private ResourceIndicator _moneyIndicator;

        [Header("Selection")]
        [SerializeField] private GameObject _selectionBorder;

        [Header("Buttons")]
        [SerializeField] private Button _deleteButton;

        [Header("Localization")]
        [SerializeField] private LocalizedString _autoSaveTag;
        [SerializeField] private LocalizedString _manualSaveTag;
        [SerializeField] private LocalizedString _dayTextLocalizedString;

        private SaveLoadScreenBase _owner;
        private int _index;

        public SaveMetadata Metadata { get; private set; }

        public void Bind(SaveLoadScreenBase owner, int index)
        {
            _owner = owner;
            _index = index;
        }

        public void SetData(SaveMetadata metadata, string cityName, ResourceIconCatalog icons)
        {
            Metadata = metadata;

            if (_timeText != null)
            {
                var localizedTag = metadata.IsAutoSave ? _autoSaveTag : _manualSaveTag;
                string tag = LocalizationService.ResolveString(
                    localizedTag, metadata.IsAutoSave ? "А" : "Р", "SaveSlot.Tag");
                var time = DateTimeOffset.FromUnixTimeSeconds(metadata.LastSavedTimestamp).LocalDateTime;
                _timeText.text = $"{tag}: {time:dd.MM.yyyy HH:mm:ss}";
            }

            if (_dayText != null)
                _dayText.text = LocalizationService.ResolveString(
                    _dayTextLocalizedString, $"День {metadata.CurrentDay}", "SaveSlot.DayText",
                    metadata.CurrentDay);

            if (_cityText != null)
                _cityText.text = cityName;

            ApplyIndicator(_playerCartDurabilityIndicator, icons?.Get(ResourceType.PlayerCartDurability)?.Icon,
                metadata.CartDurability, metadata.CartDurabilityMax);
            ApplyIndicator(_otherCartsDurabilityIndicator, icons?.Get(ResourceType.OtherCartsDurability)?.Icon,
                metadata.OtherCartsDurability, metadata.OtherCartsDurabilityMax);
            ApplyIndicator(_foodIndicator, icons?.Get(ResourceType.Food)?.Icon,
                metadata.Food, metadata.FoodMax);
            ApplyIndicator(_dangerIndicator, icons?.Get(ResourceType.Danger)?.Icon,
                metadata.Danger, metadata.DangerMax);
            ApplyIndicator(_partnersIndicator, icons?.Get(ResourceType.Partners)?.Icon,
                metadata.PartnerCount, 0);
            ApplyIndicator(_moneyIndicator, icons?.Get(ResourceType.Money)?.Icon,
                metadata.Money, 0);

            bool hasPartners = metadata.PartnerCount > 0;
            if (_otherCartsDurabilityIndicator != null)
                _otherCartsDurabilityIndicator.gameObject.SetActive(hasPartners);
            if (_partnersIndicator != null)
                _partnersIndicator.gameObject.SetActive(hasPartners);
        }

        private static void ApplyIndicator(ResourceIndicator indicator, Sprite icon, int value, int maxValue)
        {
            if (indicator == null) return;
            indicator.SetIcon(icon);
            if (indicator is SliderResourceIndicator slider && maxValue > 0)
                slider.SetValue(value, maxValue);
            else
                indicator.SetValue(value);
            indicator.HideChange();
        }

        public void SetSelectionState(bool selected)
        {
            if (_selectionBorder != null)
                _selectionBorder.SetActive(selected);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData == null || eventData.button != PointerEventData.InputButton.Left)
                return;

            bool isDoubleClick = eventData.clickCount >= 2;
            _owner?.HandleSlotClick(_index, isDoubleClick);
        }

        private void OnEnable()
        {
            if (_deleteButton != null)
                _deleteButton.onClick.AddListener(OnDeleteClicked);
        }

        private void OnDisable()
        {
            if (_deleteButton != null)
                _deleteButton.onClick.RemoveListener(OnDeleteClicked);
        }

        private void OnDeleteClicked()
        {
            _owner?.HandleSlotDelete(_index);
        }
    }
}
