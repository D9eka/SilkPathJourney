using System.Collections.Generic;
using Internal.Scripts.Meta.Unlocks;

namespace Internal.Scripts.UI.Screens.Legacy.Tabs.Shop
{
    public sealed class UnlockEntryData
    {
        public readonly string Id;
        public readonly string Name;
        public readonly string Description;
        public readonly int Cost;
        public readonly bool IsUnlocked;
        public readonly UnlockCategory Category;

        public UnlockEntryData(string id, string name, string description, int cost, bool isUnlocked, UnlockCategory category)
        {
            Id = id;
            Name = name;
            Description = description;
            Cost = cost;
            IsUnlocked = isUnlocked;
            Category = category;
        }
    }

    public sealed class LegacyShopTabState
    {
        public readonly IReadOnlyList<UnlockEntryData> Unlocks;
        public readonly int LegacyPointsTotal;

        public LegacyShopTabState(IReadOnlyList<UnlockEntryData> unlocks, int legacyPointsTotal)
        {
            Unlocks = unlocks;
            LegacyPointsTotal = legacyPointsTotal;
        }
    }
}
