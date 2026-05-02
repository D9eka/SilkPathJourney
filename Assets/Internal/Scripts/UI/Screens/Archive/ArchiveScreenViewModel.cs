using System.Collections.Generic;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Screens.Archive.Tabs;
using Internal.Scripts.UI.Screens.Building;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.Theme;
using R3;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Internal.Scripts.UI.Screens.Archive
{
    public sealed class ArchiveScreenViewModel : ScreenViewModelBase
    {
        private readonly Dictionary<ArchiveTab, IArchiveTabBuilder> _builders;
        private readonly BuildingQuestSlotViewModel _questSlotVM;
        private readonly UiThemeService _themeService;
        private readonly ResourceIconCatalog _resourceIcons;
        private readonly ReactiveProperty<ArchiveViewState> _state = new();

        private ArchiveTab _activeTab = ArchiveTab.Cities;
        private string _selectedId;
        private string _cityId;

        public UiThemeService ThemeService => _themeService;
        public ResourceIconCatalog ResourceIcons => _resourceIcons;
        public Observable<ArchiveViewState> State => _state;
        public Observable<BuildingQuestSlotState?> QuestSlot => _questSlotVM.State;
        public override ScreenId Id => ScreenId.Archive;

        public ArchiveScreenViewModel(
            IReadOnlyList<IArchiveTabBuilder> builders,
            BuildingQuestSlotViewModel questSlotVM,
            UiThemeService themeService,
            ResourceIconCatalog resourceIcons)
        {
            _builders = new Dictionary<ArchiveTab, IArchiveTabBuilder>(builders.Count);
            foreach (var b in builders) _builders[b.Tab] = b;
            _questSlotVM = questSlotVM;
            _themeService = themeService;
            _resourceIcons = resourceIcons;
        }

        protected override void OnOpen(object args)
        {
            _cityId = args as string;
            _activeTab = ArchiveTab.Cities;
            _selectedId = null;
            _questSlotVM.Bind(Economy.Buildings.BuildingType.Archive, _cityId);
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

        public void SwitchTab(ArchiveTab tab)
        {
            if (!IsOpen) return;
            _activeTab = tab;
            _selectedId = null;
            BuildState();
        }

        public void SelectEntry(string id)
        {
            if (!IsOpen) return;
            _selectedId = id;
            BuildState();
        }

        public void OnQuestSlotTalk() => _questSlotVM.OnTalk();

        private void BuildState()
        {
            var builder = _builders[_activeTab];
            var items = builder.BuildItems();
            var detail = string.IsNullOrEmpty(_selectedId) ? ArchiveDetailData.Empty : builder.BuildDetail(_selectedId);
            _state.Value = new ArchiveViewState(_activeTab, items, _selectedId, detail);
        }
    }
}
