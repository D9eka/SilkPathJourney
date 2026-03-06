namespace Internal.Scripts.UI.Localization.Args
{
    public interface ILocArg
    {
        string Key { get; }
        string ToMarkup();
    }
}
