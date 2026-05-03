using System.Collections.Generic;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Buildings;
using Internal.Scripts.Economy.Buildings.Temple;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Building;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.Theme;
using R3;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Internal.Scripts.UI.Screens.Temple
{
    public sealed class TempleScreenViewModel : ScreenViewModelBase
    {
        private readonly TempleService _templeService;
        private readonly PlayerResourceRepository _resourceRepository;
        private readonly BuildingQuestSlotViewModel _questSlotVM;
        private readonly UiThemeService _themeService;
        private readonly ResourceIconCatalog _resourceIcons;
        private readonly ReactiveProperty<TempleViewState> _state = new();

        private string _cityId;

        public UiThemeService ThemeService => _themeService;
        public ResourceIconCatalog ResourceIcons => _resourceIcons;
        public Observable<TempleViewState> State => _state;
        public Observable<BuildingQuestSlotState?> QuestSlot => _questSlotVM.State;
        public override ScreenId Id => ScreenId.Temple;

        public TempleScreenViewModel(
            TempleService templeService,
            PlayerResourceRepository resourceRepository,
            BuildingQuestSlotViewModel questSlotVM,
            UiThemeService themeService,
            ResourceIconCatalog resourceIcons)
        {
            _templeService = templeService;
            _resourceRepository = resourceRepository;
            _questSlotVM = questSlotVM;
            _themeService = themeService;
            _resourceIcons = resourceIcons;
        }

        protected override void OnOpen(object args)
        {
            _cityId = args as string;
            _questSlotVM.Bind(BuildingType.Temple, _cityId);
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

        public void OnQuestSlotTalk() => _questSlotVM.OnTalk();

        public void Action(TempleServiceKind kind)
        {
            bool success = kind switch
            {
                TempleServiceKind.Pray => _templeService.TryPray(),
                TempleServiceKind.Donate => _templeService.TryDonate(),
                TempleServiceKind.Bless => _templeService.TryBless(),
                _ => false
            };

            if (success)
                BuildState();
        }

        private void BuildState()
        {
            var resources = _resourceRepository.Current;
            int money = resources.Money;
            int morale = UnityEngine.Mathf.RoundToInt(resources.Morale);
            int danger = UnityEngine.Mathf.RoundToInt(resources.AccumulatedDanger);
            var entries = new List<TempleEntry>(3)
            {
                BuildEntry(TempleServiceKind.Pray,
                    "ui.temple.pray.name", "ui.temple.pray.description", "ui.temple.pray.button",
                    "Молитва", "Обратитесь к богам с молитвой.", "Помолиться · {0} ●",
                    _templeService.GetPrayCost(), money),
                BuildEntry(TempleServiceKind.Donate,
                    "ui.temple.donate.name", "ui.temple.donate.description", "ui.temple.donate.button",
                    "Пожертвование", "Пожертвуйте храму.", "Пожертвовать · {0} ●",
                    _templeService.GetDonateCost(), money),
                BuildEntry(TempleServiceKind.Bless,
                    "ui.temple.bless.name", "ui.temple.bless.description", "ui.temple.bless.button",
                    "Благословение", "Попросите благословения.", "Получить благословение · {0} ●",
                    _templeService.GetBlessCost(), money)
            };

            _state.Value = new TempleViewState(entries, money, morale, danger);
        }

        private static TempleEntry BuildEntry(TempleServiceKind kind,
            string nameKey, string descKey, string btnKey,
            string nameFallback, string descFallback, string btnFallback,
            int cost, int money)
        {
            string name = LocalizationService.ResolveString(new LocalizedString("UI", nameKey), nameFallback, $"Temple.{nameKey}");
            string desc = LocalizationService.ResolveString(new LocalizedString("UI", descKey), descFallback, $"Temple.{descKey}");
            string btn = LocalizationService.ResolveString(new LocalizedString("UI", btnKey), string.Format(btnFallback, cost), $"Temple.{btnKey}", cost);
            return new TempleEntry(kind, name, desc, btn, money >= cost);
        }
    }
}
