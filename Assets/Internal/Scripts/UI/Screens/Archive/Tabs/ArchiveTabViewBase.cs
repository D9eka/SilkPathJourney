using System.Collections.Generic;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Core.Tabs;
using Internal.Scripts.UI.Theme;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.Screens.Archive.Tabs
{
    public abstract class ArchiveTabViewBase : MonoBehaviour, ITabView
    {
        [SerializeField] private ArchiveListItemView _listItemPrefab;
        [SerializeField] private Transform _listContent;
        [SerializeField] private ArchiveDetailView _detailView;
        [SerializeField] private LocalizedString _emptyStateLocalizedString;

        private ArchiveScreenViewModel _viewModel;
        private readonly List<ArchiveListItemView> _spawnedItems = new();

        protected abstract ArchiveTab Tab { get; }
        public int TabId => (int)Tab;

        public void Bind(ArchiveScreenViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public void BindLocalization(LocalizationService localization) { }
        public void DisposeBinding() { }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        public void Apply(object state)
        {
            if (state is not ArchiveViewState viewState) return;
            RebuildList(viewState.Items, viewState.SelectedId);
            UpdateDetail(viewState.Detail);
        }

        private void RebuildList(IReadOnlyList<ArchiveListEntry> items, string selectedId)
        {
            foreach (var item in _spawnedItems)
                Destroy(item.gameObject);
            _spawnedItems.Clear();

            if (items == null || _listItemPrefab == null || _listContent == null)
                return;

            foreach (var entry in items)
            {
                var instance = Instantiate(_listItemPrefab, _listContent);
                instance.gameObject.InitializeColorBinders(themeService: _viewModel?.ThemeService);
                instance.Initialize(entry, HandleSelect);
                instance.SetSelected(entry.Id == selectedId);
                _spawnedItems.Add(instance);
            }
        }

        private void UpdateDetail(ArchiveDetailData detail)
        {
            if (_detailView == null) return;
            if (!detail.HasData)
            {
                string message = LocalizationService.ResolveString(
                    _emptyStateLocalizedString, "Select an item from the list", "Archive.Empty");
                _detailView.ShowEmpty(message);
            }
            else
            {
                _detailView.ShowEntry(detail.Title, detail.Description);
            }
        }

        private void HandleSelect(string id)
        {
            _viewModel?.SelectEntry(id);
        }
    }
}
