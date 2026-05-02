using System;
using Internal.Scripts.UI.Screens.Archive.Tabs;
using Internal.Scripts.UI.Screens.Core.Tabs;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.Screens.Shared;
using R3;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.Archive
{
    public class ArchiveScreen : PopupScreen
    {
        [SerializeField] private ArchiveTabBinding[] _tabs;
        [SerializeField] private QuestCardView _questCardView;

        private ArchiveScreenViewModel _viewModel;
        private TabController _tabController;
        private IDisposable _stateSubscription;
        private IDisposable _questSlotSubscription;
        private bool _tabControllerSubscribed;

        public override void BindViewModel(IScreenViewModel viewModel)
        {
            _viewModel = viewModel as ArchiveScreenViewModel;
            EnsureTabController();
            BindTabViews();
            ConfigureDefaultTab();
            if (Localization != null)
                _tabController?.BindLocalization(Localization, name);
            SubscribeTabController();
            SubscribeViewModel();
        }

        protected override void OnLocalizationReady()
        {
            base.OnLocalizationReady();
            EnsureTabController();
            _tabController?.BindLocalization(Localization, name);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            EnsureTabController();
            BindTabViews();
            if (Localization != null)
                _tabController?.BindLocalization(Localization, name);
            SubscribeTabController();
            SubscribeViewModel();
        }

        protected override void OnDisable()
        {
            UnsubscribeTabController();
            _stateSubscription?.Dispose();
            _stateSubscription = null;
            _questSlotSubscription?.Dispose();
            _questSlotSubscription = null;
            _tabController?.DisposeBinding();
            base.OnDisable();
        }

        private void EnsureTabController()
        {
            if (_tabController != null) return;
            if (_viewModel == null) return;
            _tabController = new TabController(_tabs, OnTabSelected, _viewModel.ThemeService);
        }

        private void BindTabViews()
        {
            if (_viewModel == null || _tabs == null) return;
            foreach (var binding in _tabs)
            {
                if (binding?.View is ArchiveTabViewBase archiveTab)
                    archiveTab.Bind(_viewModel);
            }
        }

        private void ConfigureDefaultTab()
        {
            if (_viewModel == null || _tabController == null) return;
            if (_tabController.TryGetDefaultTab(out int defaultTabId))
                _viewModel.SwitchTab((ArchiveTab)defaultTabId);
        }

        private void SubscribeViewModel()
        {
            if (_viewModel == null || _stateSubscription != null) return;
            _stateSubscription = _viewModel.State.Subscribe(ApplyState);
            _questSlotSubscription = _viewModel.QuestSlot.Subscribe(questState =>
            {
                if (questState.HasValue)
                    _questCardView?.Initialize(questState.Value.Description, () => _viewModel.OnQuestSlotTalk());
                else
                    _questCardView?.Hide();
            });
        }

        private void ApplyState(ArchiveViewState state)
        {
            if (state == null) return;
            _tabController?.Apply((int)state.ActiveTab, state);
        }

        private void SubscribeTabController()
        {
            if (_tabController == null || _tabControllerSubscribed) return;
            _tabController.Subscribe();
            _tabControllerSubscribed = true;
        }

        private void UnsubscribeTabController()
        {
            if (_tabController == null || !_tabControllerSubscribed) return;
            _tabController.Unsubscribe();
            _tabControllerSubscribed = false;
        }

        private void OnTabSelected(int tabId) => _viewModel?.SwitchTab((ArchiveTab)tabId);
    }
}
