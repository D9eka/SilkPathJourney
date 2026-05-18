using System;
using System.Collections.Generic;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Items;
using Internal.Scripts.Player.Background;
using Internal.Scripts.Player.Languages;
using Internal.Scripts.Player.Skills;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Theme;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.Screens.NewGame
{
    public class BackgroundDetailView : MonoBehaviour
    {
        [Header("General")]
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _featuresText;

        [Header("Skills")]
        [SerializeField] private TextMeshProUGUI _skillsTitleText;
        [SerializeField] private LocalizedString _skillsTitleString;
        [SerializeField] private Transform _skillsContent;
        [SerializeField] private LabeledBarView _skillItemPrefab;

        [Header("Languages")]
        [SerializeField] private TextMeshProUGUI _languagesTitleText;
        [SerializeField] private LocalizedString _languagesTitleString;
        [SerializeField] private Transform _languagesContent;
        [SerializeField] private LabeledBarView _languageItemPrefab;

        [Header("Starting City")]
        [SerializeField] private TextMeshProUGUI _startingCityHeaderText;
        [SerializeField] private LocalizedString _startingCityHeaderString;
        [SerializeField] private IconLabel _startingCityIconLabel;

        [Header("Resources & Items")]
        [SerializeField] private TextMeshProUGUI _itemsTitleText;
        [SerializeField] private LocalizedString _itemsTitleString;
        [SerializeField] private Transform _resourcesContent;
        [SerializeField] private IconLabel _iconLabelPrefab;
        [SerializeField] private TextMeshProUGUI _itemTextPrefab;

        private readonly List<LabeledBarView> _spawnedSkillItems = new();
        private readonly List<LabeledBarView> _spawnedLanguageItems = new();
        private readonly List<GameObject> _spawnedResourceItems = new();

        private LocalizationService.LocalizedTextHandle _skillsTitleHandle;
        private LocalizationService.LocalizedTextHandle _languagesTitleHandle;
        private LocalizationService.LocalizedTextHandle _itemsTitleHandle;
        private LocalizationService.LocalizedTextHandle _startingCityHeaderHandle;
        private LocalizationService.LocalizedTextHandle _nameHandle;
        private LocalizationService.LocalizedTextHandle _descriptionHandle;
        private LocalizationService.LocalizedTextHandle _featuresHandle;

        private UiThemeService _themeService;
        private LocalizationService _localization;
        private ItemCatalog _itemCatalog;
        private EconomyDatabase _economyDatabase;

        public void Initialize(UiThemeService themeService, LocalizationService localization,
            ItemCatalog itemCatalog, EconomyDatabase economyDatabase)
        {
            _themeService = themeService;
            _localization = localization;
            _itemCatalog = itemCatalog;
            _economyDatabase = economyDatabase;

            BindHeaders();
        }

        public void SetData(BackgroundData data)
        {
            RebindNameDescFeatures(data);

            RefreshStartingCity(data);
            RebuildSkills(data);
            RebuildLanguages(data);
            RebuildResources(data);
        }

        private void RefreshStartingCity(BackgroundData data)
        {
            CityData city = ResolveCity(data.StartingCityId);
            string cityName = city != null
                ? LocalizationService.ResolveString(city.Name, data.StartingCityId, $"city.{data.StartingCityId}.name")
                : string.Empty;
            Sprite icon = (city != null && _economyDatabase != null)
                ? _economyDatabase.GetCityType(city.Type)?.Icon
                : null;

            _startingCityIconLabel.Initialize(icon, cityName);
        }

        private CityData ResolveCity(string cityId)
        {
            if (_economyDatabase == null || string.IsNullOrEmpty(cityId))
                return null;
            foreach (CityData c in _economyDatabase.Cities)
                if (c != null && string.Equals(c.Id, cityId, StringComparison.OrdinalIgnoreCase))
                    return c;
            return null;
        }

        private void DisposeDataHandles()
        {
            _nameHandle?.Dispose();
            _descriptionHandle?.Dispose();
            _featuresHandle?.Dispose();
            _nameHandle = null;
            _descriptionHandle = null;
            _featuresHandle = null;
        }

        private void RebindNameDescFeatures(BackgroundData data)
        {
            DisposeDataHandles();

            if (_localization == null) return;

            _nameHandle = _localization.BindText(_nameText, data.Name, $"Background.{data.Id}.Name");
            _descriptionHandle = _localization.BindText(_descriptionText, data.Description, $"Background.{data.Id}.Description");

            if (string.IsNullOrEmpty(data.FeaturesKey))
                _featuresText.text = string.Empty;
            else
                _featuresHandle = _localization.BindText(_featuresText, "Player", data.FeaturesKey, $"Background.{data.Id}.Features");
        }

        private void BindHeaders()
        {
            DisposeHeaderHandles();
            if (_localization == null) return;

            _skillsTitleHandle = _localization.BindText(_skillsTitleText, _skillsTitleString, $"{name}.SkillsTitle");
            _languagesTitleHandle = _localization.BindText(_languagesTitleText, _languagesTitleString, $"{name}.LanguagesTitle");
            _itemsTitleHandle = _localization.BindText(_itemsTitleText, _itemsTitleString, $"{name}.ItemsTitle");
            _startingCityHeaderHandle = _localization.BindText(_startingCityHeaderText, _startingCityHeaderString, $"{name}.StartingCityHeader");
        }

        private void DisposeHeaderHandles()
        {
            _skillsTitleHandle?.Dispose();
            _languagesTitleHandle?.Dispose();
            _itemsTitleHandle?.Dispose();
            _startingCityHeaderHandle?.Dispose();
            _skillsTitleHandle = null;
            _languagesTitleHandle = null;
            _itemsTitleHandle = null;
            _startingCityHeaderHandle = null;
        }

        private void RebuildSkills(BackgroundData data)
        {
            ClearSkills();

            foreach (SkillType skillType in Enum.GetValues(typeof(SkillType)))
            {
                if (skillType == SkillType.None) continue;

                LabeledBarView instance = Instantiate(_skillItemPrefab, _skillsContent);
                instance.gameObject.InitializeColorBinders(themeService: _themeService);

                LocalizedString skillName = SkillView.ResolveSkillName(skillType);
                int value = data.GetSkillValue(skillType);
                float progress = (float)value / BackgroundData.MaxSkillValue;
                instance.Initialize(_localization, skillName, null, progress, $"{value}/{BackgroundData.MaxSkillValue}");

                _spawnedSkillItems.Add(instance);
            }
        }

        private void RebuildLanguages(BackgroundData data)
        {
            ClearLanguages();

            if (data.StartingLanguages == null) return;

            foreach (LanguageStartEntry entry in data.StartingLanguages)
            {
                LabeledBarView instance = Instantiate(_languageItemPrefab, _languagesContent);
                instance.gameObject.InitializeColorBinders(themeService: _themeService);

                LocalizedString languageName = SkillView.ResolveLanguageName(entry.LanguageType);
                LanguageProficiency proficiency = (LanguageProficiency)entry.Level;
                LocalizedString level = SkillView.ResolveProficiencyName(proficiency);
                float progress = SkillView.ProgressFor(proficiency);
                instance.Initialize(_localization, languageName, null, progress, level);

                _spawnedLanguageItems.Add(instance);
            }
        }

        private void RebuildResources(BackgroundData data)
        {
            ClearResources();

            SpawnIconLabel(ResourceType.Money, data.StartingMoney.ToString());
            SpawnIconLabel(ResourceType.Food, data.StartingSupplies.ToString());

            if (data.StartingItems == null) return;

            foreach (ItemStartEntry entry in data.StartingItems)
            {
                string itemName = _itemCatalog != null
                    ? _itemCatalog.ResolveItemName(entry.ItemId)
                    : entry.ItemId;
                SpawnText($"{entry.Count}× {itemName}");
            }
        }

        private void SpawnIconLabel(ResourceType type, string label)
        {
            IconLabel instance = Instantiate(_iconLabelPrefab, _resourcesContent);
            instance.gameObject.InitializeColorBinders(themeService: _themeService);

            IconView iconView = instance.GetComponentInChildren<IconView>();
            if (iconView != null)
                iconView.SetResourceType(type);

            instance.SetLabel(label);
            _spawnedResourceItems.Add(instance.gameObject);
        }

        private void SpawnText(string text)
        {
            TextMeshProUGUI instance = Instantiate(_itemTextPrefab, _resourcesContent);
            instance.gameObject.InitializeColorBinders(themeService: _themeService);
            instance.text = text;
            _spawnedResourceItems.Add(instance.gameObject);
        }

        private void ClearSkills()
        {
            foreach (LabeledBarView item in _spawnedSkillItems)
                Destroy(item.gameObject);
            _spawnedSkillItems.Clear();
        }

        private void ClearLanguages()
        {
            foreach (LabeledBarView item in _spawnedLanguageItems)
                Destroy(item.gameObject);
            _spawnedLanguageItems.Clear();
        }

        private void ClearResources()
        {
            foreach (GameObject item in _spawnedResourceItems)
                Destroy(item);
            _spawnedResourceItems.Clear();
        }

        private void OnDisable()
        {
            ClearSkills();
            ClearLanguages();
            ClearResources();
            DisposeHeaderHandles();
            DisposeDataHandles();
        }
    }
}
