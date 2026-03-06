using UnityEngine.Localization;

namespace Internal.Scripts.UI.Screens.Trader
{
    public readonly struct ProfileEntry
    {
        public readonly LocalizedString Header;
        public readonly string Content;

        public ProfileEntry(LocalizedString header, string content)
        {
            Header = header;
            Content = content;
        }
    }
}
