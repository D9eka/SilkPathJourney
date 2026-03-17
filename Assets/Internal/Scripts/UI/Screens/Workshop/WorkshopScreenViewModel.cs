using System.Collections.Generic;
using System.Linq;
using Internal.Scripts.Caravan;
using Internal.Scripts.Caravan.Generated;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Events;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Player;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.Screens.Shared;
using Internal.Scripts.UI.Theme;
using R3;
using UnityEngine;
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
        private readonly ReactiveProperty<WorkshopViewState> _state = new();

        private string _cityId;

        public UiThemeService ThemeService => _themeService;
        public ResourceIconCatalog ResourceIcons => _resourceIcons;
        public Observable<WorkshopViewState> State => _state;
        public override ScreenId Id => ScreenId.Workshop;

        public WorkshopScreenViewModel(
            PlayerResourceRepository resourceRepository,
            CaravanDatabase caravanDatabase,
            CaravanUpgradeService upgradeService,
            EventTrigger eventTrigger,
            EventDatabase eventDb,
            UiThemeService themeService,
            ResourceIconCatalog resourceIcons)
        {
            _resourceRepository = resourceRepository;
            _caravanDatabase = caravanDatabase;
            _upgradeService = upgradeService;
            _eventTrigger = eventTrigger;
            _eventDb = eventDb;
            _themeService = themeService;
            _resourceIcons = resourceIcons;
        }

        protected override void OnOpen(object args)
        {
            _cityId = args as string;
            BuildState();
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        }

        protected override void OnClose()
        {
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
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
                    FoodConsumptionPerDay = extraCartData.SuppliesPerDay
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

        public void TalkToQuestGiver(string eventId)
        {
            if (string.IsNullOrEmpty(eventId))
                return;

            var eventData = _eventDb.GetById(eventId);
            if (eventData != null)
                _eventTrigger.TriggerEvent(eventData);
        }

        public string ResolveUpgradeLabel(int cost)
        {
            return ResolveLoc("UI.Workshop.Upgrade", "UI.Workshop.Upgrade", cost);
        }

        public string ResolveMaxUpgradeLabel()
        {
            return ResolveLoc("UI.Workshop.MaxUpgrade", "UI.Workshop.MaxUpgrade");
        }

        public string ResolveBuyLabel(int price)
        {
            return ResolveLoc("UI.Workshop.Buy", "UI.Workshop.Buy", price);
        }

        public string ResolveSellLabel(int price)
        {
            return ResolveLoc("UI.Workshop.Sell", "UI.Workshop.Sell", price);
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
            string slotsFormatted = ResolveLoc("UI.Workshop.Slots", "UI.Workshop.Slots", currentCount, MAX_EXTRA_CART_SLOTS);

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
                ResolveLoc("UI.Workshop.Speed", "UI.Workshop.Speed", $"{speed:F0}"),
                ResolveLoc("UI.Workshop.Capacity", "UI.Workshop.Capacity", $"{capacity:F0}"),
                ResolveLoc("UI.Workshop.Durability", "UI.Workshop.Durability", $"{durability:F0}"),
                ResolveLoc("UI.Workshop.Animals", "UI.Workshop.Animals", animalCount),
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

                string effectDetail = $"{ResolveLoc("UI.Workshop.Speed", "UI.Workshop.Speed", $"{nextSpeed:F0}")}, " +
                                      $"{ResolveLoc("UI.Workshop.Capacity", "UI.Workshop.Capacity", $"{nextCapacity:F0}")}, " +
                                      $"{ResolveLoc("UI.Workshop.Durability", "UI.Workshop.Durability", $"{nextDurability:F0}")}";
                upgradeEffectText = ResolveLoc("UI.Workshop.UpgradeEffect", "UI.Workshop.UpgradeEffect", effectDetail);
            }
            else
            {
                upgradeEffectText = ResolveLoc("UI.Workshop.MaxUpgrade", "UI.Workshop.MaxUpgrade");
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
                    ResolveLoc("UI.Workshop.SpeedPenalty", "UI.Workshop.SpeedPenalty", $"-{extraCart.SpeedPenaltyPct:F0}"),
                    ResolveLoc("UI.Workshop.Capacity", "UI.Workshop.Capacity", $"{extraCart.Capacity:F0}"),
                    ResolveLoc("UI.Workshop.Durability", "UI.Workshop.Durability", $"{extraCart.Durability:F0}"),
                    ResolveLoc("UI.Workshop.Animals", "UI.Workshop.Animals", 1),
                    ResolveLoc("UI.Workshop.Consumption", "UI.Workshop.Consumption", $"{extraCart.SuppliesPerDay}"));

                int ownedCount = resources.Carts?.Count(c => c.TypeId == extraCart.Id) ?? 0;
                string countText = ResolveLoc("UI.Workshop.OwnedCount", "UI.Workshop.OwnedCount", ownedCount);

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
                    ? ResolveLoc("UI.Workshop.Owned", "UI.Workshop.Owned")
                    : ResolveLoc("UI.Workshop.Buy", "UI.Workshop.Buy", entry.Price);

                list.Add(new UpgradeViewData(title, description, buttonText, canBuy, owned, entry.Type));
            }

            return list;
        }

        private static string ResolveLoc(string key, string fallback, params object[] args)
        {
            var localized = new LocalizedString("UI", key);
            return LocalizationService.ResolveString(localized, fallback, key, args);
        }
    }
}
