using System;
using Internal.Scripts.Items;
using Internal.Scripts.Meta;
using Internal.Scripts.Player.Languages.Generated;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;

namespace Internal.Scripts.UI.Screens.Legacy.Tabs.LifetimeStats
{
    public sealed class LegacyLifetimeTabViewModel
    {
        private const string MissingValueFallback = "—";

        private readonly PersistentProgressService _persistent;
        private readonly ItemCatalog _itemCatalog;

        public LegacyLifetimeTabViewModel(PersistentProgressService persistent, ItemCatalog itemCatalog)
        {
            _persistent = persistent;
            _itemCatalog = itemCatalog;
        }

        public LifetimeStatsViewState BuildState()
        {
            LifetimeStatsData raw = _persistent.Lifetime;
            return new LifetimeStatsViewState(raw,
                BuildItemQuantityText(raw.BestDealItemId, raw.BestDealProfit, "+"),
                BuildItemQuantityText(raw.FavoriteItemId, raw.FavoriteItemCount, "×"),
                BuildItemQuantityText(raw.MostExpensiveItemId, raw.MostExpensivePrice, "+"),
                BuildLanguageText(raw.TopLanguageId));
        }

        private string BuildItemQuantityText(string itemId, int value, string sign)
        {
            if (string.IsNullOrEmpty(itemId)) return MissingValueFallback;
            string itemName = _itemCatalog != null ? _itemCatalog.ResolveItemName(itemId) : itemId;
            return $"{itemName} {sign}{value}";
        }

        private static string BuildLanguageText(string languageId)
        {
            if (string.IsNullOrEmpty(languageId)) return MissingValueFallback;
            if (!Enum.TryParse(languageId, out LanguageType type) || type == LanguageType.None)
                return languageId;
            var localized = SkillView.ResolveLanguageName(type);
            return LocalizationService.ResolveString(localized, languageId, $"Language.{languageId}");
        }
    }
}
