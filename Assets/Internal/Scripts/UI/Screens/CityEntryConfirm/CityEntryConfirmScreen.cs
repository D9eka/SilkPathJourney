using System;
using DG.Tweening;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using R3;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Internal.Scripts.UI.Screens.CityEntryConfirm
{
    public class CityEntryConfirmScreen : PopupScreen
    {
        [Header("Phase Containers")]
        [SerializeField] private CanvasGroup _previewCanvasGroup;
        [SerializeField] private CanvasGroup _resultCanvasGroup;
        [SerializeField] private RectTransform _previewRect;
        [SerializeField] private RectTransform _resultRect;

        [Header("Sub-views")]
        [SerializeField] private CityEntryPreviewView _previewView;
        [SerializeField] private CityEntryResultView _resultView;

        [Header("Resources")]
        [SerializeField] private ResourceIndicator _moneyIndicator;

        private CityEntryConfirmScreenViewModel _viewModel;
        private IDisposable _stateSubscription;
        private CityEntryPhase _currentPhase = CityEntryPhase.Preview;
        private CityEntryConfirmViewState _currentState;
        private Sequence _transitionSequence;

        public override void BindViewModel(IScreenViewModel viewModel)
        {
            base.BindViewModel(viewModel);
            _viewModel = viewModel as CityEntryConfirmScreenViewModel;
            SubscribeViewModel();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            SubscribeViewModel();

            _previewView.EnterClicked += HandleEnterClicked;
            _previewView.LeaveClicked += HandleLeaveClicked;
            _resultView.ContinueClicked += HandleContinueClicked;
            LocalizationSettings.SelectedLocaleChanged += HandleLocaleChanged;

            _previewCanvasGroup.alpha = 1f;
            _previewCanvasGroup.blocksRaycasts = true;
            _resultCanvasGroup.alpha = 0f;
            _resultCanvasGroup.blocksRaycasts = false;
            _previewRect.localScale = Vector3.one;
            _resultRect.localScale = Vector3.one;
            _currentPhase = CityEntryPhase.Preview;
        }

        protected override void OnDisable()
        {
            UnsubscribeViewModel();

            _previewView.EnterClicked -= HandleEnterClicked;
            _previewView.LeaveClicked -= HandleLeaveClicked;
            _resultView.ContinueClicked -= HandleContinueClicked;
            LocalizationSettings.SelectedLocaleChanged -= HandleLocaleChanged;

            KillTransition();
            base.OnDisable();
        }

        private void SubscribeViewModel()
        {
            if (_viewModel == null || _stateSubscription != null) return;
            _stateSubscription = _viewModel.State.Subscribe(ApplyState);
        }

        private void UnsubscribeViewModel()
        {
            _stateSubscription?.Dispose();
            _stateSubscription = null;
        }

        private void ApplyState(CityEntryConfirmViewState state)
        {
            if (state == null) return;
            _currentState = state;

            _mainHeader.SetText(state.CityName);

            if (state.Phase == CityEntryPhase.Preview)
            {
                _previewView.Apply(state);
                _moneyIndicator.SetResource(state.MoneyIcon, state.PlayerMoney);
            }
            else
            {
                _resultView.Apply(state);
            }

            if (state.Phase != _currentPhase)
            {
                _currentPhase = state.Phase;
                if (state.Phase == CityEntryPhase.Result)
                {
                    AnimateToResult();
                    AnimateMoneyChange(state);
                }
            }
        }

        private void AnimateMoneyChange(CityEntryConfirmViewState state)
        {
            int change = state.PlayerMoney - state.PlayerMoneyBefore;
            _moneyIndicator.AnimateValueChange(
                change, increaseIsPositive: true,
                from: state.PlayerMoneyBefore, to: state.PlayerMoney);
        }

        private void AnimateToResult()
        {
            KillTransition();

            _transitionSequence = DOTween.Sequence()
                .Join(_previewCanvasGroup.DOFade(0f, 0.2f))
                .Join(_previewRect.DOScale(0.92f, 0.2f).SetEase(Ease.InQuad))
                .AppendCallback(() =>
                {
                    _previewCanvasGroup.blocksRaycasts = false;
                    _resultCanvasGroup.blocksRaycasts = true;
                    _resultRect.localScale = Vector3.one * 0.92f;
                })
                .Join(_resultCanvasGroup.DOFade(1f, 0.25f))
                .Join(_resultRect.DOScale(1f, 0.25f).SetEase(Ease.OutBack))
                .SetLink(gameObject)
                .SetUpdate(true);
        }

        private void KillTransition()
        {
            if (_transitionSequence != null && _transitionSequence.IsActive())
                _transitionSequence.Kill();
            _transitionSequence = null;
        }

        private void HandleLocaleChanged(Locale _)
        {
            if (_currentState != null && _currentState.Phase == CityEntryPhase.Result)
                _resultView.Apply(_currentState);
        }

        private void HandleEnterClicked() => _viewModel?.ConfirmEntry();
        private void HandleLeaveClicked() => _viewModel?.Cancel();
        private void HandleContinueClicked() => _viewModel?.AcknowledgeResult();
    }
}
