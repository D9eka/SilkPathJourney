using System;
using System.Collections.Generic;
using Internal.Scripts.Config;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Cities.UI;
using Internal.Scripts.Events;
using Internal.Scripts.Quests;
using Internal.Scripts.UI.Theme;
using Internal.Scripts.UI.WorldLabel.Components;
using Internal.Scripts.World.State;
using Internal.Scripts.WorldModifiers;
using R3;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Internal.Scripts.UI.WorldLabel
{
    public class CityLabelSpawner : WorldLabelSpawnerBase
    {
        private readonly CityViewSpawner _cityViewSpawner;
        private readonly EconomyDatabase _economyDatabase;
        private readonly StaticColorController _colorController;
        private readonly WorldModifierRepository _modifierRepo;
        private readonly DayTracker _dayTracker;
        private readonly GameBalanceConfig _balanceConfig;
        private readonly QuestCityIndicatorService _questIndicator;
        private readonly QuestRepository _questRepo;
        private readonly QuestIndicatorIcons _questIcons;
        private readonly Dictionary<string, CityLabelView> _cityLabels = new();
        private IDisposable _modifierSubscription;
        private IDisposable _questSubscription;

        public CityLabelSpawner(
            CityViewSpawner cityViewSpawner,
            WorldStateController worldStateController,
            WorldCanvas worldCanvas,
            EconomyDatabase economyDatabase,
            StaticColorController colorController,
            WorldModifierRepository modifierRepo,
            DayTracker dayTracker,
            GameBalanceConfig balanceConfig,
            QuestCityIndicatorService questIndicator,
            QuestRepository questRepo,
            QuestIndicatorIcons questIcons)
            : base(worldStateController, worldCanvas)
        {
            _cityViewSpawner = cityViewSpawner;
            _economyDatabase = economyDatabase;
            _colorController = colorController;
            _modifierRepo = modifierRepo;
            _dayTracker = dayTracker;
            _balanceConfig = balanceConfig;
            _questIndicator = questIndicator;
            _questRepo = questRepo;
            _questIcons = questIcons;
        }

        protected override bool ShouldShowInViewMode(WorldViewMode viewMode) => true;

        protected override void OnDispose()
        {
            _modifierSubscription?.Dispose();
            _questSubscription?.Dispose();
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        }

        protected override void SpawnLabels()
        {
            int total = 0;

            foreach (CityView view in _cityViewSpawner.Views)
            {
                if (view.City == null) continue;
                total++;

                CityData city = view.City;
                UnityEngine.Transform nodeTransform = view.transform.parent;
                UnityEngine.Vector3 position = nodeTransform != null ? nodeTransform.position : view.transform.position;

                CityLabelView label = CreateAndConfigureLabel(
                    position,
                    $"CityLabel_{city.Id}");
                label._nameLabel.SetColorController(_colorController, city.Biome);
                label._nameLabel.SetLocalizedText(city.Name, city.Id);
                label._nameLabel.SetTooltipProvider(city);

                CityTypeData cityType = _economyDatabase.GetCityType(city.Type);
                if (cityType != null)
                {
                    if (cityType.Icon != null)
                        label._nameLabel.SetIcon(cityType.Icon);
                    label._nameLabel.SetIconTooltip(cityType.GetTooltipTitle(), cityType.GetTooltipDescription());
                }

                _cityLabels[city.Id] = label;
                bool hasModifiers = AddActiveModifiers(label.Modifiers, city.Id);
                bool hasQuest = AddQuestIndicator(label.Modifiers, city.Id);
                if (!hasModifiers && !hasQuest)
                    label.Modifiers?.SetVisible(false);
            }

            _modifierSubscription = _modifierRepo.Changed.Subscribe(_ => RefreshAllModifiers());
            _questSubscription = _questRepo.Changed.Subscribe(_ => RefreshAllModifiers());
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;

            UnityEngine.Debug.Log($"[CityLabelSpawner] City labels created: {total}");
        }

        private bool AddActiveModifiers(ModifierIconsView modifiers, string cityId)
        {
            if (modifiers == null) return false;

            var active = _modifierRepo.GetCityModifiers(cityId);
            bool added = false;
            foreach (var entry in active)
            {
                CityModifierData mod = _economyDatabase.GetCityModifier(entry.ModifierId);
                if (mod == null) continue;

                var staleness = ModifierStalenessHelper.GetStaleness(entry.LastSeenDay, _dayTracker.CurrentDay,
                    _balanceConfig.StalenessActualDays, _balanceConfig.StalenessStaleDays);
                float alpha = ModifierStalenessHelper.GetAlpha(staleness);
                if (staleness == ModifierStaleness.Unknown)
                    modifiers.AddIcon(mod.Icon, "???", ModifierStalenessHelper.GetUnknownDescription());
                else
                    modifiers.AddIcon(mod.Icon, mod.GetTooltipTitle(), mod.GetTooltipDescription(), alpha);
                added = true;
            }
            return added;
        }

        private void OnLocaleChanged(Locale _) => RefreshAllModifiers();

        private void RefreshAllModifiers()
        {
            foreach (var kvp in _cityLabels)
            {
                kvp.Value.Modifiers?.ClearIcons();
                bool hasModifiers = AddActiveModifiers(kvp.Value.Modifiers, kvp.Key);
                bool hasQuest = AddQuestIndicator(kvp.Value.Modifiers, kvp.Key);
                if (!hasModifiers && !hasQuest)
                    kvp.Value.Modifiers?.SetVisible(false);
            }
        }

        private bool AddQuestIndicator(ModifierIconsView modifiers, string cityId)
        {
            if (modifiers == null) return false;

            var ind = _questIndicator.GetIndicator(cityId);
            if (ind == QuestCityIndicator.None) return false;

            Sprite icon = ind == QuestCityIndicator.NewAvailable ? _questIcons.NewAvailable : _questIcons.ActiveStage;
            string title = _questIcons.GetTooltipTitle(ind);
            string desc = _questIcons.GetTooltipDescription(ind);
            modifiers.AddIcon(icon, title, desc);
            modifiers.SetVisible(true);
            return true;
        }
    }
}
