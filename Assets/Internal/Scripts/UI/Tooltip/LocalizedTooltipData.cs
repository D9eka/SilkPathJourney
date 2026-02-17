using Internal.Scripts.UI.Localization;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.Tooltip
{
    /// <summary>
    /// Base class for ScriptableObject data that provides localized tooltip information.
    /// Provides default implementation using LocalizationHelper.
    /// </summary>
    public abstract class LocalizedTooltipData : ScriptableObject, ITooltipDataProvider
    {
        /// <summary>
        /// The localized name/title of this data object.
        /// </summary>
        protected abstract LocalizedString TooltipName { get; }

        /// <summary>
        /// The localized description of this data object.
        /// </summary>
        protected abstract LocalizedString TooltipDescription { get; }

        /// <summary>
        /// The ID used as fallback if localization fails.
        /// </summary>
        protected abstract string TooltipId { get; }

        /// <summary>
        /// Context string for localization debugging (e.g., "BuildingData", "CityData").
        /// </summary>
        protected abstract string TooltipContext { get; }

        /// <summary>
        /// Fallback description if localization returns empty.
        /// Default: returns empty string. Override to provide game-specific fallback.
        /// </summary>
        protected virtual string FallbackDescription => string.Empty;

        public virtual string GetTooltipTitle()
        {
            return LocalizationHelper.ResolveString(
                TooltipName,
                TooltipId,
                $"{TooltipContext}Name");
        }

        public virtual string GetTooltipDescription()
        {
            string desc = LocalizationHelper.ResolveString(
                TooltipDescription,
                string.Empty,
                $"{TooltipContext}Description");

            if (string.IsNullOrWhiteSpace(desc))
                desc = FallbackDescription;

            return desc;
        }
    }
}
