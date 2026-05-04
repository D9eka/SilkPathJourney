using System;
using System.Collections.Generic;
using Internal.Scripts.Caravan;
using Internal.Scripts.Player.Background;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.Theme;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.NewGame
{
    public class NewGameScreen : PopupScreen
    {
        [Header("Background List")]
        [SerializeField] private Transform _backgroundListContent;
        [SerializeField] private BackgroundCardView _backgroundCardPrefab;

        [Header("Detail Panel")]
        [SerializeField] private BackgroundDetailView _detailView;

        [Header("Cart Class")]
        [SerializeField] private TextMeshProUGUI _cartClassHeaderText;
        [SerializeField] private LocalizedString _cartClassHeaderString;
        [SerializeField] private Transform _cartClassListContent;
        [SerializeField] private CartClassButton _cartClassButtonPrefab;

        [Header("Start Button")]
        [SerializeField] private Button _startButton;
        [SerializeField] private TextMeshProUGUI _startButtonText;
        [SerializeField] private LocalizedString _startButtonString;

        private NewGameScreenViewModel _viewModel;
        private IDisposable _backgroundSubscription;
        private IDisposable _cartClassSubscription;
        private IDisposable _canStartSubscription;
        private LocalizationService.LocalizedTextHandle _startButtonHandle;
        private LocalizationService.LocalizedTextHandle _cartClassHeaderHandle;
        private readonly List<BackgroundCardView> _spawnedCards = new();
        private readonly List<CartClassButton> _spawnedCartButtons = new();

        public override void BindViewModel(IScreenViewModel viewModel)
        {
            _viewModel = viewModel as NewGameScreenViewModel;
            SubscribeViewModel();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _startButton.onClick.AddListener(OnStart);
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
            SubscribeViewModel();
        }

        protected override void OnDisable()
        {
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
            _backgroundSubscription?.Dispose();
            _backgroundSubscription = null;
            _cartClassSubscription?.Dispose();
            _cartClassSubscription = null;
            _canStartSubscription?.Dispose();
            _canStartSubscription = null;
            _startButtonHandle?.Dispose();
            _startButtonHandle = null;
            _cartClassHeaderHandle?.Dispose();
            _cartClassHeaderHandle = null;
            _startButton.onClick.RemoveListener(OnStart);
            base.OnDisable();
        }

        private void OnLocaleChanged(Locale _)
        {
            if (_viewModel == null) return;
            RebuildList();
            RefreshDetail(_viewModel.SelectedBackground.Value);
        }

        protected override void OnLocalizationReady()
        {
            base.OnLocalizationReady();
            if (_viewModel != null)
                _detailView.Initialize(_viewModel.ThemeService, Localization, _viewModel.ItemCatalog, _viewModel.EconomyDatabase);
            RefreshStartButtonText();
            RefreshDetail(_viewModel?.SelectedBackground.Value);
        }

        private void SubscribeViewModel()
        {
            if (_viewModel == null || _backgroundSubscription != null)
                return;

            _detailView.Initialize(_viewModel.ThemeService, Localization, _viewModel.ItemCatalog, _viewModel.EconomyDatabase);
            RebuildList();
            _backgroundSubscription = _viewModel.SelectedBackground.Subscribe(OnBackgroundSelected);
            _cartClassSubscription = _viewModel.SelectedCartClass.Subscribe(OnCartClassSelected);
            _canStartSubscription = _viewModel.CanStart.Subscribe(OnCanStartChanged);
        }

        private void RebuildList()
        {
            if (_viewModel == null) return;

            foreach (BackgroundCardView card in _spawnedCards)
                Destroy(card.gameObject);
            _spawnedCards.Clear();

            IReadOnlyList<BackgroundData> backgrounds = _viewModel.Backgrounds;
            BackgroundData selected = _viewModel.SelectedBackground.Value;

            foreach (BackgroundData bg in backgrounds)
            {
                BackgroundCardView card = Instantiate(_backgroundCardPrefab, _backgroundListContent);
                card.gameObject.InitializeColorBinders(themeService: _viewModel.ThemeService);
                card.SetData(bg, _viewModel, bg == selected);
                _spawnedCards.Add(card);
            }

            RebuildCartClassList();
        }

        private void RebuildCartClassList()
        {
            if (_viewModel == null) return;

            foreach (CartClassButton btn in _spawnedCartButtons)
                Destroy(btn.gameObject);
            _spawnedCartButtons.Clear();

            IReadOnlyList<CartClassData> classes = _viewModel.CartClasses;
            CartClassData selectedCart = _viewModel.SelectedCartClass.Value;

            foreach (CartClassData cls in classes)
            {
                CartClassButton btn = Instantiate(_cartClassButtonPrefab, _cartClassListContent);
                btn.gameObject.InitializeColorBinders(themeService: _viewModel.ThemeService);
                btn.SetData(cls, _viewModel, cls == selectedCart);
                _spawnedCartButtons.Add(btn);
            }
        }

        private void OnBackgroundSelected(BackgroundData bg)
        {
            foreach (BackgroundCardView card in _spawnedCards)
                card.SetSelected(card.Data == bg);

            RefreshDetail(bg);
        }

        private void OnCartClassSelected(CartClassData selected)
        {
            IReadOnlyList<CartClassData> classes = _viewModel.CartClasses;
            for (int i = 0; i < _spawnedCartButtons.Count; i++)
                _spawnedCartButtons[i].SetSelected(i < classes.Count && classes[i] == selected);
        }

        private void OnCanStartChanged(bool canStart)
        {
            _startButton.interactable = canStart;
        }

        private void RefreshDetail(BackgroundData bg)
        {
            if (bg == null) return;
            _detailView.SetData(bg);
        }

        private void RefreshStartButtonText()
        {
            _startButtonHandle?.Dispose();
            _cartClassHeaderHandle?.Dispose();
            if (Localization == null) return;

            _startButtonHandle = Localization.BindText(_startButtonText, _startButtonString, $"{name}.StartButton");
            if (_cartClassHeaderText != null && _cartClassHeaderString != null)
                _cartClassHeaderHandle = Localization.BindText(_cartClassHeaderText, _cartClassHeaderString, $"{name}.CartClassHeader");
        }

        private void OnStart() => _viewModel?.Start();
    }
}
