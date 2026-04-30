using System;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Screens.Core.Tabs;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.Screens.Legacy.Tabs.Achievement;
using Internal.Scripts.UI.Screens.Legacy.Tabs.LifetimeStats;
using Internal.Scripts.UI.Screens.Legacy.Tabs.Shop;
using R3;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.Legacy
{
    public sealed class LegacyScreen : PopupScreen
    {
        [SerializeField] private ResourceIndicator _legacyPointsIndicator;

        [Header("Tabs")]
        [SerializeField] private LegacyTabBinding[] _tabs;

        private LegacyScreenViewModel _viewModel;
        private TabController _tabController;
        private IDisposable _stateSubscription;
        private bool _tabControllerSubscribed;

        public override void BindViewModel(IScreenViewModel viewModel)
        {
            _viewModel = viewModel as LegacyScreenViewModel;
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
            if (_viewModel == null) return;

            foreach (ITabView view in BuildTabViews())
            {
                switch (view)
                {
                    case ShopTabView shopTabView:
                        shopTabView.Bind(_viewModel.ShopTab, OnUnlock, Localization);
                        break;
                    case AchievementsTabView achievementsTabView:
                        achievementsTabView.Bind(_viewModel.AchievementsTab, Localization);
                        break;
                    case LifetimeStatsTabView lifetimeStatsTabView:
                        lifetimeStatsTabView.Bind(_viewModel.LifetimeTab, Localization);
                        break;
                }
            }
        }

        private ITabView[] BuildTabViews()
        {
            if (_tabs == null || _tabs.Length == 0)
                return Array.Empty<ITabView>();

            var views = new ITabView[_tabs.Length];
            for (int i = 0; i < _tabs.Length; i++)
                views[i] = _tabs[i]?.View;
            return views;
        }

        private void ConfigureDefaultTab()
        {
            if (_viewModel == null || _tabController == null) return;
            if (_tabController.TryGetDefaultTab(out int defaultTabId))
                _viewModel.SetDefaultTab((LegacyTab)defaultTabId);
        }

        private void SubscribeViewModel()
        {
            if (_viewModel == null || _stateSubscription != null) return;
            _stateSubscription = _viewModel.State.Subscribe(ApplyState);
        }

        private void ApplyState(LegacyViewState state)
        {
            if (state == null) return;

            _legacyPointsIndicator?.SetValue(state.LegacyPointsTotal);
            _tabController?.Apply((int)state.ActiveTab, state.ActiveTabState);
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

        private void OnUnlock(string id) => _viewModel?.TryUnlock(id);
        private void OnTabSelected(int tabId) => _viewModel?.SwitchTab((LegacyTab)tabId);
    }
}
