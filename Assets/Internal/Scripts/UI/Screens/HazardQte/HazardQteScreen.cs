using System;
using System.Collections.Generic;
using DG.Tweening;
using Internal.Scripts.Travel.Hazards;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Screens.Core.View;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Plugins.Zenject.Source.Main;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Internal.Scripts.UI.Screens.HazardQte
{
    public sealed class HazardQteScreen : ScreenViewBase
    {
        private const float FADE_DURATION = 0.3f;

        [SerializeField] private IconLabelView _header;
        [SerializeField] private FillBar _timerBar;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private TextMeshProUGUI _hintText;
        [SerializeField] private CanvasGroup _hintCanvasGroup;
        [SerializeField] private CanvasGroup _resultCanvasGroup;
        [SerializeField] private TextMeshProUGUI _resultTitleText;
        [SerializeField] private Transform _resultOutcomesContainer;
        [SerializeField] private IconLabelView _iconLabelPrefab;
        [SerializeField] private Color _successColor = new(0.2f, 0.9f, 0.2f);
        [SerializeField] private Color _failColor = new(0.9f, 0.2f, 0.2f);
        [SerializeField] private Transform _minigameContainer;
        [SerializeField] private LayoutElement _minigameLayoutElement;

        private IInstantiator _instantiator;
        private HazardQteViewModel _vm;
        private HazardTimerView _timer;
        private HazardOutcomesView _outcomes;
        private HazardMinigameSlot _minigame;

        [Inject]
        public void Construct(IInstantiator instantiator) => _instantiator = instantiator;

        private HazardData _currentData;
        private Tween _hintTween;
        private Tween _resultTween;
        private readonly List<IDisposable> _subs = new();

        protected override void Awake()
        {
            base.Awake();
            _timer = new HazardTimerView(_timerBar, _timerText);
            _outcomes = new HazardOutcomesView(_resultOutcomesContainer, _iconLabelPrefab);
            _minigame = new HazardMinigameSlot(_minigameContainer, _minigameLayoutElement, _instantiator);
        }

        private void OnEnable()
        {
            SubscribeViewModel();
        }

        private void OnDisable()
        {
            UnsubscribeViewModel();
            _minigame?.Hide();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _timer?.Dispose();
        }

        public override void BindViewModel(IScreenViewModel viewModel)
        {
            _vm = viewModel as HazardQteViewModel;
            SubscribeViewModel();
        }

        private void SubscribeViewModel()
        {
            if (_vm == null || _subs.Count > 0) return;

            _subs.Add(_vm.Data.Subscribe(OnDataChanged));
            _subs.Add(_vm.MinigamePrefab.Subscribe(OnMinigamePrefabChanged));
            _subs.Add(_vm.Result.Subscribe(OnResultChanged));
            _subs.Add(_vm.WarningText.Subscribe(OnWarningTextChanged));
            _subs.Add(_vm.HintText.Subscribe(t => _hintText.text = t ?? string.Empty));
            _subs.Add(_vm.TimeRemaining.Subscribe(t => _timer.Render(t, _vm.TimeLimit)));
            _subs.Add(_vm.ResultTitle.Subscribe(text => _resultTitleText.text = text));
            _vm.OnTimeout += HandleTimeout;
        }

        private void UnsubscribeViewModel()
        {
            if (_vm != null)
                _vm.OnTimeout -= HandleTimeout;
            foreach (var sub in _subs)
                sub.Dispose();
            _subs.Clear();
        }

        private void Update()
        {
            _vm?.TickTimer(Time.unscaledDeltaTime);
        }

        private void HandleTimeout()
        {
            _vm.CompleteMinigame(_minigame.DidPlayerSucceed());
        }

        private void OnDataChanged(HazardData data)
        {
            _currentData = data;
            _outcomes.Clear();

            if (data == null) return;

            _hintTween = _hintCanvasGroup.FadeIn(FADE_DURATION, _hintTween);
            _resultTween?.Kill();
            _resultCanvasGroup.alpha = 0f;
            _resultCanvasGroup.gameObject.SetActive(false);
        }

        private void OnWarningTextChanged(string text)
        {
            if (_currentData == null) return;
            _header.Initialize(_currentData.Icon, text ?? string.Empty);
        }

        private void OnMinigamePrefabChanged(GameObject prefab)
        {
            if (_currentData == null) return;
            _minigame.Show(prefab, _currentData.MinigameConfig, _vm.QteInput, _vm.ThemeService,
                success => _vm.CompleteMinigame(success));
        }

        private void OnResultChanged(HazardResultState? result)
        {
            if (!result.HasValue) return;

            _minigame.Hide();
            _hintTween = _hintCanvasGroup.FadeOut(FADE_DURATION, _hintTween, deactivateOnComplete: false);
            _hintText.text = string.Empty;

            bool success = result.Value.Success;
            _resultTitleText.color = success ? _successColor : _failColor;

            _outcomes.Set(result.Value.Outcomes, _vm.ResourceIcons, _vm.ThemeService);
            _resultTween = _resultCanvasGroup.FadeIn(FADE_DURATION, _resultTween);
        }
    }
}
