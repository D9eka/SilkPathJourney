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
using Internal.Scripts.UI.Localization.Generated;
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
            int foodCount = InventoryStateMutator.GetItemCount(
                _inventoryRepo.GetPlayerInventory(), SuppliesItemId.Value);
            string foodFormatted = $"{foodCount}";
            string moraleFormatted = $"{(int)res.Morale}";

            bool isAnimalSick = false;
            string healthFormatted = isAnimalSick
                ? LocalizationService.Resolve(LocUI.Table, LocUI.UI_Caravan_Health_Sick)
                : LocalizationService.Resolve(LocUI.Table, LocUI.UI_Caravan_Health_Healthy);

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

            string speedFormatted = LocalizationService.Resolve(LocUI.Table, LocUI.UI_Caravan_Stats_Speed, null, $"{baseSpeed:0.#}");

            int totalDailyCost = 0;
            foreach (var comp in res.Companions)
                totalDailyCost += _companionCosts.CalculateDailyCost(comp);
            string dailyCostFormatted = LocalizationService.Resolve(LocUI.Table, LocUI.UI_Caravan_Stats_DailyCost, null, totalDailyCost);

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

            string statusPrefix = LocalizationService.Resolve(LocUI.Table, LocUI.UI_Global_Status_Prefix);
            string effectPrefix = LocalizationService.Resolve(LocUI.Table, LocUI.UI_Global_Effect_Prefix);
            string dailyCostPrefix = LocalizationService.Resolve(LocUI.Table, LocUI.UI_Global_DailyCost_Prefix);

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
                    ? LocalizationService.Resolve(LocUI.Table, LocUI.UI_Caravan_CompanionStatus_Injured)
                    : LocalizationService.Resolve(LocUI.Table, LocUI.UI_Caravan_CompanionStatus_Healthy);
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
                                string translatorEffect = LocalizationService.Resolve(LocUI.Table, LocUI.UI_Translator_LanguageEffect, null, langName, profName);
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
                    effectText = $"{effectPrefix} {LocalizationService.Resolve(LocUI.Table, LocUI.UI_Global_NoEffect)}";

                int dailyCost = _companionCosts.CalculateDailyCost(comp);
                string dailyCostText = $"{dailyCostPrefix} {LocalizationService.Resolve(LocUI.Table, LocUI.UI_Caravan_DailyCost, null, dailyCost)}";

                list.Add(new CompanionCardData(
                    typeAndName, statusText, effectText,
                    dailyCostText, comp.IsInjured, comp.IsInjured, i));
            }

            return list;
        }

        private AnimalViewData BuildAnimal(PlayerResourceState res)
        {
            DraftAnimalData animal = _caravanDb.GetDraftAnimalById(res.DraftAnimalId);
            string animalPrefix = LocalizationService.Resolve(LocUI.Table, LocUI.UI_Global_Animals_Prefix);
            string rawName = animal != null
                ? LocalizationService.ResolveString(animal.Name, animal.Id, "Caravan.AnimalName")
                : res.DraftAnimalId;
            string typeName = $"{animalPrefix} {rawName}";

            bool isInjured = false;
            int healCost = 15;

            string statusPrefix = LocalizationService.Resolve(LocUI.Table, LocUI.UI_Global_Status_Prefix);
            string statusValue = isInjured
                ? LocalizationService.Resolve(LocUI.Table, LocUI.UI_Caravan_AnimalStatus_Sick, null, healCost)
                : LocalizationService.Resolve(LocUI.Table, LocUI.UI_Caravan_AnimalStatus_Healthy);
            string statusText = $"{statusPrefix} {statusValue}";

            string animalEffect;
            if (animal != null && (animal.SpeedModPct != 0 || animal.CapacityModPct != 0))
            {
                string speedMod = FormatSignedPercent(animal.SpeedModPct);
                string capMod = FormatSignedPercent(animal.CapacityModPct);
                animalEffect = LocalizationService.Resolve(LocUI.Table, LocUI.UI_Caravan_AnimalEffect_Base, null, speedMod, capMod);
            }
            else
            {
                animalEffect = LocalizationService.Resolve(LocUI.Table, LocUI.UI_Global_NoEffect);
            }

            string sickEffect = isInjured
                ? LocalizationService.Resolve(LocUI.Table, LocUI.UI_Caravan_AnimalEffect_Sick)
                : "";

            string combined = string.IsNullOrEmpty(sickEffect)
                ? animalEffect
                : $"{animalEffect}, {sickEffect}";
            string effectText = LocalizationService.Resolve(LocUI.Table, LocUI.UI_Global_CurrentEffect, null, combined);

            return new AnimalViewData(typeName, statusText, effectText, isInjured, isInjured, healCost);
        }

        private List<CartViewData> BuildCarts(PlayerResourceState res)
        {
            var list = new List<CartViewData>(1 + res.Carts.Count);

            string durabilityPrefix = LocalizationService.Resolve(LocUI.Table, LocUI.UI_Global_Durability_Prefix);
            string consumptionPrefix = LocalizationService.Resolve(LocUI.Table, LocUI.UI_Global_Consumption_Prefix);
            string paramsPrefix = LocalizationService.Resolve(LocUI.Table, LocUI.UI_Caravan_Params_Prefix);
            string mainCartPrefix = LocalizationService.Resolve(LocUI.Table, LocUI.UI_Caravan_MainCart_Prefix);
            string extraCartPrefix = LocalizationService.Resolve(LocUI.Table, LocUI.UI_Caravan_ExtraCart_Prefix);

            CartClassData classData = _caravanDb.GetCartClassById(res.CartClassId);
            string localizedMainName = classData != null
                ? LocalizationService.ResolveString(classData.Name, classData.Id, "Caravan.CartName")
                : res.CartClassId;
            string mainName = $"{mainCartPrefix} {localizedMainName}";

            string mainEffect = classData != null
                ? $"{paramsPrefix} {LocalizationService.Resolve(LocUI.Table, LocUI.UI_Caravan_CartEffect, null, $"{classData.SpeedKmDay:0.#}", $"{classData.Capacity:0.#}")}"
                : "";
            string mainDurability = $"{durabilityPrefix} {(int)res.PlayerCart.Durability}/{(int)res.PlayerCart.MaxDurability}";
            string mainConsumption = $"{consumptionPrefix} {LocalizationService.Resolve(LocUI.Table, LocUI.UI_Caravan_Consumption, null, $"{res.PlayerCart.FoodConsumptionPerDay:0.#}")}";

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

                string effectPrefix = LocalizationService.Resolve(LocUI.Table, LocUI.UI_Global_Effect_Prefix);
                string effectValue = extraData != null
                    ? LocalizationService.Resolve(LocUI.Table, LocUI.UI_Caravan_ExtraCartEffect, null,
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
                    string companionTypeName = typeData != null
                        ? LocalizationService.ResolveString(typeData.Name, typeData.Id, "Caravan.CompanionType")
                        : comp.TypeId;
                    string fullName = $"{companionTypeName} {comp.Name}";
                    companionInfo = LocalizationService.Resolve(LocUI.Table, LocUI.UI_Caravan_BelongsTo, null, fullName);
                }

                string effectAndCompanion = !string.IsNullOrEmpty(companionInfo)
                    ? $"{effect}, {companionInfo}"
                    : effect;

                string durability = $"{durabilityPrefix} {(int)cart.Durability}/{(int)cart.MaxDurability}";
                string consumption = $"{consumptionPrefix} {LocalizationService.Resolve(LocUI.Table, LocUI.UI_Caravan_Consumption, null, $"{cart.FoodConsumptionPerDay:0.#}")}";

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
    }
}
