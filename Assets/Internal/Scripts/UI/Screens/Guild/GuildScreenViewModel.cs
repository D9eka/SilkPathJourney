using System;
using System.Collections.Generic;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Buildings;
using Internal.Scripts.Economy.Guild;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Events;
using Internal.Scripts.Inventory;
using Internal.Scripts.Items;
using Internal.Scripts.Quests;
using Internal.Scripts.Save;
using Internal.Scripts.Trading;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Localization.Generated;
using Internal.Scripts.UI.Screens.Building;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.Screens.Shared;
using Internal.Scripts.UI.StackService;
using Internal.Scripts.UI.Theme;
using R3;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Internal.Scripts.UI.Screens.Guild
{
    public sealed class GuildScreenViewModel : ScreenViewModelBase
    {
        private readonly GuildService _guildService;
        private readonly GuildSettings _guildSettings;
        private readonly EconomyDatabase _economyDatabase;
        private readonly PlayerResourceRepository _resourceRepository;
        private readonly DayTracker _dayTracker;
        private readonly SaveRepository _saveRepository;
        private readonly UiThemeService _themeService;
        private readonly ItemCatalog _itemCatalog;
        private readonly ItemWeightCalculator _weightCalculator;
        private readonly InventoryRepository _inventoryRepository;
        private readonly ScreenStackService _screenStackService;
        private readonly ResourceIconCatalog _resourceIcons;
        private readonly BuildingQuestSlotViewModel _questSlotVM;
        private readonly ReactiveProperty<GuildViewState> _state = new();
        private readonly List<GuildOfferingAction> _offeringActions = new();

        private string _cityId;
        private List<GuildContract> _availableContracts = new();

        public Observable<GuildViewState> State => _state;
        public Observable<BuildingQuestSlotState?> QuestSlot => _questSlotVM.State;
        public UiThemeService ThemeService => _themeService;
        public ResourceIconCatalog ResourceIcons => _resourceIcons;
        public override ScreenId Id => ScreenId.Guild;

        public GuildScreenViewModel(
            GuildService guildService,
            GuildSettings guildSettings,
            EconomyDatabase economyDatabase,
            PlayerResourceRepository resourceRepository,
            DayTracker dayTracker,
            SaveRepository saveRepository,
            UiThemeService themeService,
            ItemCatalog itemCatalog,
            ItemWeightCalculator weightCalculator,
            InventoryRepository inventoryRepository,
            ScreenStackService screenStackService,
            ResourceIconCatalog resourceIcons,
            BuildingQuestSlotViewModel questSlotVM)
        {
            _guildService = guildService;
            _guildSettings = guildSettings;
            _economyDatabase = economyDatabase;
            _resourceRepository = resourceRepository;
            _dayTracker = dayTracker;
            _saveRepository = saveRepository;
            _themeService = themeService;
            _itemCatalog = itemCatalog;
            _weightCalculator = weightCalculator;
            _inventoryRepository = inventoryRepository;
            _screenStackService = screenStackService;
            _resourceIcons = resourceIcons;
            _questSlotVM = questSlotVM;
        }

        public void OnQuestSlotTalk() => _questSlotVM.OnTalk();

        protected override void OnOpen(object args)
        {
            _cityId = args as string;
            _questSlotVM.Bind(BuildingType.Guild, _cityId);
            BuildState();
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        }

        protected override void OnClose()
        {
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
            _questSlotVM.Dispose();
            _state.Value = null;
        }

        private void OnLocaleChanged(Locale _) => BuildState();

        public void Join()
        {
            _guildService.Join(_cityId);
            BuildState();
        }

        public void TakeCredit()
        {
            _guildService.TakeCredit(_cityId);
            BuildState();
        }

        public void RepayCredit()
        {
            _guildService.RepayCredit(_cityId);
            BuildState();
        }

        public void AcceptContract(int index)
        {
            if (index < 0 || index >= _availableContracts.Count)
                return;

            _guildService.AcceptContract(_availableContracts[index]);
            BuildState();
        }

        public void TryCompleteContract()
        {
            _guildService.TryCompleteContract(_cityId);
            BuildState();
        }

        public void TriggerOffering(int index)
        {
            if (index < 0 || index >= _offeringActions.Count)
                return;

            switch (_offeringActions[index])
            {
                case GuildOfferingAction.Join: Join(); break;
                case GuildOfferingAction.TakeCredit: TakeCredit(); break;
                case GuildOfferingAction.RepayCredit: RepayCredit(); break;
                case GuildOfferingAction.OpenTrade: OpenGuildTrade(); break;
            }
        }

        public void OpenGuildTrade()
        {
            _screenStackService.TryOpen(ScreenId.Trade, new GuildTradeArgs(_cityId), out _);
        }

        private void BuildState()
        {
            bool isMember = _guildService.IsMember;
            int money = _resourceRepository.Current.Money;
            GuildSaveState guildSave = _saveRepository.Data.Economy.Guild;

            float speed = _resourceRepository.Current.PlayerCart.Speed;

            _availableContracts = isMember
                ? _guildService.GetAvailableContracts(_cityId, speed)
                : new List<GuildContract>();

            GuildContractEntry[] contractEntries = BuildContractEntries(_availableContracts);
            GuildContractEntry activeEntry = default;
            if (isMember && guildSave.HasActiveContract)
                activeEntry = BuildContractEntry(guildSave.ActiveContract);

            var playerInventory = _inventoryRepository.GetPlayerInventory();
            float baseWeight = _weightCalculator.CalculateInventoryWeight(playerInventory);
            float maxWeight = _resourceRepository.Current.TotalCapacity;

            _state.Value = new GuildViewState
            {
                IsMember = isMember,
                PlayerMoney = money,
                MoneyIcon = _resourceIcons?.Get(ResourceType.Money)?.Icon,
                BaseWeight = baseWeight,
                MaxWeight = maxWeight,
                WeightIcon = _resourceIcons?.Get(ResourceType.Weight)?.Icon,
                TariffDiscountPct = _guildSettings.MemberTariffDiscount,
                CurrentDebt = guildSave.CreditAmount,
                Offerings = BuildOfferings(isMember, guildSave),
                AvailableContracts = contractEntries,
                HasActiveContract = guildSave.HasActiveContract,
                ActiveContract = activeEntry
            };
        }

        private OfferingItem[] BuildOfferings(bool isMember, GuildSaveState guildSave)
        {
            _offeringActions.Clear();
            var list = new List<OfferingItem>();

            var cityInv = _inventoryRepository.GetCityInventory(_cityId);
            bool guildHasGoods = cityInv?.GuildInventory?.Items != null
                              && cityInv.GuildInventory.Items.Count > 0;

            string tradeDescription = guildHasGoods
                ? LocalizationService.Resolve(LocUI.Table, LocUI.UI_Guild_Trade_Description)
                : LocalizationService.Resolve(LocUI.Table, LocUI.UI_Guild_Trade_Description_Empty);

            list.Add(new OfferingItem(
                LocalizationService.Resolve(LocUI.Table, LocUI.UI_Guild_Trade_Title),
                tradeDescription,
                LocalizationService.Resolve(LocUI.Table, LocUI.UI_Guild_Trade_Button),
                guildHasGoods,
                _offeringActions.Count));
            _offeringActions.Add(GuildOfferingAction.OpenTrade);

            if (!isMember)
            {
                list.Add(new OfferingItem(
                    LocalizationService.Resolve(LocUI.Table, LocUI.UI_Guild_Join_Title),
                    $"{_guildSettings.JoinCost} · -{Mathf.RoundToInt(_guildSettings.MemberTariffDiscount * 100)}% пошлина",
                    LocalizationService.Resolve(LocUI.Table, LocUI.UI_Guild_Join_Button),
                    _guildService.CanJoin(_cityId),
                    _offeringActions.Count));
                _offeringActions.Add(GuildOfferingAction.Join);
            }
            else
            {
                list.Add(new OfferingItem(
                    LocalizationService.Resolve(LocUI.Table, LocUI.UI_Guild_TakeCredit_Title),
                    $"{_guildSettings.PlayerCreditAmount} → вернуть {_guildSettings.PlayerCreditRepayment}",
                    LocalizationService.Resolve(LocUI.Table, LocUI.UI_Guild_TakeCredit_Button),
                    _guildService.CanTakeCredit(_cityId),
                    _offeringActions.Count));
                _offeringActions.Add(GuildOfferingAction.TakeCredit);

                if (guildSave.CreditAmount > 0)
                {
                    list.Add(new OfferingItem(
                        LocalizationService.Resolve(LocUI.Table, LocUI.UI_Guild_RepayCredit_Title),
                        $"{guildSave.CreditAmount}",
                        LocalizationService.Resolve(LocUI.Table, LocUI.UI_Guild_RepayCredit_Button),
                        _guildService.CanRepayCredit(_cityId),
                        _offeringActions.Count));
                    _offeringActions.Add(GuildOfferingAction.RepayCredit);
                }
            }

            return list.ToArray();
        }

        private GuildContractEntry[] BuildContractEntries(List<GuildContract> contracts)
        {
            var entries = new GuildContractEntry[contracts.Count];
            for (int i = 0; i < contracts.Count; i++)
                entries[i] = BuildContractEntry(contracts[i]);
            return entries;
        }

        private GuildContractEntry BuildContractEntry(GuildContract contract)
        {
            string cityName = ResolveCityName(contract.TargetCityId);
            int daysLeft = contract.ExpirationDay - _dayTracker.CurrentDay;

            string cargoName = null;
            float cargoWeight = 0f;
            if (contract.ContractType == GuildContractType.Cargo && !string.IsNullOrEmpty(contract.CargoItemId))
            {
                cargoName = _itemCatalog.ResolveItemName(contract.CargoItemId);
                cargoWeight = _itemCatalog.GetItemWeight(contract.CargoItemId) * contract.CargoAmount;
            }

            return new GuildContractEntry(
                contract.Id, cityName, contract.RewardMoney, daysLeft,
                contract.ContractType, cargoName, contract.CargoAmount, cargoWeight);
        }

        private string ResolveCityName(string cityId)
        {
            var city = _economyDatabase.Cities.Find(c =>
                string.Equals(c.Id, cityId, StringComparison.OrdinalIgnoreCase));

            if (city == null)
                return cityId;

            return LocalizationService.ResolveString(city.Name, city.Id, "Guild.CityName");
        }
    }
}
