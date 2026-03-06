namespace Internal.Scripts.UI.Localization.Args
{
    public class SpriteLocArg : ILocArg
    {
        public string Key { get; }
        private readonly string _spriteName;

        public SpriteLocArg(string key, string spriteName)
        {
            Key = key;
            _spriteName = spriteName;
        }

        public string ToMarkup() => $"<sprite name=\"{_spriteName}\">";
    }
}
