using System;
using System.Collections.Generic;
using Internal.Scripts.Caravan;
using Internal.Scripts.Caravan.Generated;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Inventory;
using Internal.Scripts.Items;
using Internal.Scripts.Player;
using Internal.Scripts.Player.Languages;
using Internal.Scripts.Player.Languages.Generated;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.Theme;
using R3;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Internal.Scripts.UI.Screens.Caravan
{
    public sealed class CaravanScreenViewModel : ScreenViewModelBase
    {
        private readonly PlayerResourceRepository _resourceRepo;
        private readonly CaravanDatabase _caravanDb;
        private readonly CompanionService _companionService;
        private readonly CompanionCosts _companionCosts;
        private readonly OverloadCalculator _overloadCalculator;
        private readonly ItemWeightCalculator _weightCalculator;
        private readonly InventoryRepository _inventoryRepo;
        private readonly UiThemeService _themeService;
        private readonly ResourceIconCatalog _resourceIcons;
        private readonly ReactiveProperty<CaravanViewState> _state = new();

        public UiThemeService ThemeService => _themeService;
        public ResourceIconCatalog ResourceIcons => _resourceIcons;

        public CaravanScreenViewModel(
            PlayerResourceRepository resourceRepo,
            CaravanDatabase caravanDb,
            CompanionService companionService,
            CompanionCosts companionCosts,
            OverloadCalculator overloadCalculator,
            ItemWeightCalculator weightCalculator,
            InventoryRepository inventoryRepo,
            UiThemeService themeService,
            ResourceIconCatalog resourceIcons)
        {
            _resourceRepo = resourceRepo;
            _caravanDb = caravanDb;
            _companionService = companionService;
            _companionCosts = companionCosts;
            _overloadCalculator = overloadCalculator;
            _weightCalculator = weightCalculator;
            _inventoryRepo = inventoryRepo;
            _themeService = themeService;
            _resourceIcons = resourceIcons;
        }

        public override ScreenId Id => ScreenId.Caravan;
        public Observable<CaravanViewState> State => _state;

        protected override void OnOpen(object args)
        {
            BuildState();
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        }

        protected override void OnClose()
        {
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        }

        private void OnLocaleChanged(Locale _) => BuildState();

        public void HealCompanion(int index)
        {
            var state = _resourceRepo.Current;
            if (index < 0 || index >= state.Companions.Count)
                return;

            if (!state.Companions[index].IsInjured)
                return;

            _companionService.HealCompanion(index);
            BuildState();
        }

        public void FireCompanion(int index)
        {
            _companionService.RemoveCompanion(index);
            BuildState();
        }

        public void HealAnimal()
        {
            BuildState();
        }

        public void RepairCart(int index)
        {
            BuildState();
        }

        public void DiscardCart(int index)
        {
            _resourceRepo.RemoveCart(index);
            BuildState();
        }

        private void BuildState()
        {
            PlayerResourceState res = _resourceRepo.Current;

            float weight = _weightCalculator.CalculateInventoryWeight(
                _inventoryRepo.GetPlayerInventory());
            float capacity = _overloadCalculator.GetEffectiveCapacity();
            string weightFormatted = $"{(int)weight}/{(int)capacity}";

            string dangerFormatted = $"{(int)res.AccumulatedDanger}";
            string foodFormatted = $"{(int)res.Food}";
            string moraleFormatted = "-";

            bool isAnimalSick = false;
            string healthFormatted = isAnimalSick
                ? ResolveLoc("UI.Caravan.Health.Sick", "UI.Caravan.Health.Sick")
                : ResolveLoc("UI.Caravan.Health.Healthy", "UI.Caravan.Health.Healthy");

            float baseSpeed = res.PlayerCart.Speed;
            CartClassData classData = _caravanDb.GetCartClassById(res.CartClassId);
            if (classData != null)
                baseSpeed = classData.SpeedKmDay;

            var upgradeLevel = _caravanDb.GetUpgradeLevelById(res.CartUpgradeLevelId);
            baseSpeed *= upgradeLevel.SpeedMult;

            DraftAnimalData animalData = _caravanDb.GetDraftAnimalById(res.DraftAnimalId);
            if (animalData != null)
                baseSpeed *= 1f + animalData.SpeedModPct / 100f;

            float extraCartPenalty = 0f;
            if (res.Carts != null)
            {
                foreach (var cart in res.Carts)
                {
                    if (string.IsNullOrEmpty(cart.TypeId)) continue;
                    var extra = _caravanDb.ExtraCarts.Find(e => e.Id == cart.TypeId);
                    if (extra != null)
                        extraCartPenalty += extra.SpeedPenaltyPct;
                }
            }
            baseSpeed *= 1f - extraCartPenalty / 100f;

            string speedFormatted = ResolveLoc("UI.Caravan.Stats.Speed", "UI.Caravan.Stats.Speed", $"{baseSpeed:0.#}");

            int totalDailyCost = 0;
            foreach (var comp in res.Companions)
                totalDailyCost += _companionCosts.CalculateDailyCost(comp);
            string dailyCostFormatted = ResolveLoc("UI.Caravan.Stats.DailyCost", "UI.Caravan.Stats.DailyCost", totalDailyCost);

            var companions = BuildCompanions(res);
            var animal = BuildAnimal(res);
            var carts = BuildCarts(res);

            _state.Value = new CaravanViewState(
                res.Money, weightFormatted,
                moraleFormatted, dangerFormatted,
                foodFormatted, speedFormatted,
                dailyCostFormatted, healthFormatted,
                companions, res.MaxCompanions,
                animal, carts);
        }

        private List<CompanionCardData> BuildCompanions(PlayerResourceState res)
        {
            var list = new List<CompanionCardData>(res.Companions.Count);

            string statusPrefix = ResolveLoc("UI.Global.Status.Prefix", "UI.Global.Status.Prefix");
            string effectPrefix = ResolveLoc("UI.Global.Effect.Prefix", "UI.Global.Effect.Prefix");
            string dailyCostPrefix = ResolveLoc("UI.Global.DailyCost.Prefix", "UI.Global.DailyCost.Prefix");

            for (int i = 0; i < res.Companions.Count; i++)
            {
                CompanionState comp = res.Companions[i];
                CompanionTypeData typeData = _caravanDb.GetCompanionTypeById(comp.TypeId);
                string typeName = typeData != null
                    ? LocalizationService.ResolveString(typeData.Name, typeData.Id, "Caravan.CompanionType")
                    : comp.TypeId;
                string typeAndName = !string.IsNullOrEmpty(comp.Name)
                    ? $"{typeName} {comp.Name}"
                    : typeName;

                string statusValue = comp.IsInjured
                    ? ResolveLoc("UI.Caravan.CompanionStatus.Injured", "UI.Caravan.CompanionStatus.Injured")
                    : ResolveLoc("UI.Caravan.CompanionStatus.Healthy", "UI.Caravan.CompanionStatus.Healthy");
                string statusText = $"{statusPrefix} {statusValue}";

                string effectText = "";
                if (typeData != null && !comp.IsInjured)
                {
                    if (Enum.TryParse(comp.TypeId, true, out CompanionType compType)
                        && Enum.TryParse(comp.QualityId, true, out CompanionQuality compQuality))
                    {
                        var bonus = _caravanDb.GetCompanionBonus(compType, compQuality);
                        if (!string.IsNullOrEmpty(bonus.BonusKey))
                        {
                            if (compType == CompanionType.Translator && !string.IsNullOrEmpty(comp.LanguageId))
                            {
                                Enum.TryParse(comp.LanguageId, true, out LanguageType compLang);
                                string langName = ResolveLanguageName(compLang);
                                int profValue = Mathf.RoundToInt(bonus.BonusValue);
                                string profName = ResolveProficiencyName((LanguageProficiency)profValue);
                                string translatorEffect = ResolveLoc("UI.Translator.LanguageEffect",
                                    "UI.Translator.LanguageEffect", langName, profName);
                                effectText = $"{effectPrefix} {translatorEffect}";
                            }
                            else
                            {
                                string bonusDesc = LocalizationService.ResolveString(
                                    bonus.Description, $"{bonus.BonusKey}: {bonus.BonusValue}", "Caravan.CompanionBonus");
                                effectText = $"{effectPrefix} {bonusDesc}";
                            }
                        }
                    }
                }

                if (string.IsNullOrEmpty(effectText))
                    effectText = $"{effectPrefix} {ResolveLoc("UI.Global.NoEffect", "UI.Global.NoEffect")}";

                int dailyCost = _companionCosts.CalculateDailyCost(comp);
                string dailyCostText = $"{dailyCostPrefix} {ResolveLoc("UI.Caravan.DailyCost", "UI.Caravan.DailyCost", dailyCost)}";

                list.Add(new CompanionCardData(
                    typeAndName, statusText, effectText,
                    dailyCostText, comp.IsInjured, comp.IsInjured, i));
            }

            return list;
        }

        private AnimalViewData BuildAnimal(PlayerResourceState res)
        {
            DraftAnimalData animal = _caravanDb.GetDraftAnimalById(res.DraftAnimalId);
            string animalPrefix = ResolveLoc("UI.Global.Animals.Prefix", "UI.Global.Animals.Prefix");
            string rawName = animal != null
                ? LocalizationService.ResolveString(animal.Name, animal.Id, "Caravan.AnimalName")
                : res.DraftAnimalId;
            string typeName = $"{animalPrefix} {rawName}";

            bool isInjured = false;
            int healCost = 15;

            string statusPrefix = ResolveLoc("UI.Global.Status.Prefix", "UI.Global.Status.Prefix");
            string statusValue = isInjured
                ? ResolveLoc("UI.Caravan.AnimalStatus.Sick", "UI.Caravan.AnimalStatus.Sick", healCost)
                : ResolveLoc("UI.Caravan.AnimalStatus.Healthy", "UI.Caravan.AnimalStatus.Healthy");
            string statusText = $"{statusPrefix} {statusValue}";

            string animalEffect;
            if (animal != null && (animal.SpeedModPct != 0 || animal.CapacityModPct != 0))
            {
                string speedMod = FormatSignedPercent(animal.SpeedModPct);
                string capMod = FormatSignedPercent(animal.CapacityModPct);
                animalEffect = ResolveLoc("UI.Caravan.AnimalEffect.Base",
                    "UI.Caravan.AnimalEffect.Base", speedMod, capMod);
            }
            else
            {
                animalEffect = ResolveLoc("UI.Global.NoEffect", "UI.Global.NoEffect");
            }

            string sickEffect = isInjured
                ? ResolveLoc("UI.Caravan.AnimalEffect.Sick", "UI.Caravan.AnimalEffect.Sick")
                : "";

            string combined = string.IsNullOrEmpty(sickEffect)
                ? animalEffect
                : $"{animalEffect}, {sickEffect}";
            string effectText = ResolveLoc("UI.Global.CurrentEffect", "UI.Global.CurrentEffect", combined);

            return new AnimalViewData(typeName, statusText, effectText, isInjured, isInjured, healCost);
        }

        private List<CartViewData> BuildCarts(PlayerResourceState res)
        {
            var list = new List<CartViewData>(1 + res.Carts.Count);

            string durabilityPrefix = ResolveLoc("UI.Global.Durability.Prefix", "UI.Global.Durability.Prefix");
            string consumptionPrefix = ResolveLoc("UI.Global.Consumption.Prefix", "UI.Global.Consumption.Prefix");
            string paramsPrefix = ResolveLoc("UI.Caravan.Params.Prefix", "UI.Caravan.Params.Prefix");
            string mainCartPrefix = ResolveLoc("UI.Caravan.MainCart.Prefix", "UI.Caravan.MainCart.Prefix");
            string extraCartPrefix = ResolveLoc("UI.Caravan.ExtraCart.Prefix", "UI.Caravan.ExtraCart.Prefix");

            CartClassData classData = _caravanDb.GetCartClassById(res.CartClassId);
            string localizedMainName = classData != null
                ? LocalizationService.ResolveString(classData.Name, classData.Id, "Caravan.CartName")
                : res.CartClassId;
            string mainName = $"{mainCartPrefix} {localizedMainName}";

            string mainEffect = classData != null
                ? $"{paramsPrefix} {ResolveLoc("UI.Caravan.CartEffect", "UI.Caravan.CartEffect", $"{classData.SpeedKmDay:0.#}", $"{classData.Capacity:0.#}")}"
                : "";
            string mainDurability = $"{durabilityPrefix} {(int)res.PlayerCart.Durability}/{(int)res.PlayerCart.MaxDurability}";
            string mainConsumption = $"{consumptionPrefix} {ResolveLoc("UI.Caravan.Consumption", "UI.Caravan.Consumption", $"{res.PlayerCart.FoodConsumptionPerDay:0.#}")}";

            list.Add(new CartViewData(
                mainName, true, mainEffect, mainDurability,
                mainConsumption, false, false, -1));

            for (int i = 0; i < res.Carts.Count; i++)
            {
                CartState cart = res.Carts[i];
                ExtraCartData extraData = !string.IsNullOrEmpty(cart.TypeId)
                    ? _caravanDb.ExtraCarts.Find(e => e.Id == cart.TypeId)
                    : null;

                string localizedExtraName = extraData != null
                    ? LocalizationService.ResolveString(extraData.Name, extraData.Id, "Caravan.ExtraCartName")
                    : (cart.TypeId ?? "");
                string cartName = $"{extraCartPrefix} {localizedExtraName}";

                string effectPrefix = ResolveLoc("UI.Global.Effect.Prefix", "UI.Global.Effect.Prefix");
                string effectValue = extraData != null
                    ? ResolveLoc("UI.Caravan.ExtraCartEffect",
                        "UI.Caravan.ExtraCartEffect",
                        $"{extraData.Capacity:0.#}", $"{extraData.SpeedPenaltyPct}")
                    : "";
                string effect = !string.IsNullOrEmpty(effectValue)
                    ? $"{effectPrefix} {effectValue}"
                    : "";

                string companionInfo = "";
                if (i < res.Companions.Count && !string.IsNullOrEmpty(res.Companions[i].Name))
                {
                    var comp = res.Companions[i];
                    var typeData = _caravanDb.CompanionTypes.Find(t => t.Id == comp.TypeId);
                    string typeName = typeData != null
                        ? LocalizationService.ResolveString(typeData.Name, typeData.Id, "Caravan.CompanionType")
                        : comp.TypeId;
                    string fullName = $"{typeName} {comp.Name}";
                    companionInfo = ResolveLoc("UI.Caravan.BelongsTo", "UI.Caravan.BelongsTo", fullName);
                }

                string effectAndCompanion = !string.IsNullOrEmpty(companionInfo)
                    ? $"{effect}, {companionInfo}"
                    : effect;

                string durability = $"{durabilityPrefix} {(int)cart.Durability}/{(int)cart.MaxDurability}";
                string consumption = $"{consumptionPrefix} {ResolveLoc("UI.Caravan.Consumption", "UI.Caravan.Consumption", $"{cart.FoodConsumptionPerDay:0.#}")}";

                bool canRepair = cart.Durability < cart.MaxDurability;

                list.Add(new CartViewData(
                    cartName, false, effectAndCompanion, durability,
                    consumption, canRepair, true, i));
            }

            return list;
        }

        private static string FormatSignedPercent(float value)
        {
            return value >= 0 ? $"+{value:F0}%" : $"{value:F0}%";
        }

        private static string ResolveLanguageName(LanguageType lang)
        {
            if (lang == LanguageType.None)
                return "";

            var localized = new LocalizedString("UI", $"UI.Language.{lang}.Name");
            return LocalizationService.ResolveString(localized, lang.ToString(), $"LanguageName.{lang}");
        }

        private static string ResolveProficiencyName(LanguageProficiency proficiency)
        {
            var localized = new LocalizedString("UI", $"UI.Language.Proficiency.{proficiency}");
            return LocalizationService.ResolveString(localized, proficiency.ToString(), $"Proficiency.{proficiency}");
        }

        private static string ResolveLoc(string key, string fallback, params object[] args)
        {
            var localized = new LocalizedString("UI", key);
            return LocalizationService.ResolveString(localized, fallback, key, args);
        }
    }
}
