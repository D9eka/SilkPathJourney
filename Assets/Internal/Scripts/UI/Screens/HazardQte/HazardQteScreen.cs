using System;
using System.Collections.Generic;
using DG.Tweening;
using Internal.Scripts.Travel.Hazards;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Screens.Core.View;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.HazardQte
{
    public sealed class HazardQteScreen : ScreenViewBase
    {
        private const float FadeDuration = 0.3f;

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

        private HazardQteViewModel _vm;
        private HazardTimerView _timer;
        private HazardOutcomesView _outcomes;
        private HazardMinigameSlot _minigame;

        private HazardData _currentData;
        private Tween _hintTween;
        private Tween _resultTween;
        private readonly List<IDisposable> _subs = new();

        protected override void Awake()
        {
            base.Awake();
            _timer = new HazardTimerView(_timerBar, _timerText);
            _outcomes = new HazardOutcomesView(_resultOutcomesContainer, _iconLabelPrefab);
            _minigame = new HazardMinigameSlot(_minigameContainer, _minigameLayoutElement);
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

            _hintTween = _hintCanvasGroup.FadeIn(FadeDuration, _hintTween);
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
            _minigame.Show(prefab, _currentData.InputConfig, _vm.QteInput, _vm.ThemeService,
                success => _vm.CompleteMinigame(success));
        }

        private void OnResultChanged(HazardResultState? result)
        {
            if (!result.HasValue) return;

            _minigame.Hide();
            _hintTween = _hintCanvasGroup.FadeOut(FadeDuration, _hintTween, deactivateOnComplete: false);
            _hintText.text = string.Empty;

            bool success = result.Value.Success;
            _resultTitleText.text = _vm.ResolveResultTitle(success);
            _resultTitleText.color = success ? _successColor : _failColor;

            _outcomes.Set(result.Value.Outcomes, _vm.ResourceIcons, _vm.ThemeService);
            _resultTween = _resultCanvasGroup.FadeIn(FadeDuration, _resultTween);
        }
    }
}
