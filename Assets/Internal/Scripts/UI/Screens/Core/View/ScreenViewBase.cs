using System;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.Core.View
{
    public abstract class ScreenViewBase : MonoBehaviour, IScreenView, IScreenCloseRequestSource, IScreenViewModelBinder, ILocalizationConsumer
    {
        public event Action CloseRequested;

        protected LocalizationService Localization { get; private set; }

        public void SetLocalization(LocalizationService service)
        {
            Localization = service;
            OnLocalizationReady();
        }

        protected virtual void OnLocalizationReady() { }

        public virtual void BindViewModel(IScreenViewModel viewModel)
        {
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }

        protected void RaiseCloseRequested()
        {
            CloseRequested?.Invoke();
        }
    }
}
