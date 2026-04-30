using UnityEngine.Localization;

namespace Internal.Scripts.UI.Screens.Core.Tabs
{
    public interface ITabBinding
    {
        int TabId { get; }
        TabButton Button { get; }
        ITabView View { get; }
        LocalizedString Label { get; }
    }
}
