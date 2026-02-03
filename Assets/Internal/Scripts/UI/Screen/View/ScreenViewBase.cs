using System;
using UnityEngine;

namespace Internal.Scripts.UI.Screen.View
{
    public abstract class ScreenViewBase : MonoBehaviour, IScreenView, IScreenCloseRequestSource
    {
        public event Action CloseRequested;

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
