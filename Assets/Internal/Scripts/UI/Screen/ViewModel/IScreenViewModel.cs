using Internal.Scripts.UI.Screen.Config;

namespace Internal.Scripts.UI.Screen.ViewModel
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
