namespace Internal.Scripts.UI.Tooltip
{
    /// <summary>
    /// Interface for objects that can provide tooltip data (title and description).
    /// </summary>
    public interface ITooltipDataProvider
    {
        /// <summary>
        /// Gets the tooltip title (usually object name).
        /// </summary>
        string GetTooltipTitle();

        /// <summary>
        /// Gets the tooltip description (detailed information).
        /// </summary>
        string GetTooltipDescription();
    }
}
