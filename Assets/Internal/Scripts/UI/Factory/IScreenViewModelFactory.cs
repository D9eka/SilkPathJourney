using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Core.View;
using Internal.Scripts.UI.Screens.Core.ViewModel;

namespace Internal.Scripts.UI.Factory
{
    public interface IScreenViewModelFactory
    {
        ScreenViewModelBase Create(ScreenId id, IScreenView view);
    }
}
