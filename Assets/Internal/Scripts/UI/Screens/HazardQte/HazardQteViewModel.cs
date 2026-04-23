using System;
using System.Collections.Generic;
using DG.Tweening;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Outcomes;
using Internal.Scripts.Input;
using Internal.Scripts.Travel.Hazards;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Localization.Generated;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.StackService;
using Internal.Scripts.UI.Theme;
using Internal.Scripts.World.State;
using R3;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Internal.Scripts.UI.Screens.HazardQte
{
    public sealed class HazardQteViewModel : ScreenViewModelBase
    {
        private const float ResultCloseDelay = 1.5f;

        private readonly IQteInput _qteInput;
        private readonly OutcomeApplier _outcomeApplier;
        private readonly GameClock _gameClock;
        private readonly ScreenStackService _screenStackService;
        private readonly UiThemeService _themeService;
        private readonly ResourceIconCatalog _resourceIcons;

        private readonly ReactiveProperty<HazardData> _data = new(null);
        private readonly ReactiveProperty<GameObject> _minigamePrefab = new(null);
        private readonly ReactiveProperty<HazardResultState?> _result = new(null);
        private readonly ReactiveProperty<string> _warningText = new(string.Empty);
        private readonly ReactiveProperty<string> _hintText = new(string.Empty);
        private readonly ReactiveProperty<float> _timeRemaining = new(0f);
        private readonly ReactiveProperty<string> _resultTitle = new(string.Empty);

        private Tween _closeTween;
        private bool _timerActive;

        public event Action OnTimeout;

        public override ScreenId Id => ScreenId.HazardQte;

        public Observable<HazardData> Data => _data;
        public Observable<GameObject> MinigamePrefab => _minigamePrefab;
        public Observable<HazardResultState?> Result => _result;
        public Observable<string> WarningText => _warningText;
        public Observable<string> HintText => _hintText;
        public Observable<float> TimeRemaining => _timeRemaining;
        public Observable<string> ResultTitle => _resultTitle;
        public float TimeLimit => _data.Value?.TimeLimit ?? 0f;
        public IQteInput QteInput => _qteInput;
        public UiThemeService ThemeService => _themeService;
        public ResourceIconCatalog ResourceIcons => _resourceIcons;

        public HazardQteViewModel(
            IQteInput qteInput,
            OutcomeApplier outcomeApplier,
            GameClock gameClock,
            ScreenStackService screenStackService,
            UiThemeService themeService,
            ResourceIconCatalog resourceIcons)
        {
            _qteInput = qteInput;
            _outcomeApplier = outcomeApplier;
            _gameClock = gameClock;
            _screenStackService = screenStackService;
            _themeService = themeService;
            _resourceIcons = resourceIcons;
        }

        private string ResolveResultTitle(bool success)
        {
            string key = success ? LocUI.UI_HazardQte_Result_Success : LocUI.UI_HazardQte_Result_Fail;
            return LocalizationService.Resolve(LocUI.Table, key);
        }

        private void UpdateResultTitle()
        {
            if (_result.Value.HasValue)
                _resultTitle.Value = ResolveResultTitle(_result.Value.Value.Success);
        }

        protected override void OnOpen(object args)
        {
            if (args is not HazardData hazard)
            {
                Debug.LogError("[SPJ Hazards] HazardQteViewModel opened without HazardData");
                _screenStackService.Close(ScreenId.HazardQte);
                return;
            }

            _result.Value = null;
            _data.Value = hazard;
            if (hazard.MinigamePrefab == null)
            {
                Debug.LogError($"[HazardQteViewModel] MinigamePrefab not set on {hazard.name}");
                _screenStackService.Close(ScreenId.HazardQte);
                return;
            }
            _minigamePrefab.Value = hazard.MinigamePrefab;
            _timeRemaining.Value = hazard.TimeLimit;
            _timerActive = true;
            _gameClock.Pause();

            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
            RefreshLocalizedText();
        }

        protected override void OnClose()
        {
            _timerActive = false;
            _timeRemaining.Value = 0f;
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
            _closeTween?.Kill();
            _closeTween = null;
            _gameClock.Resume();
            _data.Value = null;
            _minigamePrefab.Value = null;
            _result.Value = null;
            _warningText.Value = string.Empty;
            _hintText.Value = string.Empty;
            _resultTitle.Value = string.Empty;
        }

        private void OnLocaleChanged(Locale _) => RefreshLocalizedText();

        private void RefreshLocalizedText()
        {
            HazardData hazard = _data.Value;
            if (hazard == null)
            {
                _warningText.Value = string.Empty;
                _hintText.Value = string.Empty;
                return;
            }

            _warningText.Value = LocalizationService.ResolveString(
                hazard.WarningText, hazard.Type.ToString(), "HazardWarning");

            _hintText.Value = hazard.MinigameConfig?.Hint?.GetLocalizedString() ?? string.Empty;
            UpdateResultTitle();
        }

        public void TickTimer(float dt)
        {
            if (!_timerActive || _result.Value.HasValue) return;
            float next = Mathf.Max(0f, _timeRemaining.Value - dt);
            _timeRemaining.Value = next;
            if (next <= 0f)
            {
                _timerActive = false;
                OnTimeout?.Invoke();
            }
        }

        public void CompleteMinigame(bool success)
        {
            _timerActive = false;
            if (_result.Value.HasValue)
                return;

            HazardData hazard = _data.Value;
            if (hazard == null)
                return;

            List<EventOutcomeEntry> outcomes = success ? hazard.SuccessOutcomes : hazard.FailOutcomes;
            if (outcomes != null)
            {
                foreach (EventOutcomeEntry entry in outcomes)
                    _outcomeApplier.Apply(entry);
            }

            _result.Value = new HazardResultState(success, outcomes);
            UpdateResultTitle();

            _closeTween = DOVirtual.DelayedCall(ResultCloseDelay, () =>
            {
                _screenStackService.Close(ScreenId.HazardQte);
            }, ignoreTimeScale: true);
        }
    }
}
