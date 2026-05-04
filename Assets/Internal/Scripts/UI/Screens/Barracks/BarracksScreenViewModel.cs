using System.Collections.Generic;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Buildings;
using Internal.Scripts.Economy.Buildings.Barracks;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Building;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.Theme;
using R3;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Internal.Scripts.UI.Screens.Barracks
{
    public sealed class BarracksScreenViewModel : ScreenViewModelBase
    {
        private readonly BarracksService _barracksService;
        private readonly PlayerResourceRepository _resourceRepository;
        private readonly BuildingQuestSlotViewModel _questSlotVM;
        private readonly UiThemeService _themeService;
        private readonly ResourceIconCatalog _resourceIcons;
        private readonly ReactiveProperty<BarracksViewState> _state = new();

        private string _cityId;

        public UiThemeService ThemeService => _themeService;
        public ResourceIconCatalog ResourceIcons => _resourceIcons;
        public Observable<BarracksViewState> State => _state;
        public Observable<BuildingQuestSlotState?> QuestSlot => _questSlotVM.State;
        public override ScreenId Id => ScreenId.Barracks;

        public BarracksScreenViewModel(
            BarracksService barracksService,
            PlayerResourceRepository resourceRepository,
            BuildingQuestSlotViewModel questSlotVM,
            UiThemeService themeService,
            ResourceIconCatalog resourceIcons)
        {
            _barracksService = barracksService;
            _resourceRepository = resourceRepository;
            _questSlotVM = questSlotVM;
            _themeService = themeService;
            _resourceIcons = resourceIcons;
        }

        protected override void OnOpen(object args)
        {
            _cityId = args as string;
            _questSlotVM.Bind(BuildingType.Barracks, _cityId);
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

        public void Action(BarracksServiceKind kind)
        {
            if (kind == BarracksServiceKind.HireGuard && _barracksService.TryHireGuard())
                BuildState();
        }

        public void UpgradeEliteGuard()
        {
            if (_barracksService.TryUpgradeEliteGuard())
                BuildState();
        }

        private void BuildState()
        {
            int money = _resourceRepository.Current.Money;
            int guardCost = _barracksService.GetGuardCost();
            int eliteCost = _barracksService.GetEliteGuardCost();
            bool eliteOwned = _barracksService.IsEliteGuardOwned();

            string guardName = LocalizationService.ResolveString(new LocalizedString("UI", "ui.barracks.hire_guard.name"), "Наём охранника", "Barracks.HireGuard.Name");
            string guardDesc = LocalizationService.ResolveString(new LocalizedString("UI", "ui.barracks.hire_guard.description"), "Нанять охранника для защиты каравана.", "Barracks.HireGuard.Desc");
            string guardBtn = LocalizationService.ResolveString(new LocalizedString("UI", "ui.barracks.hire_guard.button"), $"Нанять · {guardCost} ●", "Barracks.HireGuard.Btn", guardCost);

            var scrollEntries = new List<BarracksEntry>(1)
            {
                new BarracksEntry(BarracksServiceKind.HireGuard, guardName, guardDesc, guardBtn, money >= guardCost)
            };

            string eliteName = LocalizationService.ResolveString(new LocalizedString("UI", "ui.barracks.elite_guard.name"), "Элитная охрана", "Barracks.EliteGuard.Name");
            string eliteDesc = LocalizationService.ResolveString(new LocalizedString("UI", "ui.barracks.elite_guard.description"), "Улучшение каравана: элитные бойцы.", "Barracks.EliteGuard.Desc");
            string eliteBtn = eliteOwned
                ? LocalizationService.ResolveString(new LocalizedString("UI", "ui.barracks.elite_guard.owned"), "Куплено", "Barracks.EliteGuard.Owned")
                : LocalizationService.ResolveString(new LocalizedString("UI", "ui.barracks.elite_guard.button"), $"Купить · {eliteCost} ●", "Barracks.EliteGuard.Btn", eliteCost);

            var eliteGuard = new EliteGuardEntry(eliteName, eliteDesc, eliteBtn, money >= eliteCost, eliteOwned);

            _state.Value = new BarracksViewState(scrollEntries, eliteGuard, money);
        }
    }
}
