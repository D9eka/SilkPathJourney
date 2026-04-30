using Internal.Scripts.UI.Localization;

namespace Internal.Scripts.UI.Screens.Core.Tabs
{
    public interface ITabView
    {
        int TabId { get; }
        void BindLocalization(LocalizationService localization);
        void DisposeBinding();
        void SetActive(bool isActive);
        void Apply(object state);
    }
}
