using System.Collections.Generic;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Buildings.Healer;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.Theme;
using R3;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Internal.Scripts.UI.Screens.Healer
{
    public sealed class HealerScreenViewModel : ScreenViewModelBase
    {
        private readonly HealerService _healerService;
        private readonly PlayerResourceRepository _resourceRepository;
        private readonly HealerEntryFormatter _formatter;
        private readonly UiThemeService _themeService;
        private readonly ResourceIconCatalog _resourceIcons;
        private readonly ReactiveProperty<HealerViewState> _state = new();

        public UiThemeService ThemeService => _themeService;
        public ResourceIconCatalog ResourceIcons => _resourceIcons;
        public Observable<HealerViewState> State => _state;
        public override ScreenId Id => ScreenId.Healer;

        public HealerScreenViewModel(
            HealerService healerService,
            PlayerResourceRepository resourceRepository,
            HealerEntryFormatter formatter,
            UiThemeService themeService,
            ResourceIconCatalog resourceIcons)
        {
            _healerService = healerService;
            _resourceRepository = resourceRepository;
            _formatter = formatter;
            _themeService = themeService;
            _resourceIcons = resourceIcons;
        }

        protected override void OnOpen(object args)
        {
            BuildState();
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        }

        protected override void OnClose()
        {
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
            _state.Value = null;
        }

        private void OnLocaleChanged(Locale _) => BuildState();

        public void Heal(int companionIndex)
        {
            _healerService.TryHeal(companionIndex);
            BuildState();
        }

        private void BuildState()
        {
            var res = _resourceRepository.Current;
            int money = res.Money;
            var entries = new List<HealerEntry>(res.Companions.Count);

            for (int i = 0; i < res.Companions.Count; i++)
                entries.Add(_formatter.Format(i, res.Companions[i], money));

            bool anyInjured = entries.Exists(e => e.IsInjured);
            _state.Value = new HealerViewState(entries, money, anyInjured);
        }
    }
}
