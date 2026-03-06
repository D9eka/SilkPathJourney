namespace Internal.Scripts.UI.Localization.Args
{
    public class TextLocArg : ILocArg
    {
        public string Key { get; }
        private readonly string _text;

        public TextLocArg(string key, string text)
        {
            Key = key;
            _text = text;
        }

        public string ToMarkup() => _text;
    }
}
