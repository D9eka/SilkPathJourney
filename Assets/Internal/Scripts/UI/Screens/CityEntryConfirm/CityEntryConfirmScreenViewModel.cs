using System.Collections.Generic;
using Internal.Scripts.Caravan.Generated;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Cities.Smuggling;
using Internal.Scripts.Economy.Cities.Tariff;
using Internal.Scripts.Economy.Guild;
using Internal.Scripts.Economy.Save.Models;
using Internal.Scripts.Inventory;
using Internal.Scripts.Player;
using Internal.Scripts.Quests;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.StackService;
using Internal.Scripts.UI.Tooltip;
using Internal.Scripts.UI.WorldLabel;
using Internal.Scripts.WorldModifiers;
using R3;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Internal.Scripts.UI.Screens.CityEntryConfirm
{
    public sealed class CityEntryConfirmScreenViewModel : ScreenViewModelBase
    {
        private readonly TariffService _tariffService;
        private readonly SmugglingCheckService _smugglingService;
        private readonly ICityEntryService _cityEntryService;
        private readonly ScreenStackService _screenStackService;
        private readonly PlayerResourceRepository _playerResources;
        private readonly InventoryRepository _inventoryRepository;
        private readonly GuildService _guildService;
        private readonly GuildSettings _guildSettings;
        private readonly CaravanUpgradeService _upgradeService;
        private readonly EconomyDatabase _database;
        private readonly WorldModifierRepository _modifierRepo;
        private readonly ResourceIconCatalog _resourceIcons;
        private readonly TooltipService _tooltipService;
        private readonly QuestCityIndicatorService _questIndicator;
        private readonly BuildingFilterCatalog _buildingFilterCatalog;
        private readonly QuestIndicatorIcons _questIcons;

        public ResourceIconCatalog ResourceIcons => _resourceIcons;
        public TooltipService TooltipService => _tooltipService;

        private readonly ReactiveProperty<CityEntryConfirmViewState> _state = new();
        private CityData _city;

        public Observable<CityEntryConfirmViewState> State => _state;

        public CityEntryConfirmScreenViewModel(
            TariffService tariffService,
            SmugglingCheckService smugglingService,
            ICityEntryService cityEntryService,
            ScreenStackService screenStackService,
            PlayerResourceRepository playerResources,
            InventoryRepository inventoryRepository,
            GuildService guildService,
            GuildSettings guildSettings,
            CaravanUpgradeService upgradeService,
            EconomyDatabase database,
            WorldModifierRepository modifierRepo,
            ResourceIconCatalog resourceIcons,
            TooltipService tooltipService,
            QuestCityIndicatorService questIndicator,
            BuildingFilterCatalog buildingFilterCatalog,
            QuestIndicatorIcons questIcons)
        {
            _tariffService = tariffService;
            _smugglingService = smugglingService;
            _cityEntryService = cityEntryService;
            _screenStackService = screenStackService;
            _playerResources = playerResources;
            _inventoryRepository = inventoryRepository;
            _guildService = guildService;
            _guildSettings = guildSettings;
            _upgradeService = upgradeService;
            _database = database;
            _modifierRepo = modifierRepo;
            _resourceIcons = resourceIcons;
            _tooltipService = tooltipService;
            _questIndicator = questIndicator;
            _buildingFilterCatalog = buildingFilterCatalog;
            _questIcons = questIcons;
        }

        public override ScreenId Id => ScreenId.CityEntryConfirm;

        protected override void OnOpen(object args)
        {
            _city = args as CityData;
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
            BuildPreviewState();

            if (ShouldSkipConfirmation())
                ProceedToCity();
        }

        private bool ShouldSkipConfirmation()
        {
            var state = _state.Value;
            return state != null
                && state.TariffAmount == 0
                && !state.HasSmugglingRisk;
        }

        protected override void OnClose()
        {
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
            _state.Value = null;
        }

        private void OnLocaleChanged(UnityEngine.Localization.Locale _) => BuildPreviewState();

        public void ConfirmEntry()
        {
            int moneyBefore = _playerResources.Current.Money;

            var tariffResult = _tariffService.ChargePlayerTariff(_city);
            var smugglingResult = _smugglingService.PerformCheck(_city);

            int moneyAfter = _playerResources.Current.Money;

            _state.Value = new CityEntryConfirmViewState
            {
                CityName = ResolveCityName(),
                Phase = CityEntryPhase.Result,
                CityType = ResolveCityType(),
                Buildings = ResolveBuildings(),
                Modifiers = ResolveModifiers(),
                QuestIndicatorIcon = _questIndicator?.GetIndicatorIcon(_city.Id, _questIcons),
                QuestIndicatorText = _questIndicator?.GetIndicatorText(_city.Id),
                IsGuildMember = _guildService.IsMember,
                GuildDiscountPct = _guildSettings.MemberTariffDiscount,
                PlayerMoney = moneyAfter,
                PlayerMoneyBefore = moneyBefore,
                MoneyIcon = _resourceIcons?.Get(ResourceType.Money)?.Icon,
                TariffResult = tariffResult,
                SmugglingResult = smugglingResult
            };
        }

        public void AcknowledgeResult()
        {
            ProceedToCity();
        }

        public void Cancel()
        {
            _screenStackService.CloseTop();
        }

        private void ProceedToCity()
        {
            _screenStackService.CloseTop();
            if (_cityEntryService.CanEnterCity(_city))
                _cityEntryService.EnterCity(_city);
            else
                _screenStackService.TryOpen(ScreenId.EnterCity, _city.Id, out _);
        }

        private void BuildPreviewState()
        {
            if (_city == null)
                return;

            var inventory = _inventoryRepository.GetPlayerInventory();
            int reputation = _playerResources.Current.Reputation;
            int tariff = _tariffService.EstimateTariffWithGuild(inventory, _city, reputation);

            bool hasCompartment = _upgradeService.HasUpgrade(CaravanUpgradeType.HiddenCompartment);
            InventoryState compartment = hasCompartment ? _inventoryRepository.GetHiddenCompartment() : null;
            int hiddenCount = compartment?.Items?.Count ?? 0;
            bool hasRisk = hiddenCount > 0;
            float chance = hasRisk ? _smugglingService.CalculateDetectionChance() : 0f;

            int money = _playerResources.Current.Money;

            _state.Value = new CityEntryConfirmViewState
            {
                CityName = ResolveCityName(),
                Phase = CityEntryPhase.Preview,
                CityType = ResolveCityType(),
                Buildings = ResolveBuildings(),
                Modifiers = ResolveModifiers(),
                QuestIndicatorIcon = _questIndicator?.GetIndicatorIcon(_city.Id, _questIcons),
                QuestIndicatorText = _questIndicator?.GetIndicatorText(_city.Id),
                TariffAmount = tariff,
                IsGuildMember = _guildService.IsMember,
                GuildDiscountPct = _guildSettings.MemberTariffDiscount,
                PlayerMoney = money,
                MoneyIcon = _resourceIcons?.Get(ResourceType.Money)?.Icon,
                CanAfford = money >= tariff,
                HasSmugglingRisk = hasRisk,
                DetectionChance = chance,
                HiddenItemCount = hiddenCount
            };
        }

        private IconLabelEntry ResolveCityType()
        {
            var data = _database.GetCityType(_city.Type);
            if (data == null)
                return new IconLabelEntry(null, _city.Type.ToString());
            string name = LocalizationService.ResolveString(data.Name, _city.Type.ToString(), "CityEntry.CityType");
            return new IconLabelEntry(data.Icon, name, data);
        }

        private IconLabelEntry[] ResolveBuildings()
        {
            if (_city.Buildings == null || _city.Buildings.Length == 0)
                return System.Array.Empty<IconLabelEntry>();

            var list = new List<IconLabelEntry>(_city.Buildings.Length);
            foreach (var buildingId in _city.Buildings)
            {
                var building = _database.GetBuilding(buildingId);
                if (building == null) continue;
                string name = LocalizationService.ResolveString(building.Name, building.Type.ToString(), "CityEntry.Building");
                Sprite icon = _buildingFilterCatalog?.Get(buildingId)?.Icon;
                list.Add(new IconLabelEntry(icon, name));
            }
            return list.ToArray();
        }

        private IconLabelEntry[] ResolveModifiers()
        {
            var entries = _modifierRepo.GetCityModifiers(_city.Id);
            if (entries == null || entries.Count == 0)
                return System.Array.Empty<IconLabelEntry>();

            var result = new List<IconLabelEntry>(entries.Count);
            foreach (var entry in entries)
            {
                var data = _database.GetCityModifier(entry.ModifierId);
                if (data == null) continue;
                string name = LocalizationService.ResolveString(data.Name, data.Id, "CityEntry.Modifier");
                result.Add(new IconLabelEntry(data.Icon, name, data));
            }
            return result.ToArray();
        }

        private string ResolveCityName() =>
            LocalizationService.ResolveString(_city.Name, _city.Id, "CityEntry");
    }
}
