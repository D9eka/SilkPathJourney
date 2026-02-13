using Internal.Scripts.UI.Screens.Core.Config;

namespace Internal.Scripts.UI.Screens.Core.ViewModel
{
    public interface IScreenViewModel
    {
        ScreenId Id { get; }
        bool IsOpen { get; }
        void Open(object args);
        void Close();
        void OnFocusGained();
        void OnFocusLost();
    }
}
