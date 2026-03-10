using System;
using System.Collections.Generic;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.Theme;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.EnterCity
{
    public class EnterCityScreen : PopupScreen
    {
        [SerializeField] private BuildingButtonView _buttonPrefab;

        [Header("Layout")]
        [SerializeField] private GridLayoutGroup _singleGrid;
        [SerializeField] private GridLayoutGroup _doubleGrid;
        [SerializeField] private RectTransform _buildingsRoot;
        [SerializeField] private int _maxRows = 6;

        private EnterCityScreenViewModel _viewModel;
        private IDisposable _stateSubscription;
        private readonly List<BuildingButtonView> _spawnedButtons = new();

        protected override void OnEnable()
        {
            base.OnEnable();
            SubscribeViewModel();
        }

        protected override void OnDisable()
        {
            UnsubscribeViewModel();
            base.OnDisable();
        }

        public override void BindViewModel(IScreenViewModel viewModel)
        {
            _viewModel = viewModel as EnterCityScreenViewModel;
            SubscribeViewModel();
        }

        private void SubscribeViewModel()
        {
            if (_viewModel == null || _stateSubscription != null) return;
            _stateSubscription = _viewModel.State.Subscribe(ApplyState);
        }

        private void UnsubscribeViewModel()
        {
            if (_viewModel == null) return;
            _stateSubscription?.Dispose();
            _stateSubscription = null;
        }

        private void ApplyState(EnterCityViewState state)
        {
            if (state != null && _mainHeader != null && _mainHeader.Text != null)
                _mainHeader.Text.text = state.CityName;
            RebuildButtons(state?.Buildings);
        }

        private void RebuildButtons(IReadOnlyList<BuildingEntry> entries)
        {
            foreach (var btn in _spawnedButtons)
                Destroy(btn.gameObject);
            _spawnedButtons.Clear();

            if (entries == null || _buttonPrefab == null || _buildingsRoot == null) return;

            LayoutRebuilder.ForceRebuildLayoutImmediate(_buildingsRoot);

            int singleCount = CalcSingleCount(entries.Count, _buildingsRoot.rect.height);

            for (int i = 0; i < entries.Count; i++)
            {
                var parent = i < singleCount ? _singleGrid.transform : _doubleGrid.transform;
                var instance = Instantiate(_buttonPrefab, parent);
                instance.gameObject.InitializeColorBinders(themeService: _viewModel?.ThemeService);
                instance.Initialize(entries[i], HandleBuildingClicked);
                _spawnedButtons.Add(instance);
            }

            ApplyCellWidths(singleCount, entries.Count);
        }

        private int CalcSingleCount(int total, float availableHeight)
        {
            float rowH = _singleGrid.cellSize.y + _singleGrid.spacing.y;
            int effectiveMax = Mathf.Min(_maxRows, Mathf.FloorToInt(availableHeight / rowH));
            int bestSingle = 0;

            for (int k = 0; k <= total; k++)
            {
                if (k + Mathf.CeilToInt((total - k) / 2f) <= effectiveMax)
                    bestSingle = k;
                else
                    break;
            }

            return bestSingle;
        }

        private void ApplyCellWidths(int singleCount, int total)
        {
            float w = _buildingsRoot.rect.width;
            _singleGrid.cellSize = new Vector2(w, _singleGrid.cellSize.y);
            _singleGrid.gameObject.SetActive(singleCount > 0);

            float dw = (w - _doubleGrid.spacing.x) / 2f;
            _doubleGrid.cellSize = new Vector2(dw, _doubleGrid.cellSize.y);
            _doubleGrid.gameObject.SetActive(total - singleCount > 0);
        }

        private void HandleBuildingClicked(BuildingEntry entry)
        {
            _viewModel?.OpenBuilding(entry);
        }
    }
}
