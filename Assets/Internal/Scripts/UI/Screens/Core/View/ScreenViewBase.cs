using System;
using DG.Tweening;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.Core.View
{
    public abstract class ScreenViewBase : MonoBehaviour, IScreenView, IScreenCloseRequestSource, IScreenViewModelBinder, ILocalizationConsumer
    {
        private const float SHOW_DURATION = 0.3f;
        private const float HIDE_DURATION = 0.3f;

        [Header("Animation")]
        [SerializeField] private RectTransform[] _animationRoots;
        [SerializeField] private CanvasGroup _canvasGroup;

        public event Action CloseRequested;
        public event Action HideCompleted;

        private Vector3[] _originalScales;
        private Sequence _sequence;

        protected LocalizationService Localization { get; private set; }

        public void SetLocalization(LocalizationService service)
        {
            Localization = service;
            OnLocalizationReady();
        }

        protected virtual void OnLocalizationReady() { }

        protected virtual void Awake()
        {
            if (_animationRoots != null && _animationRoots.Length > 0)
            {
                _originalScales = new Vector3[_animationRoots.Length];
                for (int i = 0; i < _animationRoots.Length; i++)
                    _originalScales[i] = _animationRoots[i].localScale;
            }
        }

        public virtual void BindViewModel(IScreenViewModel viewModel)
        {
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);

            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }

            if (_animationRoots == null || _animationRoots.Length == 0)
                return;

            KillSequence();

            _sequence = DOTween.Sequence();
            for (int i = 0; i < _animationRoots.Length; i++)
            {
                _animationRoots[i].localScale = Vector3.zero;
                _sequence.Join(_animationRoots[i].DOScale(_originalScales[i], SHOW_DURATION).SetEase(Ease.OutCubic));
            }
            _sequence.SetLink(gameObject).SetUpdate(true);
        }

        public virtual void Hide()
        {
            if (_animationRoots == null || _animationRoots.Length == 0)
            {
                gameObject.SetActive(false);
                HideCompleted?.Invoke();
                return;
            }

            KillSequence();

            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }

            _sequence = DOTween.Sequence();
            for (int i = 0; i < _animationRoots.Length; i++)
                _sequence.Join(_animationRoots[i].DOScale(Vector3.zero, HIDE_DURATION).SetEase(Ease.InCubic));

            _sequence
                .SetLink(gameObject)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                    HideCompleted?.Invoke();
                });
        }

        protected void RaiseCloseRequested()
        {
            CloseRequested?.Invoke();
        }

        private void KillSequence()
        {
            if (_sequence != null && _sequence.IsActive())
                _sequence.Kill();
            _sequence = null;
        }

        protected virtual void OnDestroy()
        {
            KillSequence();
        }
    }
}
