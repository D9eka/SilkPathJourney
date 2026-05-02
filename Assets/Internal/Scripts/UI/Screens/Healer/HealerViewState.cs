using System.Collections.Generic;

namespace Internal.Scripts.UI.Screens.Healer
{
    public sealed class HealerViewState
    {
        public readonly IReadOnlyList<HealerEntry> Entries;
        public readonly int PlayerMoney;
        public readonly bool AnyInjured;

        public HealerViewState(IReadOnlyList<HealerEntry> entries, int playerMoney, bool anyInjured)
        {
            Entries = entries;
            PlayerMoney = playerMoney;
            AnyInjured = anyInjured;
        }
    }

    public readonly struct HealerEntry
    {
        public readonly int CompanionIndex;
        public readonly string Name;
        public readonly string QualityKey;
        public readonly string StatusText;
        public readonly string ButtonText;
        public readonly bool IsInjured;
        public readonly int HealCost;
        public readonly bool CanAfford;

        public HealerEntry(int companionIndex, string name, string qualityKey,
            string statusText, string buttonText, bool isInjured, int healCost, bool canAfford)
        {
            CompanionIndex = companionIndex;
            Name = name;
            QualityKey = qualityKey;
            StatusText = statusText;
            ButtonText = buttonText;
            IsInjured = isInjured;
            HealCost = healCost;
            CanAfford = canAfford;
        }
    }
}
