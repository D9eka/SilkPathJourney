using Internal.Scripts.UI.Screen.Config;
using Internal.Scripts.UI.Screen.View;
using Internal.Scripts.UI.Screen.ViewModel;

namespace Internal.Scripts.UI.Factory
{
    public interface IScreenViewModelFactory
    {
        ScreenViewModelBase Create(ScreenId id, IScreenView view);
    }
}
