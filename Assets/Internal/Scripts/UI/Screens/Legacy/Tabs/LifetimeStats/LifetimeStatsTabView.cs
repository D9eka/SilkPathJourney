using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Core.Tabs;
using Internal.Scripts.UI.Screens.Legacy.Tabs.LifetimeStats.Sections;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.Legacy.Tabs.LifetimeStats
{
    public sealed class LifetimeStatsTabView : MonoBehaviour, ITabView
    {
        [SerializeField] private LifeStatsSection[] _lifeStatsSections;
        private LegacyLifetimeTabViewModel _viewModel;

        public int TabId => (int)LegacyTab.Lifetime;

        public void Bind(LegacyLifetimeTabViewModel viewModel, LocalizationService localization)
        {
            _viewModel = viewModel;
            if (localization != null)
                BindLocalization(localization);
        }
        
        public void BindLocalization(LocalizationService localization)
        {
            DisposeBinding();

            foreach (LifeStatsSection lifeStatsSection in _lifeStatsSections)
            {
                lifeStatsSection.BindLocalization(localization);
            }
        }

        public void DisposeBinding()
        {
            foreach (LifeStatsSection lifeStatsSection in _lifeStatsSections)
            {
                lifeStatsSection.DisposeBinding();
            }
        }

        public void Apply(LifetimeStatsViewState viewState)
        {
            if (viewState == null) return;
            foreach (LifeStatsSection lifeStatsSection in _lifeStatsSections)
            {
                lifeStatsSection.Apply(viewState);
            }
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        public void Apply(object state)
        {
            Apply(state as LifetimeStatsViewState);
        }
    }
}
