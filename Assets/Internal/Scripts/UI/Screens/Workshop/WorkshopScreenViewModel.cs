using System.Collections.Generic;
using System.Linq;
using Internal.Scripts.Caravan;
using Internal.Scripts.Caravan.Generated;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Buildings;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Events;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Player;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Localization.Generated;
using Internal.Scripts.UI.Screens.Building;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.Screens.Shared;
using Internal.Scripts.UI.Theme;
using R3;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Internal.Scripts.UI.Screens.Workshop
{
    public sealed class WorkshopScreenViewModel : ScreenViewModelBase
    {
        private const int MAX_EXTRA_CART_SLOTS = 3;

        private readonly PlayerResourceRepository _resourceRepository;
        private readonly CaravanDatabase _caravanDatabase;
        private readonly CaravanUpgradeService _upgradeService;
        private readonly EventTrigger _eventTrigger;
        private readonly EventDatabase _eventDb;
        private readonly UiThemeService _themeService;
        private readonly ResourceIconCatalog _resourceIcons;
        private readonly BuildingQuestSlotViewModel _questSlotVM;
        private readonly ReactiveProperty<WorkshopViewState> _state = new();

        private string _cityId;

        public UiThemeService ThemeService => _themeService;
        public ResourceIconCatalog ResourceIcons => _resourceIcons;
        public Observable<WorkshopViewState> State => _state;
        public Observable<BuildingQuestSlotState?> QuestSlot => _questSlotVM.State;
        public override ScreenId Id => ScreenId.Workshop;

        public WorkshopScreenViewModel(
            PlayerResourceRepository resourceRepository,
            CaravanDatabase caravanDatabase,
            CaravanUpgradeService upgradeService,
            EventTrigger eventTrigger,
            EventDatabase eventDb,
            UiThemeService themeService,
            ResourceIconCatalog resourceIcons,
            BuildingQuestSlotViewModel questSlotVM)
        {
            _resourceRepository = resourceRepository;
            _caravanDatabase = caravanDatabase;
            _upgradeService = upgradeService;
            _eventTrigger = eventTrigger;
            _eventDb = eventDb;
            _themeService = themeService;
            _resourceIcons = resourceIcons;
            _questSlotVM = questSlotVM;
        }

        public void OnQuestSlotTalk() => _questSlotVM.OnTalk();

        protected override void OnOpen(object args)
        {
            _cityId = args as string;
            _questSlotVM.Bind(BuildingType.Workshop, _cityId);
            BuildState();
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        }

        protected override void OnClose()
        {
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
            _questSlotVM.Dispose();
        }

        private void OnLocaleChanged(Locale _) => BuildState();

        public void UpgradeMainCart()
        {
            var resources = _resourceRepository.Current;
            var nextLevel = FindNextUpgradeLevel(resources.CartUpgradeLevelId);
            if (nextLevel == null)
                return;

            if (resources.Money < nextLevel.Value.Price)
                return;

            string nextLevelId = nextLevel.Value.Level.ToString();
            _resourceRepository.UpdateResources(s =>
            {
                s.Money -= nextLevel.Value.Price;
                s.CartUpgradeLevelId = nextLevelId;

                var cartClass = _caravanDatabase.GetCartClassById(s.CartClassId);
                if (cartClass != null)
                {
                    s.PlayerCart.Capacity = cartClass.Capacity * nextLevel.Value.CapacityMult;
                    s.PlayerCart.MaxDurability = cartClass.Durability * nextLevel.Value.DurabilityMult;
                    if (s.PlayerCart.Durability > s.PlayerCart.MaxDurability)
                        s.PlayerCart.Durability = s.PlayerCart.MaxDurability;
                }
            });

            BuildState();
        }

        public void BuyExtraCart(ExtraCartType type)
        {
            var resources = _resourceRepository.Current;
            int currentCount = resources.Carts?.Count ?? 0;
            if (currentCount >= MAX_EXTRA_CART_SLOTS)
                return;

            var extraCartData = _caravanDatabase.GetExtraCart(type);
            if (extraCartData == null)
                return;

            if (resources.Money < extraCartData.Price)
                return;

            _resourceRepository.UpdateResources(s =>
            {
                s.Money -= extraCartData.Price;
                s.Carts.Add(new CartState
                {
                    TypeId = extraCartData.Id,
                    Capacity = extraCartData.Capacity,
                    Durability = extraCartData.Durability,
                    MaxDurability = extraCartData.Durability,
                    Speed = extraCartData.SpeedPenaltyPct,
                    AnimalCount = extraCartData.SuppliesPerDay
                });
            });

            BuildState();
        }

        public void SellExtraCart(ExtraCartType type)
        {
            var resources = _resourceRepository.Current;
            if (resources.Carts == null || resources.Carts.Count == 0)
                return;

            var extraCartData = _caravanDatabase.GetExtraCart(type);
            if (extraCartData == null)
                return;

            int index = resources.Carts.FindIndex(c => c.TypeId == extraCartData.Id);
            if (index < 0)
                return;

            _resourceRepository.UpdateResources(s =>
            {
                s.Money += extraCartData.SellPrice;
                s.Carts.RemoveAt(index);
            });

            BuildState();
        }

        public void BuyUpgrade(CaravanUpgradeType type)
        {
            if (_upgradeService.PurchaseUpgrade(type))
                BuildState();
        }

        public string ResolveUpgradeLabel(int cost)
        {
            return LocalizationService.Resolve(LocUI.Table, LocUI.UI_Workshop_Upgrade, null, cost);
        }

        public string ResolveMaxUpgradeLabel()
        {
            return LocalizationService.Resolve(LocUI.Table, LocUI.UI_Workshop_MaxUpgrade);
        }

        public string ResolveBuyLabel(int price)
        {
            return LocalizationService.Resolve(LocUI.Table, LocUI.UI_Workshop_Buy, null, price);
        }

        public string ResolveSellLabel(int price)
        {
            return LocalizationService.Resolve(LocUI.Table, LocUI.UI_Workshop_Sell, null, price);
        }

        private void BuildState()
        {
            var resources = _resourceRepository.Current;
            int money = resources.Money;
            string weightFormatted = $"{resources.TotalCapacity:F0}";

            float baseSpeed = resources.PlayerCart.Speed;
            var classData = _caravanDatabase.GetCartClassById(resources.CartClassId);
            if (classData != null) baseSpeed = classData.SpeedKmDay;
            var upgradeLevel = _caravanDatabase.GetUpgradeLevelById(resources.CartUpgradeLevelId);
            baseSpeed *= upgradeLevel.SpeedMult;
            var animal = _caravanDatabase.GetDraftAnimalById(resources.DraftAnimalId);
            if (animal != null) baseSpeed *= 1f + animal.SpeedModPct / 100f;
            float extraPenalty = 0f;
            if (resources.Carts != null)
                foreach (var c in resources.Carts)
                {
                    if (string.IsNullOrEmpty(c.TypeId)) continue;
                    var ex = _caravanDatabase.ExtraCarts.Find(e => e.Id == c.TypeId);
                    if (ex != null) extraPenalty += ex.SpeedPenaltyPct;
                }
            baseSpeed *= 1f - extraPenalty / 100f;
            string speedFormatted = $"{baseSpeed:0.#}";

            var mainCart = BuildMainCartData(resources, money);
            var extraCarts = BuildExtraCartList(resources, money);
            var upgrades = BuildUpgradeList(resources, money);

            int currentCount = resources.Carts?.Count ?? 0;
            string slotsFormatted = LocalizationService.Resolve(LocUI.Table, LocUI.UI_Workshop_Slots, null, currentCount, MAX_EXTRA_CART_SLOTS);

            _state.Value = new WorkshopViewState(
                money, weightFormatted, speedFormatted, mainCart, extraCarts, upgrades,
                currentCount, MAX_EXTRA_CART_SLOTS, slotsFormatted,
                false, null, null);
        }

        private MainCartViewData BuildMainCartData(PlayerResourceState resources, int money)
        {
            var cartClass = _caravanDatabase.GetCartClassById(resources.CartClassId);
            var currentUpgrade = _caravanDatabase.GetUpgradeLevelById(resources.CartUpgradeLevelId);

            string typeName = cartClass != null
                ? LocalizationService.ResolveString(cartClass.Name, cartClass.Id, "Workshop.CartClass")
                : resources.CartClassId;

            string upgradeName = LocalizationService.ResolveString(
                currentUpgrade.Name, currentUpgrade.Level.ToString(), "Workshop.UpgradeLevel");

            string displayName = $"{typeName} ({upgradeName})";

            float baseSpeed = cartClass?.SpeedKmDay ?? 0f;
            float baseCapacity = cartClass?.Capacity ?? 0f;
            float baseDurability = cartClass?.Durability ?? 0f;
            int animalCount = cartClass?.AnimalCount ?? 0;

            float speed = baseSpeed * currentUpgrade.SpeedMult;
            float capacity = baseCapacity * currentUpgrade.CapacityMult;
            float durability = baseDurability * currentUpgrade.DurabilityMult;

            var stats = new CartStatsData(
                displayName,
                LocalizationService.Resolve(LocUI.Table, LocUI.UI_Workshop_Speed, null, $"{speed:F0}"),
                LocalizationService.Resolve(LocUI.Table, LocUI.UI_Workshop_Capacity, null, $"{capacity:F0}"),
                LocalizationService.Resolve(LocUI.Table, LocUI.UI_Workshop_Durability, null, $"{durability:F0}"),
                LocalizationService.Resolve(LocUI.Table, LocUI.UI_Workshop_Animals, null, animalCount),
                "");

            var nextLevel = FindNextUpgradeLevel(resources.CartUpgradeLevelId);
            bool canUpgrade = nextLevel != null && money >= nextLevel.Value.Price;
            int upgradeCost = nextLevel?.Price ?? 0;

            string upgradeEffectText;
            if (nextLevel != null)
            {
                float nextSpeed = baseSpeed * nextLevel.Value.SpeedMult;
                float nextCapacity = baseCapacity * nextLevel.Value.CapacityMult;
                float nextDurability = baseDurability * nextLevel.Value.DurabilityMult;

                string effectDetail = $"{LocalizationService.Resolve(LocUI.Table, LocUI.UI_Workshop_Speed, null, $"{nextSpeed:F0}")}, " +
                                      $"{LocalizationService.Resolve(LocUI.Table, LocUI.UI_Workshop_Capacity, null, $"{nextCapacity:F0}")}, " +
                                      $"{LocalizationService.Resolve(LocUI.Table, LocUI.UI_Workshop_Durability, null, $"{nextDurability:F0}")}";
                upgradeEffectText = LocalizationService.Resolve(LocUI.Table, LocUI.UI_Workshop_UpgradeEffect, null, effectDetail);
            }
            else
            {
                upgradeEffectText = LocalizationService.Resolve(LocUI.Table, LocUI.UI_Workshop_MaxUpgrade);
            }

            bool isMaxLevel = nextLevel == null;
            return new MainCartViewData(stats, upgradeEffectText, canUpgrade, isMaxLevel, upgradeCost);
        }

        private List<ExtraCartViewData> BuildExtraCartList(PlayerResourceState resources, int money)
        {
            var result = new List<ExtraCartViewData>();
            int currentCount = resources.Carts?.Count ?? 0;
            bool hasSlots = currentCount < MAX_EXTRA_CART_SLOTS;

            foreach (var extraCart in _caravanDatabase.ExtraCarts)
            {
                string typeName = LocalizationService.ResolveString(
                    extraCart.Name, extraCart.Id, "Workshop.ExtraCart");

                var stats = new CartStatsData(
                    typeName,
                    LocalizationService.Resolve(LocUI.Table, LocUI.UI_Workshop_SpeedPenalty, null, $"-{extraCart.SpeedPenaltyPct:F0}"),
                    LocalizationService.Resolve(LocUI.Table, LocUI.UI_Workshop_Capacity, null, $"{extraCart.Capacity:F0}"),
                    LocalizationService.Resolve(LocUI.Table, LocUI.UI_Workshop_Durability, null, $"{extraCart.Durability:F0}"),
                    LocalizationService.Resolve(LocUI.Table, LocUI.UI_Workshop_Animals, null, 1),
                    LocalizationService.Resolve(LocUI.Table, LocUI.UI_Workshop_Consumption, null, $"{extraCart.SuppliesPerDay}"));

                int ownedCount = resources.Carts?.Count(c => c.TypeId == extraCart.Id) ?? 0;
                string countText = LocalizationService.Resolve(LocUI.Table, LocUI.UI_Workshop_OwnedCount, null, ownedCount);

                bool canBuy = hasSlots && money >= extraCart.Price;
                bool canSell = ownedCount > 0;

                result.Add(new ExtraCartViewData(stats, countText,
                    extraCart.Price, extraCart.SellPrice, canBuy, canSell, extraCart.Type));
            }

            return result;
        }

        private CaravanDatabase.CartUpgradeLevelEntry? FindNextUpgradeLevel(string currentLevelId)
        {
            var sorted = _caravanDatabase.UpgradeLevels
                .OrderBy(u => u.SortOrder)
                .ToList();

            int currentIndex = -1;
            for (int i = 0; i < sorted.Count; i++)
            {
                if (sorted[i].Level.ToString().Equals(currentLevelId, System.StringComparison.OrdinalIgnoreCase))
                {
                    currentIndex = i;
                    break;
                }
            }

            if (currentIndex < 0 || currentIndex >= sorted.Count - 1)
                return null;

            return sorted[currentIndex + 1];
        }

        private List<UpgradeViewData> BuildUpgradeList(PlayerResourceState resources, int money)
        {
            var list = new List<UpgradeViewData>();

            foreach (var entry in _caravanDatabase.CaravanUpgrades)
            {
                if (entry.Building != BuildingId.Workshop)
                    continue;

                string title = LocalizationService.ResolveString(
                    entry.Name, entry.Type.ToString(), "Workshop.UpgradeName");
                string description = LocalizationService.ResolveString(
                    entry.Description, entry.EffectDescription, "Workshop.UpgradeDesc");
                bool owned = _upgradeService.HasUpgrade(entry.Type);
                bool canBuy = !owned && money >= entry.Price;

                string buttonText = owned
                    ? LocalizationService.Resolve(LocUI.Table, LocUI.UI_Workshop_Owned)
                    : LocalizationService.Resolve(LocUI.Table, LocUI.UI_Workshop_Buy, null, entry.Price);

                list.Add(new UpgradeViewData(title, description, buttonText, canBuy, owned, entry.Type));
            }

            return list;
        }

    }
}
