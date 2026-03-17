using System.Collections.Generic;
using Internal.Scripts.Caravan;
using Internal.Scripts.Caravan.Generated;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Player;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.Screens.Shared;
using Internal.Scripts.UI.Theme;
using R3;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.Screens.Caravansary
{
    public sealed class CaravansaryScreenViewModel : ScreenViewModelBase
    {
        private const string SUITABLE = "suitable";
        private const string NEUTRAL = "neutral";
        private const string UNSUITABLE = "unsuitable";

        private readonly PlayerResourceRepository _resourceRepository;
        private readonly CaravanDatabase _caravanDatabase;
        private readonly DraftAnimalService _draftAnimalService;
        private readonly List<ICityOffering> _offerings;
        private readonly UiThemeService _themeService;
        private readonly ResourceIconCatalog _resourceIcons;
        private readonly ReactiveProperty<CaravansaryViewState> _state = new();

        private string _cityId;

        public UiThemeService ThemeService => _themeService;
        public ResourceIconCatalog ResourceIcons => _resourceIcons;

        public CaravansaryScreenViewModel(
            PlayerResourceRepository resourceRepository,
            CaravanDatabase caravanDatabase,
            DraftAnimalService draftAnimalService,
            List<ICityOffering> offerings,
            UiThemeService themeService,
            ResourceIconCatalog resourceIcons)
        {
            _resourceRepository = resourceRepository;
            _caravanDatabase = caravanDatabase;
            _draftAnimalService = draftAnimalService;
            _offerings = offerings;
            _themeService = themeService;
            _resourceIcons = resourceIcons;
        }

        public override ScreenId Id => ScreenId.Caravansary;
        public Observable<CaravansaryViewState> State => _state;

        protected override void OnOpen(object args)
        {
            _cityId = args as string;
            BuildState();
        }

        protected override void OnClose()
        {
        }

        public void ExecuteOffering(int index, bool toMax)
        {
            if (index < 0 || index >= _offerings.Count) return;
            _offerings[index].Execute(_resourceRepository, toMax);
            BuildState();
        }

        public void SwitchAnimal(DraftAnimalType animalType)
        {
            _draftAnimalService.SwitchAnimal(animalType);
            BuildState();
        }

        private void BuildState()
        {
            var resources = _resourceRepository.Current;
            int money = resources.Money;

            string weightFormatted = $"{resources.TotalCapacity:F0}";

            var offerings = BuildOfferings(resources, money);

            var currentAnimalData = _caravanDatabase.GetDraftAnimalById(resources.DraftAnimalId);
            DraftAnimalType currentType = currentAnimalData?.AnimalType ?? DraftAnimalType.Camel;

            var biomeMap = BuildBiomeMap();

            var currentAnimal = BuildAnimalData(currentAnimalData, 0, currentType, money, biomeMap);
            var animals = new List<AnimalSwitchData>();

            for (int i = 0; i < _caravanDatabase.DraftAnimals.Count; i++)
            {
                var animal = _caravanDatabase.DraftAnimals[i];
                if (animal.AnimalType == currentType) continue;
                animals.Add(BuildAnimalData(animal, i, currentType, money, biomeMap));
            }

            _state.Value = new CaravansaryViewState(
                money, weightFormatted, offerings, currentAnimal, animals);
        }

        private List<OfferingItem> BuildOfferings(PlayerResourceState resources, int money)
        {
            var result = new List<OfferingItem>();
            for (int i = 0; i < _offerings.Count; i++)
            {
                var item = _offerings[i].Build(resources, money);
                var indexed = item.Type == OfferingType.CartRepair
                    ? new OfferingItem(item.RepairData, i)
                    : new OfferingItem(item.Title, item.Description, item.ButtonText, item.CanAction, i);

                if (indexed.IsVisible)
                    result.Add(indexed);
            }
            return result;
        }

        private AnimalSwitchData BuildAnimalData(
            DraftAnimalData animal, int index,
            DraftAnimalType currentType, int money,
            Dictionary<DraftAnimalType, (List<string> incompatible, List<string> neutral, List<string> compatible)> biomeMap)
        {
            if (animal == null)
            {
                return new AnimalSwitchData("", "", "", "", "", "", "",
                    false, false, "", index, default);
            }

            string name = LocalizationService.ResolveString(animal.Name, animal.Id, "Caravansary.Animal");

            string effectText;
            if (animal.SpeedModPct == 0 && animal.CapacityModPct == 0)
            {
                effectText = ResolveLoc("UI.Global.NoEffect", "UI.Global.NoEffect");
            }
            else
            {
                string speedMod = FormatSignedPercent(animal.SpeedModPct);
                string capMod = FormatSignedPercent(animal.CapacityModPct);
                string combined = ResolveLoc("UI.Caravan.AnimalEffect.Base",
                    "UI.Caravan.AnimalEffect.Base", speedMod, capMod);
                effectText = ResolveLoc("UI.Global.CurrentEffect", "UI.Global.CurrentEffect", combined);
            }

            string priceText = ResolveLoc("UI.Caravansary.AnimalPrice", "UI.Caravansary.AnimalPrice", animal.Price);
            string consumptionText = ResolveLoc("UI.Caravansary.AnimalConsumption", "UI.Caravansary.AnimalConsumption", $"{animal.FeedPerDay:F0}");

            bool isCurrent = animal.AnimalType == currentType;
            bool canSwitch = !isCurrent && money >= animal.Price;

            string incompatible = "";
            string neutral = "";
            string compatible = "";

            if (biomeMap.TryGetValue(animal.AnimalType, out var biomes))
            {
                incompatible = string.Join(", ", biomes.incompatible);
                neutral = string.Join(", ", biomes.neutral);
                compatible = string.Join(", ", biomes.compatible);
            }

            string buttonText = isCurrent
                ? ""
                : ResolveLoc("UI.Caravansary.SwitchAnimal", "UI.Caravansary.SwitchAnimal", animal.Price);

            return new AnimalSwitchData(name, effectText, priceText, consumptionText,
                incompatible, neutral, compatible, isCurrent, canSwitch, buttonText, index, animal.AnimalType);
        }

        private Dictionary<DraftAnimalType, (List<string> incompatible, List<string> neutral, List<string> compatible)> BuildBiomeMap()
        {
            var map = new Dictionary<DraftAnimalType, (List<string> incompatible, List<string> neutral, List<string> compatible)>();

            foreach (var entry in _caravanDatabase.AnimalBiomeCompatibility)
            {
                if (!map.ContainsKey(entry.Animal))
                    map[entry.Animal] = (new List<string>(), new List<string>(), new List<string>());

                string biomeName = ResolveLoc($"UI.Biome.{entry.Biome}", $"UI.Biome.{entry.Biome}");
                var lists = map[entry.Animal];

                switch (entry.Compatibility?.ToLowerInvariant())
                {
                    case SUITABLE:
                        lists.compatible.Add(biomeName);
                        break;
                    case NEUTRAL:
                        lists.neutral.Add(biomeName);
                        break;
                    case UNSUITABLE:
                        lists.incompatible.Add(biomeName);
                        break;
                }
            }

            return map;
        }

        private static string FormatSignedPercent(float value)
        {
            return value >= 0 ? $"+{value:F0}%" : $"{value:F0}%";
        }

        private static string ResolveLoc(string key, string fallback, params object[] args)
        {
            var localized = new LocalizedString("UI", key);
            return LocalizationService.ResolveString(localized, fallback, key, args);
        }
    }
}
