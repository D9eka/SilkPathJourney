using Internal.Scripts.UI.Screen.Config;
using Internal.Scripts.UI.Screen.View;

namespace Internal.Scripts.UI.Screen.ViewModel
{
    public abstract class ScreenViewModelBase : IScreenViewModel
    {
        protected ScreenViewModelBase(IScreenView view)
        {
            View = view;
        }

        protected IScreenView View { get; }

        public abstract ScreenId Id { get; }
        public bool IsOpen { get; private set; }

        public void Open(object args)
        {
            IsOpen = true;
            View.Show();
            OnOpen(args);
        }

        public void Close()
        {
            OnClose();
            View.Hide();
            IsOpen = false;
        }

        public virtual void OnFocusGained()
        {
        }

        public virtual void OnFocusLost()
        {
        }

        protected abstract void OnOpen(object args);
        protected abstract void OnClose();
    }
}
