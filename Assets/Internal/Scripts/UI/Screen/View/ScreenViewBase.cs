using System;
using Internal.Scripts.UI.Screen.ViewModel;
using UnityEngine;

namespace Internal.Scripts.UI.Screen.View
{
    public abstract class ScreenViewBase : MonoBehaviour, IScreenView, IScreenCloseRequestSource, IScreenViewModelBinder
    {
        public event Action CloseRequested;

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
