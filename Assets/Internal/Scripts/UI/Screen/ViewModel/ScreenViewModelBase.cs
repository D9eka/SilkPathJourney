using Internal.Scripts.UI.Screen.Config;

namespace Internal.Scripts.UI.Screen.ViewModel
{
    public abstract class ScreenViewModelBase : IScreenViewModel
    {
        public abstract ScreenId Id { get; }
        public bool IsOpen { get; private set; }

        public void Open(object args)
        {
            IsOpen = true;
            OnOpen(args);
        }

        public void Close()
        {
            OnClose();
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
