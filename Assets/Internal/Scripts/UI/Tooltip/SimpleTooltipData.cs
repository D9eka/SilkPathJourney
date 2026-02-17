namespace Internal.Scripts.UI.Tooltip
{
    /// <summary>
    /// Simple implementation of ITooltipDataProvider for dynamic tooltips without ScriptableObject.
    /// </summary>
    public class SimpleTooltipData : ITooltipDataProvider
    {
        private readonly string _title;
        private readonly string _description;

        public SimpleTooltipData(string title, string description)
        {
            _title = title;
            _description = description;
        }

        public string GetTooltipTitle() => _title;
        public string GetTooltipDescription() => _description;
    }
}
